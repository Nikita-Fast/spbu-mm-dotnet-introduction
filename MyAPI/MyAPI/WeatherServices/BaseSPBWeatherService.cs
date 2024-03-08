using RestSharp;

namespace MyAPI.WeatherServices
{
    public abstract class BaseSPBWeatherService
    {
        public const string latitude = "59.9343";
        public const string longitude = "30.3351";
        protected RestClient _client;
        public string _ApiKey { get; private set; }
        public abstract WeatherForecast GetWeatherForecast();

        protected BaseSPBWeatherService(string apiKey)
        {
            _ApiKey = apiKey;
        }
    }
    public class WeatherForecast
    {
        public double TemperatureCelsius { get; set; }
        public double TemperatureFahrenheit => 32 + (int)(TemperatureCelsius / 0.5566);
        public string? Cloudiness { get; set; }
        public string? Humidity { get; set; }
        public string? Precipitation { get; set; }
        public string? WindDirection { get; set; }
        public string? WindSpeed { get; set; }

        public void ReplaceNullValues()
        {
            Cloudiness ??= "Данных нет";
            Humidity ??= "Данных нет";
            Precipitation ??= "Данных нет";
            WindDirection ??= "Данных нет";
            WindSpeed ??= "Данных нет";
        }
    }
}
