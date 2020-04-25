using Serilog;
using SmartEE.WeatherForecast.Common.Models.Location;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace SmartEE.WeatherForecast.Service.Services
{
    internal class LocationService
    {
        static ConcurrentDictionary<long, ConcurrentDictionary<string, LocationModel>> methodRequests = new ConcurrentDictionary<long, ConcurrentDictionary<string, LocationModel>>();

        static string _LocationWSURL = "";//"https://eu1.locationiq.com/v1/search.php?key=[TOKEN]&q=[LOCATION]&format=json";
        /// <summary>
        /// Location service initialization
        /// </summary>
        /// <param name="wsToken">Location service token</param>
        internal static void Init(string wsToken = null)
        {
            _LocationWSURL = $"https://eu1.locationiq.com/v1/search.php?key={(wsToken ?? "a1779b7817b3b2")}&q=[LOCATION]&format=json";
            try
            {
                var res = DBlite.ExecuteSQL("CREATE TABLE IF NOT EXISTS Location (Id INTEGER PRIMARY KEY AUTOINCREMENT, CityName TEXT, Latitude TEXT, Longitude TEXT, QueryElapsedMilliseconds INTEGER); CREATE UNIQUE INDEX IF NOT EXISTS ndx_CityName ON Location(CityName);");
                if (res == -1)
                {
                    Log.Error("Location table create error");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Location table create error", ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Fetches location info by city name from related remote api
        /// </summary>
        /// <param name="cityName">City Name</param>
        /// <returns>LocationModel</returns>
        internal static LocationModel FetchLocationInfo(string cityName)
        {
            cityName = RemoveDiacritics(cityName.ToLower());
            LocationModel locationModel = null;
            var currrentTSHour = Convert.ToInt64((Math.Floor((decimal)(DateTime.Now.ToEpoch()) / 3600) * 3600));
            if (!methodRequests.ContainsKey(currrentTSHour)) // eger guncel saat dict icinde yoksa ekleme ve onceki saatle islem yapma bolumu
            {
                methodRequests[currrentTSHour] = new ConcurrentDictionary<string, LocationModel>();
            }

            if (methodRequests[currrentTSHour].ContainsKey(cityName))
            {
                return methodRequests[currrentTSHour].Where(r => r.Key == cityName).Select(r => r.Value).FirstOrDefault();
            }

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = httpClient.GetAsync(_LocationWSURL.Replace("[LOCATION]", cityName ?? "istanbul")).Result)
                    {
                        using (var content = response.Content)
                        {
                            //get the json result from location api
                            var result = content.ReadAsStringAsync().Result;
                            locationModel = Deserialize.FromJson(result).FirstOrDefault();
                            sw.Stop();
                            if (locationModel != null)
                            {
                                //TO DO : consider same spelled different places..
                                //This is more simple to check if the record is available or not then if record not available save etc..
                                var res = DBlite.ExecuteSQL($"INSERT OR IGNORE INTO Location (CityName, Latitude, Longitude, QueryElapsedMilliseconds) VALUES (@CityName, @Latitude, @Longitude, @QueryElapsedMilliseconds);",
                                    new ArrayList {
                                        new SQLiteParameter("@CityName", cityName),
                                        new SQLiteParameter("@Latitude", locationModel.Lat),
                                        new SQLiteParameter("@Longitude", locationModel.Lon),
                                        new SQLiteParameter("@QueryElapsedMilliseconds", sw.ElapsedMilliseconds)
                                    });
                                if (res == -1)
                                {
                                    Log.Error("Location table insert error");
                                }
                                else
                                {
                                    var dt = DBlite.GetData($"SELECT Id, Latitude, Longitude, QueryElapsedMilliseconds FROM Location WHERE CityName = @CityName;",
                                        new ArrayList {
                                            new SQLiteParameter("@CityName", cityName)
                                        });

                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        var dr = dt.Rows[0];
                                        locationModel = new LocationModel
                                        {
                                            CityId = dr["Id"].Obj2Int32(),
                                            CityName = cityName,
                                            Lat = dr["Latitude"].Obj2String(),
                                            Lon = dr["Longitude"].Obj2String(),
                                            QueryElapsedMilliseconds = dr["QueryElapsedMilliseconds"].Obj2Int64(),
                                        };

                                        //max 50 records
                                        if (methodRequests.Count >= 50)
                                            methodRequests[currrentTSHour] = new ConcurrentDictionary<string, LocationModel>();

                                        //if cityId does not exist
                                        if (!methodRequests[currrentTSHour].ContainsKey(cityName))
                                            methodRequests[currrentTSHour].TryAdd(cityName, locationModel);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("LocationService : " + ex.ToString());
            }

            return locationModel;
        }

        internal static string RemoveDiacritics(string text)
        {
            Encoding srcEncoding = Encoding.UTF8;
            Encoding destEncoding = Encoding.GetEncoding(1252); // Latin alphabet

            text = destEncoding.GetString(Encoding.Convert(srcEncoding, destEncoding, srcEncoding.GetBytes(text)));

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                if (!CharUnicodeInfo.GetUnicodeCategory(normalizedString[i]).Equals(UnicodeCategory.NonSpacingMark))
                {
                    result.Append(normalizedString[i]);
                }
            }

            return result.ToString();
        }
    }
}
