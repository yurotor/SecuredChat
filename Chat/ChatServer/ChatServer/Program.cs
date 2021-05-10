using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer
{
    class Program
    {
        public static Dictionary<string, TcpClient> clientsList = new Dictionary<string, TcpClient>();
        static int bufferSize = 4096;
        static List<Thread> threads = new List<Thread>();

        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(8888);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            try
            {
                serverSocket.Start();
                Console.WriteLine("Secure Chat Server Started...");
                while (true)
                {
                    counter += 1;
                    clientSocket = serverSocket.AcceptTcpClient();

                    byte[] bytesFrom = new byte[bufferSize];
                    var networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    var clientName = Encoding.ASCII.GetString(bytesFrom);
                    clientName = clientName.Substring(0, clientName.IndexOf("$"));
                    clientsList.Add(clientName, clientSocket);
                    Broadcast(clientName + " has joined the chat ", clientName, false);
                    Console.WriteLine(clientName + " has joined the chat");
                    var t = new Thread(() => ChatWith(clientSocket, clientName, (name, msg) => Broadcast(msg, name, true)));
                    t.Start();
                    threads.Add(t);
                }
            }
            finally
            {
                clientSocket?.Close();
                serverSocket?.Stop();
            }
            
            Console.WriteLine("exit");
            Console.ReadLine();
        }

        public static void ChatWith(TcpClient client, string name, Action<string, string> send)
        {
            byte[] bytesFrom = new byte[bufferSize];

            while (true)
            {
                try
                {
                    var networkStream = client.GetStream();
                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    var msg = Encoding.ASCII.GetString(bytesFrom);
                    msg = msg.Substring(0, msg.IndexOf("$"));
                    Console.WriteLine(name + " says : " + msg);

                    send(name, msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Broadcast(string msg, string clientName, bool isChatMessage)
        {
            foreach (var client in clientsList)
            {
                try
                {
                    var broadcastStream = client.Value.GetStream();
                    byte[] broadcastBytes = null;

                    if (isChatMessage)
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(clientName + " says : " + msg);
                    }
                    else
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(msg);
                    }

                    broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                    broadcastStream.Flush();
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        } 

    }
}

    

