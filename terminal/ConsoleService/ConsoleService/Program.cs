using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleService
{
    class Program
    {
        static async Task Main()
        {
            var connectionString = "Endpoint=sb://paid-bus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=g+v1x2DzE3fB7GKa+LB3YmOszK79sXAke0mmWYtm2Jk=";

            Console.WriteLine("Creating Service Bus sender....");
            
            var sender = new MessageSender(connectionString, "agroqueue");

            var msj = new Message(Encoding.UTF8.GetBytes($"mensaje 1"))
            {
                SessionId = "sesion-mensaje1"
            };
            await sender.SendAsync(msj);

            await sender.SendAsync(new Message(Encoding.UTF8.GetBytes($"mensaje 2"))
            { 
                SessionId = "sesion-mensaje1"
            });

            await sender.SendAsync(new Message(Encoding.UTF8.GetBytes($"mensaje 3"))
            {
                SessionId = "sesion-mensaje1"
            });

            Console.WriteLine("Sending all messages...");
            
            Console.WriteLine("All messages sent.");
        }
    }
}
