using RestSharp;
using System.Text.Json;

namespace MyAPI.WeatherServices
{
    public class TommorowioSPBWeatherService : BaseSPBWeatherService
    {
        public TommorowioSPBWeatherService(string apiKey) : base(apiKey)
        {
            _client = new RestClient("https://api.tomorrow.io/v4/");
        }

        override public WeatherForecast GetWeatherForecast()
        {
            WeatherForecast weatherForecast = new WeatherForecast();

            var request = new RestRequest("weather/forecast", Method.Get);
            request.AddQueryParameter("apikey", _ApiKey);
            request.AddQueryParameter("location", $"{latitude},{longitude}");
            request.AddQueryParameter("fields", "temperature,cloudCover,humidity,precipitationProbability,windSpeed,windDirection");

            var response = _client.Execute(request);
            if (response.IsSuccessful)
            {   
                if (response.Content is null)
                {
                    throw new Exception("Tomorrowio: получен пустой ответ на запрос о погоде");
                }

                TomorrowioWeatherForecast? forecast = JsonSerializer.Deserialize<TomorrowioWeatherForecast>(response.Content);

                if (forecast is null) {
                    throw new Exception("Tomorrowio: Данных нет");
                }

                Minutely currentWeather = forecast.timelines.minutely[0];

                weatherForecast.TemperatureCelsius = currentWeather.values.temperature;
                weatherForecast.Cloudiness = currentWeather.values.cloudCover.ToString();
                weatherForecast.Humidity = currentWeather.values.humidity.ToString();
                weatherForecast.Precipitation = currentWeather.values.precipitationProbability.ToString();
                weatherForecast.WindSpeed = currentWeather.values.windSpeed.ToString();
                weatherForecast.WindDirection = currentWeather.values.windDirection.ToString();
            }
            else
            {
                throw new Exception($"Не удалось получить данные с Tomorrowio: {response.ErrorMessage}");
            }

            weatherForecast.ReplaceNullValues();
            return weatherForecast;
        }
    }

    public class TomorrowioWeatherForecast
    {
        public Timelines timelines { get; set; }
    }

    public class Timelines
    {
        public List<Minutely> minutely { get; set; }
    }

    public class Minutely
    {
        public DateTime time { get; set; }
        public Values values { get; set; }
    }

    public class Values
    {
        public double cloudCover { get; set; }
        public double humidity { get; set; }
        public double precipitationProbability { get; set; }
        public double temperature { get; set; }
        public double windDirection { get; set; }
        public double windSpeed { get; set; }
    }
}
