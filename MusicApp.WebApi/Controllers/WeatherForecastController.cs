using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MusicApp.WebApi.Services;

namespace MusicApp.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {


        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherForecastService _service;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            _logger.LogDebug("Returning 5 weather thingies");
            return await _service.GetWeather(5);
        }
    }
}