using MyAPI.WeatherServices;
using MyAPI.Controllers;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestReplaceNullValues()
        {
            var x = new WeatherForecast();
            const int TEMPERATURE_CELSIUS = 11;
            const string WIND_SPEED = "22";
            const string CLOUDINESS = "33";
            const string NO_DATA = "Данных нет";

            x.TemperatureCelsius = TEMPERATURE_CELSIUS;
            x.Cloudiness = CLOUDINESS;
            x.WindSpeed = WIND_SPEED; 
            x.ReplaceNullValues();

            Assert.Equal(TEMPERATURE_CELSIUS, x.TemperatureCelsius);
            Assert.Equal(32 + (int)(TEMPERATURE_CELSIUS / 0.5566), x.TemperatureFahrenheit);
            Assert.Equal(CLOUDINESS, x.Cloudiness);
            Assert.Equal(NO_DATA, x.Humidity);
            Assert.Equal(NO_DATA, x.Precipitation);
            Assert.Equal(NO_DATA, x.WindDirection);
            Assert.Equal(WIND_SPEED, x.WindSpeed);
        }

        [Fact]
        public void TestGetForecast()
        {
            var api = new WeatherForecastController();

            const string UNSUPPORTED_SERVICE_NAME = "lalala";
            var res = (Microsoft.AspNetCore.Mvc.ObjectResult)api.GetForecast(UNSUPPORTED_SERVICE_NAME);
            Assert.Equal(500, res.StatusCode);
            Assert.Equal($"Сервис {UNSUPPORTED_SERVICE_NAME} не поддерживается", res.Value);

        }
    }
}