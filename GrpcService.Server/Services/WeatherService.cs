using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcService.Server.Contracts;

namespace GrpcService.Server.Services
{
    public class WeatherService : Server.WeatherService.WeatherServiceBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public WeatherService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        public override async Task<WeatherResponse> GetCurrentWeather(GetCurrentWeatherForCity request, ServerCallContext context)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var uri = $"https://api.openweathermap.org/data/2.5/weather?q={request.City}&appid=33437061427810ad3d26f6ae0ebac594&units={request.Units}";
            var responseText = await httpClient.GetStringAsync(uri);

            var temperatures = JsonSerializer.Deserialize<Temperatures>(responseText);
            return new WeatherResponse
            {
                Temperature = temperatures!.Main.Temp,
                FeelsLike = temperatures.Main.FeelsLike
            };
        }
    }
}