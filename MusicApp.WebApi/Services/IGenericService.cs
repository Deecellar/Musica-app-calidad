using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicApp.WebApi.Services
{
    public interface IWeatherForecastService
    {
        Task<IEnumerable<WeatherForecast>> GetWeather(int ammount);
    }
}