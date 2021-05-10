using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        TcpClient clientSocket = new TcpClient();
        NetworkStream serverStream;
        int bufferSize = 4096;
        string endToken = "$";

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            var outStream = Encoding.ASCII.GetBytes(textBoxSend.Text + endToken);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Write("Connected to Secure Chat Server...");
            clientSocket.Connect("127.0.0.1", 8888);
            serverStream = clientSocket.GetStream();

            var outStream = Encoding.ASCII.GetBytes(textBoxName.Text + endToken);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            new Thread(GetMessage).Start();
        }

        private void GetMessage()
        {
            while (true)
            {
                serverStream = clientSocket.GetStream();
                byte[] inStream = new byte[bufferSize];
                serverStream.Read(inStream, 0, inStream.Length);
                Write(Encoding.ASCII.GetString(inStream));
            }
        }

        private void Write(string data)
        {
            if (InvokeRequired)
                Invoke(new Action<string>(Write), data);
            else
                textBoxChat.Text = textBoxChat.Text + Environment.NewLine + " >> " + data;
        }
    }
}
