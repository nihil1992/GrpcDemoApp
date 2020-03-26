using Grpc.Core;
using GrpcDemoApp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer.Services
{
    public class ChatCustomerService : ChatService.ChatServiceBase
    {
        private readonly ILogger<ChatCustomerService> _logger;
        private readonly IChatRoomService _chatRoomService;


        public ChatCustomerService(ILogger<ChatCustomerService> logger, IChatRoomService chatRoomService)
        {
            _logger = logger;
            _chatRoomService = chatRoomService;
        }

        public override async Task<JoinCustomerReply> JoinCustomerChat(JoinCustomerRequest request, ServerCallContext context)
        {
            return new JoinCustomerReply { RoomId = await _chatRoomService.AddCustomerToChatRoomAsync(request.Customer) };
        }

        public override async Task SendMessageToChatRoom(IAsyncStreamReader<ChatMessage> requestStream, IServerStreamWriter<ChatMessage> responseStream,
            ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            _logger.LogInformation($"Connection id: {httpContext.Connection.Id}");

            if (!await requestStream.MoveNext())
            {
                return;
            }

            _chatRoomService.ConnectCustomerToChatRoom(requestStream.Current.RoomId, Guid.Parse(requestStream.Current.CustomerId), responseStream);
            var user = requestStream.Current.CustomerName;
            _logger.LogInformation($"{user} connected");

            try
            {
                while (await requestStream.MoveNext())
                {
                    if (!string.IsNullOrEmpty(requestStream.Current.Message))
                    {
                        if (string.Equals(requestStream.Current.Message, "qw!", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        await _chatRoomService.BroadcastMessageAsync(requestStream.Current);
                    }
                }
            }
            catch (IOException)
            {
                _chatRoomService.DisconnectCustomer(requestStream.Current.RoomId, Guid.Parse(requestStream.Current.CustomerId));
                _logger.LogInformation($"Connection for {user} was aborted.");
            }
        }
    }
}
