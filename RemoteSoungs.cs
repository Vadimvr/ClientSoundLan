using NAudio.Wave;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace clientSoung
{
    public partial class RemoteSounds : Form
    {
        private bool play = false;
        WaveOut outSound;
        BufferedWaveProvider buffer;
        Thread thread;
        Socket listeningSocket;
        int port = 8855;
        EndPoint remoteIp;
        string Host;
        public RemoteSounds()
        {
            InitializeComponent();
        }
        private void Listening()
        { 
            while (play)
            {
                try
                {
                    byte[] data = new byte[65535];
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
                port = (int)portNUD.Value;
                buffer = new BufferedWaveProvider(new WaveFormat(36000, 32, 2));
                outSound = new WaveOut();
                outSound.Init(buffer);
                outSound.Play();
                play = true;
                Host = Dns.GetHostName();
                remoteIp = new IPEndPoint(IPAddress.Parse(Dns.GetHostByName(Host).AddressList[0].ToString()), port);
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                listeningSocket.Bind(remoteIp);
                thread = new Thread(new ThreadStart(Listening));
                thread.Start();
            }
        }
        private void Stop_Click(object sender, EventArgs e)
        {
            if (play)
            {
                thread.Abort();
                play = false;
                listeningSocket.Close();
               listeningSocket.Dispose();
                if (outSound != null)
                {
                    outSound.Stop();
                    outSound.Dispose();
                    outSound = null;
                }
                buffer = null;
            }
        }
    }
}