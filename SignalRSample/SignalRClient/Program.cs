using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRClient
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {

            Console.Write("What is your name? : ");
            var yourname = Console.ReadLine();
            var hubConnection = new HubConnection("http://localhost:1218/signalr");
            IHubProxy chatProxy = hubConnection.CreateHubProxy("ChatHub");
            chatProxy.On("addNewMessageToPage", (string name, string message) =>
            {
                Console.WriteLine($"{name} : {message}");
                Console.Write("Message :");
            });
            await hubConnection.Start();

            Console.Write("Message :");
            while (true)
            {
                var yourmessage = Console.ReadLine();
                await chatProxy.Invoke("Send", yourname, yourmessage);

            }
           
        }
    }
}
