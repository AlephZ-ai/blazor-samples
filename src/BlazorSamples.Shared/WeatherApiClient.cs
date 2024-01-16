using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared
{
    public class WeatherApiClient(HttpClient httpClient)
    {
        public async Task<WeatherForecast[]> GetWeatherAsync()
        {
            return await httpClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast").ConfigureAwait(false) ?? [];
        }
    }
}
