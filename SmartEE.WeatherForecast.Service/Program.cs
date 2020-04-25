using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.RollingFileAlternate;
using SmartEE.WeatherForecast.Service.Services;
using System;
using System.Text;
using System.Threading;

namespace SmartEE.WeatherForecast.Service
{
#pragma warning disable 1591
    public class Program
    {
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        public static void Main(string[] args)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Log.Logger = new LoggerConfiguration().WriteTo.Async(a => a.RollingFileAlternate(".\\logs", fileSizeLimitBytes: 1024 * 1024 * 10, outputTemplate: "|L|{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")).CreateLogger();
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Shutdown();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Shutdown();
                eventArgs.Cancel = true;
            };

            Thread threadweb = new Thread(() => StartWebServer(args));
            threadweb.Start();

            Log.Information("Starting service");

            LocationService.Init();
            ForecastService.Init();
        }

        #region web server
        /// <summary>
        /// Application web server initialization
        /// </summary>
        /// <param name="args">Args to be exposed when the application starts</param>
        private static void StartWebServer(string[] args)
        {
            //RunHost().GetAwaiter().GetResult();
            CreateHostBuilder(args).Build().Run();
        }
        #endregion

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        /// <summary>
        /// Application shut down processes
        /// </summary>
        private static void Shutdown()
        {
            cts.Cancel();
            // dispose all timers and threads in other classes too
            Log.Information("Shutting Down Service");
            Log.CloseAndFlush();
        }
    }
#pragma warning restore 1591
}
