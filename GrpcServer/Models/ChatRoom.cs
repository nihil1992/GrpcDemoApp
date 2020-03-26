using GrpcDemoApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer.Models
{
    public class ChatRoom
    {
		public int Id { get; }
		public List<CustomersMd> CustomersInRoom { get; }
		public ChatRoom(int id)
		{
			Id = id;
			CustomersInRoom = new List<CustomersMd>();
		}
	}
}
