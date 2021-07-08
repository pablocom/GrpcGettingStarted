using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcService.Server.Services
{
    public class PrintService : Server.PrintService.PrintServiceBase
    {
        private readonly ILogger<PrintService> _logger;

        public PrintService(ILogger<PrintService> logger)
        {
            _logger = logger;
        }

        public override async Task<Empty> PrintStream(IAsyncStreamReader<PrintRequest> requestStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
                _logger.LogInformation($"{DateTime.UtcNow:O} - Client said: {request.Message}");

            return new Empty();
        }
    }
}