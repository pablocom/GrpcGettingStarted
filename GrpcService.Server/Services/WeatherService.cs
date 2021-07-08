using System;
using System.Net.Http;
using System.Text.Json;
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
        
        public override async Task<WeatherResponse> GetCurrentWeather(GetCurrentWeatherForCityRequest request, ServerCallContext context)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var temperatures = await GetCurrentTemperaturesAsync(request, httpClient);
            return new WeatherResponse
            {
                Temperature = temperatures!.Main.Temp,
                FeelsLike = temperatures.Main.FeelsLike,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                City = request.City,
                Units = request.Units
            };
        }

        public override async Task GetCurrentWeatherStream(
            GetCurrentWeatherForCityRequest request,
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
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    City = request.City,
                    Units = request.Units
                });
                await Task.Delay(1000);
            }
        }

        public override async Task<MultiWeatherResponse> GetMultiCurrentWeatherStream(
            IAsyncStreamReader<GetCurrentWeatherForCityRequest> requestStream, 
            ServerCallContext context)
        {
            
            
            var httpClient = _httpClientFactory.CreateClient();
            var response = new MultiWeatherResponse()
            {
                Weather = { }
            };
            await foreach (var request in requestStream.ReadAllAsync()) // Client streaming for gRPC
            {
                var temperatures = await GetCurrentTemperaturesAsync(request, httpClient);
                response.Weather.Add(new WeatherResponse()
                {
                    Temperature = temperatures!.Main.Temp,
                    FeelsLike = temperatures.Main.FeelsLike,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    City = request.City,
                    Units = request.Units
                });
            }
            return response;
        }

        private static async Task<Temperatures> GetCurrentTemperaturesAsync(GetCurrentWeatherForCityRequest request, HttpClient httpClient)
        {
            var uri = $"https://api.openweathermap.org/data/2.5/weather?q={request.City}&appid=33437061427810ad3d26f6ae0ebac594&units={request.Units}";
            var responseText = await httpClient.GetStringAsync(uri).ConfigureAwait(false);
            var temperatures = JsonSerializer.Deserialize<Temperatures>(responseText);
            return temperatures;
        }
    }
}