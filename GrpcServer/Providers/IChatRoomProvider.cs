using GrpcServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer.Providers
{
    public interface IChatRoomProvider
    {
        ChatRoom GetFreeChatRoom();
        ChatRoom GetChatRoomById(int roomId);
        ChatRoom AddChatRoom();
    }
}
