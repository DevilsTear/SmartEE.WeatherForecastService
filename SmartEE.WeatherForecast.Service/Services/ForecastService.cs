using Serilog;
using SmartEE.WeatherForecast.Common.Models.Forecast;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace SmartEE.WeatherForecast.Service.Services
{
    internal class ForecastService
    {
        static ConcurrentDictionary<long, ConcurrentDictionary<int, WeatherForecastModel>> methodRequests = new ConcurrentDictionary<long, ConcurrentDictionary<int, WeatherForecastModel>>();

        static string _ForecastWSURL = "";//"https://api.darksky.net/forecast/[TOKEN]/[LATITUDE],[LONGITUDE]?lang=tr&units=si&exclude=minutely,hourly,alerts,flags";
        /// <summary>
        /// Location service initialization
        /// </summary>
        /// <param name="wsToken">Forecast service token</param>
        internal static void Init(string wsToken = null)
        {
            _ForecastWSURL = $"https://api.darksky.net/forecast/{(wsToken ?? "f3146e0fc78b4930d41a60703c08e2ae")}/[LATITUDE],[LONGITUDE]?lang=tr&units=si&exclude=minutely,hourly,alerts,flags";
            try
            {
                var res = DBlite.ExecuteSQL("CREATE TABLE IF NOT EXISTS Forecast (Id INTEGER PRIMARY KEY AUTOINCREMENT, CityId INTEGER, DailyMinTemperature TEXT, DailyMaxTemperature TEXT, WeeklyMinTemperature TEXT, WeeklyMaxTemperature TEXT, TS INTEGER, QueryElapsedMilliseconds INTEGER); CREATE INDEX IF NOT EXISTS ndx_TS ON Forecast(TS); CREATE UNIQUE INDEX IF NOT EXISTS ndx_CityId_TS ON Forecast(CityId,TS);");
                if (res == -1)
                {
                    Log.Error("Forecast table create error");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Forecast table create error", ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Fetches forecast info by city name from related remote api
        /// </summary>
        /// <param name="cityId">City Id</param>
        /// <param name="latitude">City coordinates latitude value</param>
        /// <param name="longitude">City coordinates longitude value</param>
        /// <returns>ForecastModel</returns>
        internal static WeatherForecastModel FetchForecastInfo(int cityId, string latitude, string longitude)
        {
            WeatherForecastModel weatherForecast = null;
            var currrentTSHour = Convert.ToInt64((Math.Floor((decimal)(DateTime.Now.ToEpoch()) / 3600) * 3600));
            if (!methodRequests.ContainsKey(currrentTSHour)) // eger guncel saat dict icinde yoksa ekleme ve onceki saatle islem yapma bolumu
            {
                methodRequests[currrentTSHour] = new ConcurrentDictionary<int, WeatherForecastModel>();
            }

            if (methodRequests[currrentTSHour].ContainsKey(cityId))
            {
                return methodRequests[currrentTSHour].Where(r => r.Key == cityId).Select(r => r.Value).FirstOrDefault();
            }

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = httpClient.GetAsync(_ForecastWSURL.Replace("[LATITUDE]", latitude ?? "41.0096334").Replace("[LONGITUDE]", longitude ?? "28.9651646")).Result)
                    {
                        using (var content = response.Content)
                        {
                            //get the json result from location api
                            var result = content.ReadAsStringAsync().Result;
                            var forecastModel = Deserialize.FromJson(result);
                            if (forecastModel != null)
                            {
                                var curDayForecast = forecastModel.Daily.Data.OrderBy(r => r.Time).FirstOrDefault();
                                var ts = curDayForecast.Time;
                                var dailyMinTemperature = (int)Math.Round(curDayForecast.TemperatureMin, 0);
                                var dailyMaxTemperature = (int)Math.Round(curDayForecast.TemperatureMax, 0);
                                var weeklyMinTemperature = (int)Math.Round(forecastModel.Daily.Data.OrderBy(r => r.TemperatureMin).FirstOrDefault().TemperatureMin, 0);
                                var weeklyMaxTemperature = (int)Math.Round(forecastModel.Daily.Data.OrderByDescending(r => r.TemperatureMax).FirstOrDefault().TemperatureMax, 0);
                                sw.Stop();
                                weatherForecast = new WeatherForecastModel
                                {
                                    CityId = cityId,
                                    TS = ts,
                                    DailyMinTemperature = dailyMinTemperature,
                                    DailyMaxTemperature = dailyMaxTemperature,
                                    WeeklyMinTemperature = weeklyMinTemperature,
                                    WeeklyMaxTemperature = weeklyMaxTemperature,
                                    ForecastQueryElapsedMilliseconds = sw.ElapsedMilliseconds,
                                };

                                //max 50 records
                                if(methodRequests.Count >= 50)
                                    methodRequests[currrentTSHour] = new ConcurrentDictionary<int, WeatherForecastModel>();

                                //if cityId does not exist
                                if (!methodRequests[currrentTSHour].ContainsKey(cityId))
                                    methodRequests[currrentTSHour].TryAdd(cityId, weatherForecast);

                                //This is more simple to check if the record is available or not then if record not available save etc..
                                var res = DBlite.ExecuteSQL($"INSERT OR IGNORE INTO Forecast (CityId, DailyMinTemperature, DailyMaxTemperature, WeeklyMinTemperature, WeeklyMaxTemperature, TS, QueryElapsedMilliseconds) VALUES (@CityId, @DailyMinTemperature, @DailyMaxTemperature, @WeeklyMinTemperature, @WeeklyMaxTemperature, @TS, @QueryElapsedMilliseconds);",
                                        new ArrayList {
                                            new SQLiteParameter("@CityId", cityId),
                                            new SQLiteParameter("@DailyMinTemperature", dailyMinTemperature.Obj2String()),
                                            new SQLiteParameter("@DailyMaxTemperature", dailyMaxTemperature.Obj2String()),
                                            new SQLiteParameter("@WeeklyMinTemperature", weeklyMinTemperature.Obj2String()),
                                            new SQLiteParameter("@WeeklyMaxTemperature", weeklyMaxTemperature.Obj2String()),
                                            new SQLiteParameter("@TS", ts),
                                            new SQLiteParameter("@QueryElapsedMilliseconds", weatherForecast.ForecastQueryElapsedMilliseconds)
                                        });
                                if (res == -1)
                                {
                                    Log.Error("Forecast table insert error");
                                }
                                else
                                {
                                    var dt = DBlite.GetData($"SELECT Forecast.Id, Forecast.CityId, Location.CityName, Forecast.DailyMinTemperature, Forecast.DailyMaxTemperature, Forecast.WeeklyMinTemperature, Forecast.WeeklyMaxTemperature, Forecast.TS, Forecast.QueryElapsedMilliseconds AS ForecastQueryElapsedMilliseconds, Location.QueryElapsedMilliseconds AS LocationQueryElapsedMilliseconds FROM Forecast INNER JOIN Location ON Location.Id = Forecast.CityId WHERE Forecast.CityId = @CityId AND Forecast.TS = @TS;",
                                        new ArrayList {
                                            new SQLiteParameter("@CityId", cityId),
                                            new SQLiteParameter("@TS", ts),
                                        });
                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        var dr = dt.Rows[0];
                                        weatherForecast = new WeatherForecastModel
                                        {
                                            CityId = dr["CityId"].Obj2Int32(),
                                            TS = dr["TS"].Obj2Int64(),
                                            DailyMinTemperature = dr["DailyMinTemperature"].Obj2Int32(),
                                            DailyMaxTemperature = dr["DailyMaxTemperature"].Obj2Int32(),
                                            WeeklyMinTemperature = dr["WeeklyMinTemperature"].Obj2Int32(),
                                            WeeklyMaxTemperature = dr["WeeklyMaxTemperature"].Obj2Int32(),
                                            ForecastQueryElapsedMilliseconds = dr["ForecastQueryElapsedMilliseconds"].Obj2Int64(),
                                            LocationQueryElapsedMilliseconds = dr["LocationQueryElapsedMilliseconds"].Obj2Int64(),
                                        };
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("ForecastService : " + ex.ToString());
            }

            return weatherForecast;
        }
    }
}
