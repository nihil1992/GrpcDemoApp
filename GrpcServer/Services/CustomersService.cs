using Grpc.Core;
using GrpcServer.Protos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer.Services
{
    public class CustomersService : Customer.CustomerBase
    {
        private readonly ILogger<CustomersService> _logger;

        public CustomersService(ILogger<CustomersService> logger)
        {
            _logger = logger;
        }

        //This service method Unary call
        public override Task<CustomerModel> GetCustomerInfo(CustomerLookUpModel request, ServerCallContext context)
        {
            CustomerModel customerDetail = new CustomerModel();

            switch (request.UserId)
            {
                case 1:
                    {
                        customerDetail.FirstName = "Nihil";
                        customerDetail.LastName = "Patel";
                        customerDetail.EmailAddress = "nihilpatel1992@gmail.com";
                        customerDetail.Age = 28;
                        customerDetail.IsActive = true;
                    }
                    break;
                case 2:
                    {
                        customerDetail.FirstName = "Sonal";
                        customerDetail.LastName = "Patel";
                        customerDetail.EmailAddress = "sonalnihilpatel@gmail.com";
                        customerDetail.Age = 29;
                        customerDetail.IsActive = true;
                    }
                    break;
                case 3:
                    {
                        customerDetail.FirstName = "Ketan";
                        customerDetail.LastName = "Varude";
                        customerDetail.EmailAddress = "ketanvarude@gmail.com";
                        customerDetail.Age = 28;
                        customerDetail.IsActive = true;
                    }
                    break;
                default:
                    {
                        customerDetail.FirstName = "Nihil";
                        customerDetail.LastName = "Patel";
                        customerDetail.EmailAddress = "nihilpatel1992@gmail.com";
                        customerDetail.Age = 28;
                        customerDetail.IsActive = true;
                    }
                    break;
            }

            return Task.FromResult(customerDetail);
        }

        //This service method Server Streaming call
        public override async Task GetNewCustomer(NewCustomerRequest request, IServerStreamWriter<CustomerModel> responseStream, ServerCallContext context)
        {
            List<CustomerModel> customers = new List<CustomerModel>()
            {
                new CustomerModel
                {
                    FirstName="Tim",
                    LastName="Corey",
                    EmailAddress="tim@iamtimcorey.com",
                    Age=42,
                    IsActive=true
                },
                new CustomerModel
                {
                    FirstName="Sue",
                    LastName="Strom",
                    EmailAddress="sue@stromy.net",
                    Age=28,
                    IsActive=false
                },
                new CustomerModel
                {
                    FirstName="Bilbo",
                    LastName="Baggins",
                    EmailAddress="bilbo@iddleearth.com",
                    Age=27,
                    IsActive=false
                },
                new CustomerModel
                {
                    FirstName="Charly",
                    LastName="Parker",
                    EmailAddress="charly@parker.com",
                    Age=35,
                    IsActive=true
                }
            };

            foreach (var cust in customers)
            {
                //add time for getting response with 1 sec delay.
                await Task.Delay(1000);
                await responseStream.WriteAsync(cust);
            }
        }


    }

}
