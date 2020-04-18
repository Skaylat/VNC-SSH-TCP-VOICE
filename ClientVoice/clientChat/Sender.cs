using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Forms;

namespace ClientForm
{
    class Sender
    {
        int bitRate, bitDepth, deviceID;
        string friendAddress;
        IPEndPoint endPoint;
        TcpClient client;
        WaveIn waveIn;

        // Default Constructor
        public Sender() { }

        // Overloaded Constructor
        public Sender(int bitRate, int bitDepth, int deviceID, string friendAddress)
        {
            this.bitRate = bitRate;
            this.bitDepth = bitDepth;
            this.deviceID = deviceID;
            this.friendAddress = friendAddress;

            endPoint = new IPEndPoint(IPAddress.Parse(friendAddress), 8081);
            waveIn = new WaveIn();
        }

        public void InitializeWaveInEvent()
        {
            waveIn.BufferMilliseconds = 50;
            waveIn.DeviceNumber = deviceID;
            waveIn.WaveFormat = new WaveFormat(bitRate, bitDepth, WaveIn.GetCapabilities(deviceID).Channels);
        }

        public void SendVoice()
        {
            InitializeWaveInEvent();
            try
            {
                client = new TcpClient(endPoint.Address.ToString(), endPoint.Port);
            }
            catch (SocketException)
            {
                MessageBox.Show("[ERROR] Le support n'a pas répondu à temps");
                Application.Restart();
                return;
            }
            waveIn.DataAvailable += sourcestream_DataAvailable;
            waveIn.StartRecording();
            while (true)
            {

            }
        }

        private void sourcestream_DataAvailable(object notUsed, WaveInEventArgs e)
        {
            try
            {
                byte[] buffer = (e.Buffer);
                NetworkStream stream = client.GetStream();
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public void DisconnectVoice()
        {
            client.Close();
        }
    }
}
