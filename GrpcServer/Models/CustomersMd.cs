using Grpc.Core;
using GrpcDemoApp;
using System;

namespace GrpcServer.Models
{
    public class CustomersMd
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ColorInConsole { get; set; }
        public IAsyncStreamWriter<ChatMessage> Stream { get; set; }
    }
}
