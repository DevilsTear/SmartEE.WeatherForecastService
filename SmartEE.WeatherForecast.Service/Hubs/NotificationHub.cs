using Microsoft.AspNetCore.SignalR;
using SmartEE.WeatherForecast.Common.Models.Forecast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace SmartEE.WeatherForecast.Service.Hubs
{
    /// <summary>
    /// SignalR hub class
    /// </summary>
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Processes to be carried out when a new SignalR connection has exposed..
        /// </summary>
        /// <returns></returns>
        public async override Task OnConnectedAsync()
        {
            var identity = Context.User.Identity;

            //Check if the client authenticated..
            if (identity.IsAuthenticated)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, identity.Name).ConfigureAwait(false);
            }
            else Context.Abort();

            var ui = Context.UserIdentifier;

            var dt = DBlite.GetData($"SELECT Forecast.Id, Forecast.CityId, Location.CityName, Forecast.DailyMinTemperature, Forecast.DailyMaxTemperature, Forecast.WeeklyMinTemperature, Forecast.WeeklyMaxTemperature, Forecast.TS, Forecast.QueryElapsedMilliseconds AS ForecastQueryElapsedMilliseconds, Location.QueryElapsedMilliseconds AS LocationQueryElapsedMilliseconds FROM Forecast INNER JOIN Location ON Location.Id = Forecast.CityId WHERE Forecast.TS = @TS;",
                                        new ArrayList {
                                            new SQLiteParameter("@TS", DateTime.Now.Date.ToEpoch())
                                        });
            if (dt != null && dt.Rows.Count > 0)
            {
                var dailyQueries = new List<WeatherForecastModel>();

                foreach (DataRow dr in dt.Rows)
                    dailyQueries.Add(new WeatherForecastModel
                    {
                        CityId = dr["CityId"].Obj2Int32(),
                        CityName = dr["CityName"].Obj2String(),
                        TS = dr["TS"].Obj2Int64(),
                        DailyMinTemperature = dr["DailyMinTemperature"].Obj2Int32(),
                        DailyMaxTemperature = dr["DailyMaxTemperature"].Obj2Int32(),
                        WeeklyMinTemperature = dr["WeeklyMinTemperature"].Obj2Int32(),
                        WeeklyMaxTemperature = dr["WeeklyMaxTemperature"].Obj2Int32(),
                        ForecastQueryElapsedMilliseconds = dr["ForecastQueryElapsedMilliseconds"].Obj2Int64(),
                        LocationQueryElapsedMilliseconds = dr["LocationQueryElapsedMilliseconds"].Obj2Int64(),
                    });

                await Clients.User(ui).SendAsync("CurrentDayForecastQueries", dailyQueries);
            }


            await base.OnConnectedAsync();
        }
    }
}
