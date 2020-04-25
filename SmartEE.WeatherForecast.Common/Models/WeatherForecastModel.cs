using System;

namespace SmartEE.WeatherForecast.Common.Models.Forecast
{
    public class WeatherForecastModel
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public DateTime Date => TS.ToEpoch();

        public int DailyMinTemperature { get; set; }
        public int DailyMaxTemperature { get; set; }
        public int WeeklyMinTemperature { get; set; }
        public int WeeklyMaxTemperature { get; set; }

        //public int DailyMinTemperatureF => 32 + (int)(DailyMinTemperatureC / 0.5556);

        public long TS { get; set; }

        public long ForecastQueryElapsedMilliseconds { get; set; }
        public long LocationQueryElapsedMilliseconds { get; set; }
        public long MethodQueryElapsedMilliseconds { get; set; }
    }
}
