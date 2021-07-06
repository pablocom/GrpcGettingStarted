using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.Server.Contracts;
using Microsoft.Extensions.Logging;

namespace GrpcService.Server.Services
{
    public class WeatherService : Server.WeatherService.WeatherServiceBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WeatherService> _logger;
        
        public WeatherService(IHttpClientFactory httpClientFactory, ILogger<WeatherService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        
        public override async Task<WeatherResponse> GetCurrentWeather(GetCurrentWeatherForCity request, ServerCallContext context)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var temperatures = await GetCurrentTemperaturesAsync(request, httpClient);
            return new WeatherResponse
            {
                Temperature = temperatures!.Main.Temp,
                FeelsLike = temperatures.Main.FeelsLike,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
        }

        public override async Task GetCurrentWeatherStream(
            GetCurrentWeatherForCity request,
            IServerStreamWriter<WeatherResponse> responseStream,
            ServerCallContext context)
        {
            var httpClient = _httpClientFactory.CreateClient();

            for (int i = 0; i < 30; i++)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Request was cancelled");
                    break;
                }
                
                var temperatures = await GetCurrentTemperaturesAsync(request, httpClient);
                await responseStream.WriteAsync(new WeatherResponse()
                {
                    Temperature = temperatures!.Main.Temp,
                    FeelsLike = temperatures.Main.FeelsLike,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
                });
                await Task.Delay(1000);
            }
        }

        private static async Task<Temperatures> GetCurrentTemperaturesAsync(GetCurrentWeatherForCity request, HttpClient httpClient)
        {
            var uri = $"https://api.openweathermap.org/data/2.5/weather?q={request.City}&appid=33437061427810ad3d26f6ae0ebac594&units={request.Units}";
            var responseText = await httpClient.GetStringAsync(uri).ConfigureAwait(false);
            var temperatures = JsonSerializer.Deserialize<Temperatures>(responseText);
            return temperatures;
        }
    }
}