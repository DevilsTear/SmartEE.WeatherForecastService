using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartEE.WeatherForecast.Service.Hubs;
using SmartEE.WeatherForecast.Service.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SmartEE.WeatherForecast.Service.Controllers
{
    /// <summary>
    /// Weather Forecast Controller Class
    /// </summary>
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        /// <summary>
        /// Controller initializer
        /// </summary>
        /// <param name="logger">Logger(SeriLog) injection param</param>
        /// <param name="hubContext">Hub(SignalR) injection param</param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Fetch Weather Forecast By City Name
        /// </summary>
        /// <param name="cityName">City Name to be queried</param>
        /// <returns>Ok, NotFound or BadRequest Http Results..</returns>
        [SwaggerOperation(Tags = new[] { "Fetch Weather Forecast By City Name" })]
        [HttpGet("api/v1/[controller]/{cityName}")]
        public async Task<ActionResult> GetWeatherForecastByCityName(string cityName)
        {
            if (cityName.IsNullTypeOrEmpty())
                return BadRequest("You have to post a city name to get weather forecast results..");

            Stopwatch sw = Stopwatch.StartNew();
            var location = LocationService.FetchLocationInfo(cityName);
            if (location != null)
            {
                var weatherForecast = ForecastService.FetchForecastInfo(location.CityId, location.Lat, location.Lon);
                if (weatherForecast != null)
                {
                    weatherForecast.CityName = location.CityName;
                    weatherForecast.LocationQueryElapsedMilliseconds = location.QueryElapsedMilliseconds;
                    sw.Stop();
                    weatherForecast.MethodQueryElapsedMilliseconds = sw.ElapsedMilliseconds;
                }

                await _hubContext.Clients.All.SendAsync("LastForecastQuery", weatherForecast);

                return Ok(weatherForecast);
            }

            return NotFound($"Could not find any records with city name:{cityName}");
        }
    }
}
