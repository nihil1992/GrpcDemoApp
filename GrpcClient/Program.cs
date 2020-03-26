using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemoApp;
using GrpcServer.Protos;
using System;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        static Random RNG = new Random();
        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var customerClient = new Customer.CustomerClient(channel);

            #region Unary RCP
            Console.WriteLine("Unary RPC");
            Console.WriteLine("==============================================");
            await EnterCustomerDetail(customerClient);

            Console.WriteLine("Would you like to get any customer detail ? y/n");
            bool isRpt = Convert.ToBoolean((Console.ReadLine().ToLower().Equals("y", StringComparison.InvariantCultureIgnoreCase)) ? true : false);

            if (isRpt)
            {
                await EnterCustomerDetail(customerClient);
            }
            #endregion

            #region Server Streaming RCP

            Console.WriteLine();
            Console.WriteLine("Server Streaming");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine("All Customers List :");
            Console.WriteLine();
            using (var call = customerClient.GetNewCustomer(new NewCustomerRequest()))
            {
                while (await call.ResponseStream.MoveNext())
                {
                    var currentCustomer = call.ResponseStream.Current;

                    Console.WriteLine($"{currentCustomer.FirstName} {currentCustomer.LastName} :- {currentCustomer.EmailAddress}");
                }
            }
            #endregion

            #region Client Streaming RCP
            Console.WriteLine();
            Console.WriteLine("Client Streaming");
            Console.WriteLine("==============================================");


            var streamClient = new Cltstream.CltstreamClient(channel);
            await ClientStreamingCallExample(streamClient);

            #endregion

            #region Bidirectional RCP
            Console.WriteLine();
            Console.WriteLine("Bi-Direactional Streaming");
            Console.WriteLine("==============================================");

            var customer = new CustomerProtoModel
            {
                ColorInConsole = GetRandomChatColor(),
                Id = Guid.NewGuid().ToString(),
                Name = args.Length > 0 ? args[0] : "TheHulk"
            };

            
            var client = new ChatService.ChatServiceClient(channel);
            var joinCustomerReply = await client.JoinCustomerChatAsync(new JoinCustomerRequest
            {
                Customer = customer
            });

            using (var streaming = client.SendMessageToChatRoom(new Metadata { new Metadata.Entry("CustomerName", customer.Name) }))
            {
                var response = Task.Run(async () =>
                {
                    while (await streaming.ResponseStream.MoveNext())
                    {
                        Console.ForegroundColor = Enum.Parse<ConsoleColor>(streaming.ResponseStream.Current.Color);
                        Console.WriteLine($"{streaming.ResponseStream.Current.CustomerName}: {streaming.ResponseStream.Current.Message}");
                        Console.ForegroundColor = Enum.Parse<ConsoleColor>(customer.ColorInConsole);
                    }
                });

                await streaming.RequestStream.WriteAsync(new ChatMessage
                {
                    CustomerId = customer.Id,
                    Color = customer.ColorInConsole,
                    Message = "",
                    RoomId = joinCustomerReply.RoomId,
                    CustomerName = customer.Name
                });
                Console.ForegroundColor = Enum.Parse<ConsoleColor>(customer.ColorInConsole);
                Console.WriteLine($"Joined the chat as {customer.Name}");

                var line = Console.ReadLine();
                DeletePrevConsoleLine();
                while (!string.Equals(line, "qw!", StringComparison.OrdinalIgnoreCase))
                {
                    await streaming.RequestStream.WriteAsync(new ChatMessage
                    {
                        Color = customer.ColorInConsole,
                        CustomerId = customer.Id,
                        CustomerName = customer.Name,
                        Message = line,
                        RoomId = joinCustomerReply.RoomId
                    });
                    line = Console.ReadLine();
                    DeletePrevConsoleLine();
                }
                await streaming.RequestStream.CompleteAsync();
            }
            Console.WriteLine("Press any key to exit...");
            #endregion


            Console.WriteLine("==========**** END ****===========");
            Console.ReadLine();
        }

        #region Unary
        //This method is used for Unary RPC
        private static async Task EnterCustomerDetail(Customer.CustomerClient customerClient)
        {
            Console.WriteLine("Enter UserId : ");
            var userIdCnl = Convert.ToInt32(Console.ReadLine());
            await CustomerInfoMethod(userIdCnl, customerClient);
        }

        //This method is used for Unary RPC
        private static async Task CustomerInfoMethod(int userId, Customer.CustomerClient customerClient)
        {
            var clientRequestId = new CustomerLookUpModel()
            {
                UserId = userId
            };

            var customer = await customerClient.GetCustomerInfoAsync(clientRequestId);

            Console.WriteLine($"{customer.FirstName} {customer.LastName}");
        }
        #endregion

        #region Client Streaming

        private static async Task ClientStreamingCallExample(Cltstream.CltstreamClient client)
        {
            using (var call = client.AccumulateCount())
            {
                for (var i = 0; i < 3; i++)
                {
                    var count = RNG.Next(5);
                    Console.WriteLine($"Accumulating with {count}");
                    await call.RequestStream.WriteAsync(new CounterRequest { Count = count });
                    await Task.Delay(2000);
                }

                await call.RequestStream.CompleteAsync();

                var response = await call;
                Console.WriteLine($"Count: {response.Count}");
            }
        }
        #endregion

        #region Bidicrectional Streaming
        private static string GetRandomChatColor()
        {
            var colors = Enum.GetValues(typeof(ConsoleColor));
            var rnd = new Random();
            return colors.GetValue(rnd.Next(1, colors.Length - 1)).ToString();
        }

        private static void DeletePrevConsoleLine()
        {
            if (Console.CursorTop == 0) return;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
        #endregion
    }
}
