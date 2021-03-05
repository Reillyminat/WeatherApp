using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
namespace WeatherApp.Controllers
{
    public class OpenWeatherResponse
    {
        public string Name { get; set; }

        public IEnumerable<WeatherDescription> Weather { get; set; }

        public Main Main { get; set; }

        public List<List> List { get; set; }

        public Wind Wind { get; set; }
    }
    public class Wind
    {
        public string Speed { get; set; }
    }
    public class WeatherDescription
    {
        public string Description { get; set; }
    }

    public class Main
    {
        public string Temp { get; set; }
        public string Temp_min { get; set; }
        public string Temp_max { get; set; }

    }
    public class List
    {
        public Main Main { get; set; }
        public string dt_txt { get; set; }
        public Wind Wind { get; set; }
        public IEnumerable<WeatherDescription> Weather { get; set; }
    }
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }
        [HttpGet("[action]/{city}")]
        public async Task<IActionResult> WeatherNow(string city)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri("http://api.openweathermap.org");
                    var response = await client.GetAsync($"/data/2.5/weather?q={city}&appid=5cd3c51c9ef4183ce866b54bbf4a4340&units=metric");
                    response.EnsureSuccessStatusCode();

                    var stringResult = await response.Content.ReadAsStringAsync();
                    var rawWeather = JsonConvert.DeserializeObject<OpenWeatherResponse>(stringResult);
                    return Ok(new
                    {
                        Temp = string.Format("{0}°С", Math.Round(Convert.ToDouble(rawWeather.Main.Temp.Replace('.', ',')), 1)),
                        Summary = string.Join(",", rawWeather.Weather.Select(x => x.Description)),
                        Wind_speed = rawWeather.Wind.Speed + "м/с",
                        City = rawWeather.Name,
                        Date = response.Headers.Date
                    });
                }
                catch (HttpRequestException httpRequestException)
                {
                    return BadRequest($"Error getting weather from OpenWeather: {httpRequestException.Message}");
                }
            }
        }
        [HttpGet("[action]/{city}")]
        public async Task<IActionResult> WeatherForecast(string city)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri("http://api.openweathermap.org");
                    var response = await client.GetAsync($"/data/2.5/forecast?q={city}&appid=5cd3c51c9ef4183ce866b54bbf4a4340");
                    response.EnsureSuccessStatusCode();

                    var stringResult = await response.Content.ReadAsStringAsync();
                    var rawWeather = JsonConvert.DeserializeObject<OpenWeatherResponse>(stringResult);
                    int cnt = 0;
                    return Ok(rawWeather.List.Select(i => new
                    {
                        Temp_min = string.Format("{0}°С", Math.Round((double.Parse((rawWeather.List[cnt].Main.Temp_min).Replace('.', ',')) - 273.15))),
                        Temp_max = string.Format("{0}°С", Math.Round((double.Parse((rawWeather.List[cnt].Main.Temp_max).Replace('.', ',')) - 273.15))),
                        Summary = string.Join(",", rawWeather.List[cnt].Weather.Select(x => x.Description)),
                        Wind_speed = rawWeather.List[cnt].Wind.Speed+"м/с",
                        dt_txt = rawWeather.List[cnt++].dt_txt
                    }));
                }
                catch (HttpRequestException httpRequestException)
                {
                    return BadRequest($"Error getting weather from OpenWeather: {httpRequestException.Message}");
                }
            }
        }
    }
}
