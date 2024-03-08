using RestSharp;
using System.Text.Json;

namespace MyAPI.WeatherServices
{
    public class StormglassSPBWeatherService : BaseSPBWeatherService
    {
        public StormglassSPBWeatherService(string apiKey) : base(apiKey)
        {
            _client = new RestClient("https://api.stormglass.io/v2/");
        }

        override public WeatherForecast GetWeatherForecast()
        {
            WeatherForecast weatherData = new WeatherForecast();

            var request = new RestRequest("weather/point", Method.Get);
            request.AddHeader("Authorization", _ApiKey);
            request.AddQueryParameter("lat", $"{latitude}");
            request.AddQueryParameter("lng", $"{longitude}");
            request.AddQueryParameter("params", "airTemperature,cloudCover,humidity,gust,windWaveDirection");

            var response = _client.Execute(request);
            if (response.IsSuccessful)
            {
                if (response.Content is null)
                {
                    throw new Exception("Stormglass: получен пустой ответ на запрос о погоде");
                }

                StormglassWeatherForecast? forecast = JsonSerializer.Deserialize<StormglassWeatherForecast>(response.Content);

                if (forecast is null)
                {
                    throw new Exception("Stormglass: Данных нет");
                }

                var currentWeather = forecast.hours[0];
                for (int i = 0; i < forecast.hours.Count; i++)
                {
                    var hourWeather = forecast.hours[i];
                    if (hourWeather.time.Hour == DateTime.UtcNow.Hour)
                    {
                        currentWeather = hourWeather;
                        break;
                    }
                }

                weatherData.TemperatureCelsius = currentWeather.airTemperature.noaa;
                weatherData.Cloudiness = currentWeather.cloudCover.noaa.ToString();
                weatherData.Humidity = currentWeather.humidity.noaa.ToString();
                weatherData.WindSpeed = currentWeather.windWaveDirection.dwd.ToString();
                weatherData.WindDirection = currentWeather.gust.noaa.ToString();
            }
            else
            {
                throw new Exception($"Не удалось получить данные со Stormglass: {response.ErrorMessage}");
            }

            weatherData.ReplaceNullValues();
            return weatherData;
        }
    }

    public class StormglassWeatherForecast
    {
        public List<Hour> hours { get; set; }
    }

    public class Hour
    {
        public AirTemperature airTemperature { get; set; }
        public CloudCover cloudCover { get; set; }
        public Gust gust { get; set; }
        public Humidity humidity { get; set; }
        public WindWaveDirection windWaveDirection {  get; set; }
        public DateTime time {  get; set; }

    }

    public class AirTemperature
    {
        public double noaa { get; set; }
    }

    public class CloudCover
    {
        public double noaa { get; set; }
    }
    public class Gust
    {
        public double noaa { get; set; }
    }

    public class Humidity
    {
        public double noaa { get; set; }
    }

    public class WindWaveDirection
    {
        public double dwd { get; set; }
    }
}
