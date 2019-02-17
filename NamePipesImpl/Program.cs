using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using NamedPipesLibrary;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMTtoPDFMainProcess
{
    class Program
    {
        public static Boolean isEnded = false;
        [STAThread]
        static void Main(string[] args)
        {
            // MyNamedPipes.Run();

            List<Process> processList = new List<Process>();
            int i = 0;
            while (i < 4)
            {

                ProcessStartInfo processStartInfo = new ProcessStartInfo(@"C:\Users\PWang\source\repos\CMTFileExport - Copy\CMTFileExport\bin\Debug\CMTFileExport.exe", "DEV," + i.ToString());
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.ErrorDialog = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardError = true;
                //processStartInfo.ErrorDialogParentHandle
                Process process = new Process();
                //process.BeginOutputReadLine();
                process.EnableRaisingEvents = true;
                process.Exited += Process_Exited;
                //process.ErrorDataReceived += Process_ErrorDataReceived;
                process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(Process_ErrorDataReceived);
                process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(Process_ErrorDataReceived);
                process.StartInfo = processStartInfo;
                process.Start();
                Thread subThread = new Thread(() =>
                {
                    process.WaitForExit(1000);

                    try
                    {
                        //ExitCode throws if the process is hanging
                        process.Close();
                        process.Dispose();
                    }
                    catch (InvalidOperationException ioex)
                    {

                    }
                });
                subThread.Start();
                processList.Add(process);
                i++;
                Thread.Sleep(5000);
            }
            while (!isEnded)
            {
                int j = 0;
                foreach (var process in processList)
                {
                    if (!process.HasExited)
                    {
                        j++;
                    }
                }
                Console.WriteLine(j);
                Thread.Sleep(2000);
            }


            Thread.Sleep(20000);
            // Run();
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            ((Process)sender).Close();
            ((Process)sender).Dispose();
            Console.WriteLine("process closed");
        }

        private static string pipeName = "Demo2Pipe";

        public static void Run()
        {
            Task.Run(() => Server());

            Task.Delay(300).Wait();

            var clients = new List<string>()
            {
                "Client 1",
                "Client 2",
                "Client 3",
                "Client 4",
                "Client 5",
                "Client 6",
                "Client 7",
                "Client 8"
            };

            Parallel.ForEach(clients, (c) => Client(c));
        }

        static void Server()
        {
            var server = new NamedPipeServer(pipeName);
            server.newRequestEvent += (s, e) =>
            {
                e.Response = "Echo. " + e.Request;
                Console.WriteLine(e.Request);
            };
            server.newServerInstanceEvent += (s, e) => Console.WriteLine("server start");
            server.Start();
            Task.Delay(10000).Wait();
            server.Dispose();
        }

        static void Client(string clientName)
        {
            using (var client = new NamedPipeClient(pipeName))
            {
                client.OnConnection += Client_OnConnection;
                client.Connect();
                var request = clientName + " Request a";
                client.SendRequest(request);
                var response = client.ReceiveResponse();
                Console.WriteLine(response);
                Task.Delay(100).Wait();

                var request1 = clientName + " Request b";
                client.SendRequest(request1);
                var response1 = client.ReceiveResponse();
                Console.WriteLine(response1);
                Task.Delay(100).Wait();

                var request2 = clientName + " Request c";
                client.SendRequest(request2);
                var response2 = client.ReceiveResponse();
                Console.WriteLine(response2);
            }
        }

        private static void Client_OnConnection(object sender, EventArgs e)
        {
            Console.WriteLine("Client Connected");
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            isEnded = true;
            MessageBox.Show("exit");
        }
    }
}
