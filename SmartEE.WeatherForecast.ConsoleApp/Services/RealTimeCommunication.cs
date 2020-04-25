using ConsoleTables;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using SmartEE.WeatherForecast.Common.Models.Forecast;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartEE.WeatherForecast.ConsoleApp.Services
{
    /// <summary>
    /// Real time communication class
    /// </summary>
    public static class RealTimeCommunication
    {
        /// <summary>
        /// Real time communication class initialization processes..
        /// </summary>
        public static void Init()
        {
            Task.Factory.StartNew(() => SignalRConnect());
        }

        /// <summary>
        /// SignalR connection event declarations..
        /// </summary>
        public static async void SignalRConnect()
        {
            try
            {
                var connection = new HubConnectionBuilder().WithUrl("http://localhost:5000/notifications", options =>
                {
                    //Hub connection security support..
                    options.Headers["X-KEY"] = "";
                }).WithAutomaticReconnect().Build();

                connection.On<List<WeatherForecastModel>>("CurrentDayForecastQueries", (forecasts) =>
                {
                    Console.WriteLine($"Last forecast queries : {DateTime.Now.ToString("yyyy-MM-dd")}");
                    ShowWeatherForecast(forecasts);
                });

                connection.On<WeatherForecastModel>("LastForecastQuery", (forecast) =>
                 {
                     Console.WriteLine($"New forecast received : {forecast.Date.ToString("yyyy-MM-dd HH:mm")}");
                     ShowWeatherForecast(new List<WeatherForecastModel> { forecast });
                 });

                await connection.StartAsync();
            }

            catch (Exception ex)
            {
                Log.Error("SignalR error", ex.ToString());
            }
        }

        /// <summary>
        /// Prints received WeatherForecastModel list to the console..
        /// </summary>
        /// <param name="forecasts">List of WeatherForecastModel</param>
        private static void ShowWeatherForecast(List<WeatherForecastModel> forecasts)
        {
            var consoleTable = new ConsoleTable("City Name", "Daily Min", "Daily Max", "Weekly Min", "Weekly Max", "Forecast WS Elapsed ms", "Location WS Elapsed ms", "Total Elapsed ms");
            foreach (var forecast in forecasts)
            {
                consoleTable.AddRow(forecast.CityName, forecast.DailyMinTemperature + "Cº", forecast.DailyMaxTemperature + "Cº", forecast.WeeklyMinTemperature + "Cº", forecast.WeeklyMaxTemperature + "Cº", forecast.ForecastQueryElapsedMilliseconds + "ms", forecast.LocationQueryElapsedMilliseconds + "ms", forecast.MethodQueryElapsedMilliseconds + "ms");
            }

            consoleTable.Write();
        }
    }
}
