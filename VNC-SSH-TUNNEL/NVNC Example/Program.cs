#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NVNC;
using Renci.SshNet;
using Renci.SshNet.Common;
using NAudio;

#endregion

namespace VNCTest
{
    internal class Program
    {
        public static void startTunnel()
        {
            var keypath = @"C:\file.ppk";
            var pk = new PrivateKeyFile(keypath);
            var keyFiles = new[] { pk };

            var client = new SshClient("domain.com", 22, "root", pk);
            client.KeepAliveInterval = new TimeSpan(0, 0, 30);
            client.ConnectionInfo.Timeout = new TimeSpan(0, 0, 20);
            try
            {
                client.Connect();
                Console.WriteLine("[INFO] Connecté");
            }
            catch
            {
                Console.WriteLine("[FATAL] Erreur de connexion au tunnel");
            }
            IPAddress iplocal = IPAddress.Parse("127.0.0.1");
            IPAddress ipremote = IPAddress.Parse("IP");
            UInt32 portdistant1 = 7000;
            UInt32 portlocal1 = 5900;
            UInt32 portdistant2 = 7006;
            UInt32 portlocal2 = 4500;
            ForwardedPortRemote port1 = new ForwardedPortRemote(ipremote, portdistant1, iplocal, portlocal1);
            ForwardedPortRemote port2 = new ForwardedPortRemote(ipremote, portdistant2, iplocal, portlocal2);
            client.AddForwardedPort(port1);
            client.AddForwardedPort(port2);
            port1.Exception += delegate (object sender, ExceptionEventArgs e)
            {
                Console.WriteLine("[FATAL] "+ e.Exception.ToString());
            };
            port2.Exception += delegate (object sender, ExceptionEventArgs e)
            {
                Console.WriteLine("[FATAL] " + e.Exception.ToString());
            };
            port1.Start();
            port2.Start();
            Console.WriteLine("[DEBUG] Port forwarding ok");
            System.Threading.Thread.Sleep(1000 * 60 * 60 * 8);
            port1.Stop();
            port2.Stop();
            client.Disconnect();

        }

        private static void Main(string[] args)
        {
            
            var s = new VncServer("", "password", 5901, 5900, "VNC Client");
            Thread thr = new Thread(new ThreadStart(startTunnel));
            
            try
            {
                thr.Start();
                s.Start();
                Console.Read();
                s.Stop();
            }
            catch (ArgumentNullException ex)
            {
               s.Stop();
                return;
            }
            
        }



    }
}