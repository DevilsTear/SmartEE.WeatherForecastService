using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SmartEE.WeatherForecast.Common.Validators;
using SmartEE.WeatherForecast.Service.Hubs;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;

namespace SmartEE.WeatherForecast.Service
{
#pragma warning disable 1591
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();


            services.AddRazorPages();
            services.AddSignalR();
            services.AddControllers();
            services.AddSingleton<IObjectModelValidator>(new NullObjectModelValidator());
            services.AddMvc(options =>
            {
                options.InputFormatters.Insert(0, new BinaryInputFormatter());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SmartEE WeatherForecast Service API", Version = "v1" });
                x.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SmartEE WeatherForecast Service API", Version = "v2" });

                var sc = new OpenApiSecurityScheme
                {
                    Description = "X-API-KEY Authorization header using the API-Key.",
                    In = ParameterLocation.Header,
                    Name = "X-API-KEY",
                    Type = SecuritySchemeType.ApiKey
                };

                x.AddSecurityDefinition("apikey", sc);

                x.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "apikey" }
                        }, new List<string>()
                    }
                });

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "SmartEE.WeatherForecast.Service.xml");
                x.IncludeXmlComments(filePath);

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger(option =>
            {
                option.RouteTemplate = "doc/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint("v1/swagger.json", "SmartEE WeatherForecast Service API V1");
                option.SwaggerEndpoint("v2/swagger.json", "SmartEE WeatherForecast Service API V2");
                option.RoutePrefix = "doc";
            });

            //app.UseIpRateLimiting();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                var _cache = app.ApplicationServices.GetService<IMemoryCache>();



                if (context.Request.Headers.ContainsKey("X-KEY") || 1 == 1)
                {
                    if (_cache.TryGetValue("A:" + context.Request.Headers["X-KEY"].ToString(), out string userid))
                    {

                        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "test"), new Claim(ClaimTypes.Name, "testname") };
                        var userIdentity = new ClaimsIdentity(claims, "NonEmptyAuthType");
                        context.User = new ClaimsPrincipal(userIdentity);
                        context.Items["UserID"] = userid;

                        await next.Invoke();
                    }
                    else
                    {
                        //Check DB
                        _cache.Set<string>("A:" + context.Request.Headers["X-KEY"].ToString(), "test");
                        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "test"), new Claim(ClaimTypes.Name, "testname") };
                        var userIdentity = new ClaimsIdentity(claims, "NonEmptyAuthType");
                        context.User = new ClaimsPrincipal(userIdentity);
                        context.Items["UserID"] = "test";

                        await next.Invoke();

                        //if not exists in db
                        //context.Response.StatusCode = 401;
                        //await context.Response.WriteAsync("Unauthorized");
                    }
                }
                else
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                }

            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/notifications");
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("WEATHER FORECAST SERVICE!");
                });
            });
        }
    }
#pragma warning restore 1591
}
