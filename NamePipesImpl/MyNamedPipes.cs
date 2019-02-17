using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamePipesImpl
{
    class MyNamedPipes
    {
        public static Boolean isEnded = false;
        private static string pipeName = "Demo1Pipe";

        public static void Run()
        {
            Task.Run(() => Server());

            Task.Delay(300).Wait();

            Client();
        }

        static void Server()
        {
            using (var server = new NamedPipeServerStream(pipeName))
            {
                server.WaitForConnection();
                var reader = new StreamReader(server);
                var writer = new StreamWriter(server);
                while (true)
                {
                    var received = reader.ReadLine();
                    Console.WriteLine("Received from client: " + received);
                    //var toSend = "Hello, client.";
                    //writer.WriteLine(toSend);
                    //writer.Flush();
                }
            }
        }

        static void Client()
        {
            using (var client = new NamedPipeClientStream(pipeName))
            {
                using (var writer = new StreamWriter(client))
                {
                    client.Connect(100);
                    while (true)
                    {
                        var request = "Hello, server.";
                        writer.WriteLine(request);
                        writer.Flush();

                        //var reader = new StreamReader(client);
                        //var response = reader.ReadLine();
                        //Console.WriteLine("Response from server: " + response);
                    }
                }
            }
        }
    }
}
