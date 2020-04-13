using NAudio.Wave;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace clientSoung
{
    public partial class RemoteSoungs : Form
    {
        private bool play = false;
        WaveOut outSoung;
        BufferedWaveProvider buffer;
        Thread thread;
        Socket listeningSocket;
        public RemoteSoungs()
        {
            InitializeComponent();
        }
        private void Listening()
        {
            string Host = Dns.GetHostName();
            EndPoint remoteIp = new IPEndPoint(IPAddress.Parse(Dns.GetHostByName(Host).AddressList[0].ToString()), 8855);
            int siseArr = int.Parse(textBox2.Text);
            listeningSocket.Bind(remoteIp);
            while (play)
            {
                try
                {
                    byte[] data = new byte[siseArr];
                    int received = listeningSocket.ReceiveFrom(data, ref remoteIp);
                    buffer.AddSamples(data, 0, received);
                }
                catch (SocketException ex)
                {
                    play = false;
                    MessageBox.Show(ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    play = false;
                    MessageBox.Show(ex.Message);
                    break;
                }
            }
            thread = new Thread(new ThreadStart(Listening));
            thread.Start();
        }
        private void Start_Click(object sender, EventArgs e)
        {
            if (!play)
            {
                buffer = new BufferedWaveProvider(new WaveFormat(36000, 32, 2));
                outSoung = new WaveOut();
                outSoung.Init(buffer);
                outSoung.Play();
                play = true;
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                thread = new Thread(new ThreadStart(Listening));
                thread.Start();
            }
        }
        private void Stop_Click(object sender, EventArgs e)
        {
            if (play)
            {
                play = false;
                listeningSocket.Close();
                listeningSocket.Dispose();
                if (outSoung != null)
                {
                    outSoung.Stop();
                    outSoung.Dispose();
                    outSoung = null;
                }
                buffer = null;
            }
        }
    }
}