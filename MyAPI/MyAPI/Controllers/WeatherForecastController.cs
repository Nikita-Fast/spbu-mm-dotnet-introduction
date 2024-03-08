using Microsoft.AspNetCore.Mvc;
using MyAPI.WeatherServices;

namespace MyAPI.Controllers
{ 

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly TommorowioSPBWeatherService _serviceTommorowio;
        private readonly StormglassSPBWeatherService _serviceStormglass;

        public WeatherForecastController()
        {
            _serviceTommorowio = new TommorowioSPBWeatherService(Environment.GetEnvironmentVariable("APIKEY_TOMORROWIO"));
            _serviceStormglass = new StormglassSPBWeatherService(Environment.GetEnvironmentVariable("APIKEY_STORMGLASS"));
        }

        private Dictionary<string, WeatherForecast> GetAggregatedWeatherData(string service)
        {
            Dictionary<string, WeatherForecast> dict = new Dictionary<string, WeatherForecast>();
            switch (service.ToUpper())
            {
                case "TOMORROWIO":
                    dict["tomorrowio"] = _serviceTommorowio.GetWeatherForecast();
                    break;
                case "STORMGLASS":
                    dict["stormglass"] = _serviceStormglass.GetWeatherForecast();
                    break;
                case "ALL":
                    dict["tomorrowio"] = _serviceTommorowio.GetWeatherForecast();
                    dict["stormglass"] = _serviceStormglass.GetWeatherForecast();
                    break;
                default:
                    throw new Exception($"Сервис {service} не поддерживается");
            }
            return dict;
        }

        /// <summary>
        /// Получить список интегрированных сервисов
        /// </summary>
        [HttpGet("services")]
        public IActionResult GetIntegratedServices()
        {
            return Ok("Tomorrowio, Stormglass");
        }

        /// <summary>
        /// Получить данные о погоде с поддерживаемых сервисов. Поддерживаемые сервисы: Tommorowio, Stormglass
        /// </summary>
        /// <param name="service"></param>
        /// <remarks>
        /// Примеры:
        ///     GET /forecast?service=all
        ///     GET /forecast?service=tommorowio
        ///     GET /forecast?service=stormglass
        /// </remarks>
        [HttpGet("forecast")]
        public IActionResult GetForecast(string service)
        {
            try
            {
                var aggWeatherData = GetAggregatedWeatherData(service);
                return Ok(aggWeatherData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
