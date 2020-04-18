using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ClientForm
{
    public partial class Form1 : Form
    {
        public IPAddress myIpAddress;
        public Form1()
        {
            InitializeComponent();

            Cmb_BitDepth.SelectedIndex = 0;
            Cmb_BitRate.SelectedIndex = 7;

            GetInputDevices();
            Thread statusPort = new Thread(new ThreadStart(testPortBoucle));
            statusPort.Start();
        }

        public void testPortBoucle()
        {

            
            while (true) {
                Thread.Sleep(1000);
                using (TcpClient tcpClient = new TcpClient())
                {
                    try
                    {
                        MethodInvoker inv = delegate
                        {
                            this.label6.Text = "Ouvert";
                        };
                        tcpClient.Connect("127.0.0.1", 6000);
                        tcpClient.Close();
                        this.Invoke(inv);
                    }
                    catch (Exception)
                    {
                        MethodInvoker inv = delegate
                        {
                            this.label6.Text = "Fermé";
                        };
                        try
                        {
                            this.Invoke(inv);
                        }
                        catch (InvalidOperationException)
                        {

                        }
                    }
                }
            }
        }

        public void Btn_CallFriend_Click(object sender, EventArgs e)
        {
            bool call = false;

            if (Txt_FriendAddress.Text.Equals(null))
            {
                MessageBox.Show("Enter an IP address for the friend.");
            }
            else if(call == false)
            {
                try
                {
                    myIpAddress = IPAddress.Parse(Txt_YourAddress.Text.ToString());
                    int bitRate = Int32.Parse(Cmb_BitRate.SelectedItem.ToString());
                    int bitDepth = Int32.Parse(Cmb_BitDepth.SelectedItem.ToString());
                    int deviceID = Cmb_InputDevices.SelectedIndex;
                    string friendAddress = Txt_FriendAddress.Text;

                    Sender send = new Sender(bitRate, bitDepth, deviceID, friendAddress);
                    Receiver receive = new Receiver(bitRate, bitDepth, deviceID, myIpAddress);
                    Thread thr1 = new Thread(new ThreadStart(receive.StartListening));
                    Thread thr2 = new Thread(new ThreadStart(send.SendVoice));

                    try
                    {
                        thr1.Start();
                    }
                    catch (SocketException)
                    {
                        send = new Sender();
                        receive = new Receiver();

                        try
                        {
                            receive.StopReceiver();
                        }
                        catch { }
                        try
                        {
                            send.DisconnectVoice();
                        }
                        catch { }
                            Btn_CallFriend.BackColor = Color.Red;
                            Btn_CallFriend.Text = "Disconnected.";

                            call = false;
                        return;
                    }

                    try
                    {
                        thr2.Start();
                    }
                    catch (SocketException)
                    {
                        send = new Sender();
                        receive = new Receiver();

                        try
                        {
                            receive.StopReceiver();
                        }
                        catch { }
                        try
                        {
                            send.DisconnectVoice();
                        }
                        catch { }
                        Btn_CallFriend.BackColor = Color.Red;
                        Btn_CallFriend.Text = "Disconnected.";

                        call = false;
                        return;
                    }

                    
                    

                    Btn_CallFriend.BackColor = Color.Green;
                    Btn_CallFriend.Text = "Calling Friend.";

                    call = true;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else if (call)
            {
                Sender send = new Sender();
                Receiver receive = new Receiver();

                try
                {
                    receive.StopReceiver();
                    send.DisconnectVoice();

                    Btn_CallFriend.BackColor = Color.Red;
                    Btn_CallFriend.Text = "Disconnected.";

                    call = false;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void GetInputDevices()
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                Cmb_InputDevices.Items.Add(capabilities.ProductName);
            }

            if (Cmb_InputDevices.Items.Count > 0)
            {
                Cmb_InputDevices.SelectedIndex = 0;
            }
        }

        public string GetIpAddress()
        {
            return Txt_YourAddress.Text.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(Txt_YourAddress.Text != "")
            {
                myIpAddress = IPAddress.Parse(Txt_YourAddress.Text.ToString());
                int bitRate = Int32.Parse(Cmb_BitRate.SelectedItem.ToString());
                int bitDepth = Int32.Parse(Cmb_BitDepth.SelectedItem.ToString());
                int deviceID = Cmb_InputDevices.SelectedIndex;
                string friendAddress = Txt_FriendAddress.Text;
                Receiver receive = new Receiver(bitRate, bitDepth, deviceID, myIpAddress);
                Thread thr1 = new Thread(new ThreadStart(receive.StartListening));
                thr1.Start();
            }
            else
            {
                MessageBox.Show("[ERROR] Aucune ip définie pour le serveur");
            }
        }
    }
}
