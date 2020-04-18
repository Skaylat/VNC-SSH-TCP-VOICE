using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;


namespace ClientForm
{
    class Receiver
    {
        int bitRate, bitDepth, deviceID;

        TcpListener serveur;
        IPEndPoint endPoint;
        WaveOut waveOut;
        WaveInEvent sourceStream;
        BufferedWaveProvider waveProvider;

        // Default Constructor
        public Receiver() { }

        // Overloaded Constructor
        public Receiver(int bitRate, int bitDepth, int deviceID, IPAddress serverIp)
        {
            this.bitRate = bitRate;
            this.bitDepth = bitDepth;
            this.deviceID = deviceID;
            serveur = new TcpListener(serverIp, 6001);
            endPoint = new IPEndPoint(serverIp, 6001);
            waveProvider = null;
            sourceStream = null;
            waveOut = new WaveOut();
        }

        public void InitializeStream()
        {
            sourceStream = new WaveInEvent();
            sourceStream.BufferMilliseconds = 50;
            sourceStream.DeviceNumber = 0;
            sourceStream.WaveFormat = new WaveFormat(bitRate, bitDepth, WaveIn.GetCapabilities(deviceID).Channels);
        }

        public void StartWaveProvider()
        {
            waveProvider = new BufferedWaveProvider(sourceStream.WaveFormat);
            waveOut.Init(waveProvider);
            waveOut.Play();
        }

        public void StartListening()
        {
            InitializeStream();
            StartWaveProvider();
            try
            {
                serveur.Start();
            }
            catch (SocketException)
            {
                MessageBox.Show("[FATAL] Erreur");
                return;
            }
            Byte[] bytes = new Byte[4096];
            String data = null;
            while (true)
            {
                Console.Write("Waiting for a connection... ");
                TcpClient client = serveur.AcceptTcpClient();
                Console.WriteLine("Connected!");
                data = null;
                NetworkStream stream = client.GetStream();
                int i = 0;
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    waveProvider.AddSamples(bytes, 0, bytes.Length);
                    if (waveProvider.BufferedBytes > 88100)
                    {
                        waveProvider.ClearBuffer();
                    }
                }

            }
        }

        public void StopReceiver()
        {
            serveur.Stop();
        }
    }
}
