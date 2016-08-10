using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hangman
{
    public class PlayerHandler
    {
        public TcpClient tcpclient;
        private Server myServer;

        public string Name { get; set; }
        public bool WonGame { get; set; }
        public int Wins { get; set; }
        public Queue<char> GuessQueue { get; set; }


        public PlayerHandler(TcpClient c, Server server, Queue<char>guessQueue)
        {
            tcpclient = c;
            this.myServer = server;
            GuessQueue = guessQueue;
        }

        public void Run()
        {
            try
            {
                string message = "";
                bool firstLogin = true;

                while (firstLogin)
                {
                    SetUserName();
                    firstLogin = false;
                }


                while (!message.Equals("quit"))
                {

                    NetworkStream n = tcpclient.GetStream();
                    message = new BinaryReader(n).ReadString();
                    
                    if (message.Split(' ')[0].ToLower().Equals("pm"))
                    {
                        var msg = "";
                        for (int i = 0; i < message.Split(' ').Length; i++)
                        {
                            if (i > 1)
                            {
                                msg += message.Split(' ')[i] + " ";
                            }
                        }
                        myServer.BroadcastPrivate(this, msg, message.Split(' ')[1]);
                    }
                    else if (message.Length == 1)
                    {
                       GuessQueue.Enqueue(message[0]);
                    }
                    else
                    {
                        myServer.Broadcast(this, message);
                    }
                    Console.WriteLine(message);
                }

                myServer.DisconnectClient(this);
                tcpclient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SetUserName()
        {

            NetworkStream n = tcpclient.GetStream();
            BinaryWriter w = new BinaryWriter(n);
            w.Write("Hello new user. Please set a username: ");
            string username = new BinaryReader(n).ReadString();
            Name = username;
            myServer.Broadcast(this, $"has joined the game!");
            w.Flush();

        }
    }
}
