using GrpcServer.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grpc.Core;

namespace GrpcServer.Services
{
    public class ClientStreamService : Cltstream.CltstreamBase
    {
        private readonly ILogger<ClientStreamService> _logger;
        private readonly IncrementingCounter _counter;

        public ClientStreamService(ILogger<ClientStreamService> logger, IncrementingCounter counter)
        {
            _logger = logger;
            _counter = counter;
        }
        //This service method Client Streaming call
        public override async Task<CounterReply> AccumulateCount(IAsyncStreamReader<CounterRequest> requestStream, ServerCallContext context)
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                _logger.LogInformation($"Incrementing count by {message.Count}");

                _counter.Increment(message.Count);
            }

            return new CounterReply { Count = _counter.Count };
        }
    }
}
