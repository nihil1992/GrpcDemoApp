using Grpc.Core;
using GrpcDemoApp;
using GrpcServer.Models;
using GrpcServer.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer.Services
{
	public class ChatRoomService : IChatRoomService
    {

		private readonly IChatRoomProvider _chatRoomProvider;

		public ChatRoomService(IChatRoomProvider chatRoomProvider)
		{
			_chatRoomProvider = chatRoomProvider;
		}

		public async Task BroadcastMessageAsync(ChatMessage message)
		{
			var chatRoom = _chatRoomProvider.GetChatRoomById(message.RoomId);
			foreach (var customer in chatRoom.CustomersInRoom)
			{
				await customer.Stream.WriteAsync(message);
				Console.WriteLine($"Sent message from {message.CustomerName} to {customer.Name}");
			}
		}

		public Task<int> AddCustomerToChatRoomAsync(CustomerProtoModel customer)
		{
			var room = _chatRoomProvider.GetFreeChatRoom();
			room.CustomersInRoom.Add(new CustomersMd
			{
				ColorInConsole = customer.ColorInConsole,
				Name = customer.Name,
				Id = Guid.Parse(customer.Id)
			});
			return Task.FromResult(room.Id);
		}

		public void ConnectCustomerToChatRoom(int roomId, Guid customerId, IAsyncStreamWriter<ChatMessage> responseStream)
		{
			_chatRoomProvider.GetChatRoomById(roomId).CustomersInRoom.FirstOrDefault(c => c.Id == customerId).Stream = responseStream;
		}

		public void DisconnectCustomer(int roomId, Guid customerId)
		{
			var room = _chatRoomProvider.GetChatRoomById(roomId);
			room.CustomersInRoom.Remove(room.CustomersInRoom.FirstOrDefault(c => c.Id == customerId));
		}
	}
}
