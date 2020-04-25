using SmartEE.WeatherForecast.ConsoleApp.Services;
using System;

namespace SmartEE.WeatherForecast.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Weather Forecast Client..");
            RealTimeCommunication.Init();
            Console.ReadLine();
        }
    }
}
