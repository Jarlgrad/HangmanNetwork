using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hangman
{
    public class PlayerHandler
    {
        public TcpClient tcpclient;
        private Server myServer;

        public string Name { get; set; }
        public bool WonGame { get; set; }
        public int Wins { get; set; }
        public Queue<VCTProtocol> GuessQueue { get; set; }


        public PlayerHandler(TcpClient c, Server server, Queue<VCTProtocol> guessQueue)
        {
            tcpclient = c;
            this.myServer = server;
            GuessQueue = guessQueue;
        }

        public void Run()
        {
            bool firstLogin = true;

            while (firstLogin)
            {
                SetUserName();
                firstLogin = false;
            }

            try
            {
                string message = "";



                while (!message.Equals("quit"))
                {
                    NetworkStream n = tcpclient.GetStream();
                    message = new BinaryReader(n).ReadString();
                    message = Console.ReadLine();
                    // Todo: Privata meddelanden och byta namn, om det finns tid
                    //
                    //if (message.Split(' ')[0].ToLower().Equals("pm"))
                    //{
                    //    var msg = "";
                    //    for (int i = 0; i < message.Split(' ').Length; i++)
                    //    {
                    //        if (i > 1)
                    //        {
                    //            msg += message.Split(' ')[i] + " ";
                    //        }
                    //    }
                    //    myServer.BroadcastPrivate(this, msg, message.Split(' ')[1]);
                    //}
                    //else if (message.Split(' ')[0].ToLower().Equals("name"))
                    //{
                    //    VCTProtocol tmpInput = new VCTProtocol { Player = this, Message=$"{this.Name} changed their name to {message.Split(' ')[1]}", Version = "0.3" };
                    //    this.Name = message.Split(' ')[1];
                    //    var tmpJson = JsonConvert.SerializeObject(tmpInput);

                    //    myServer.Broadcast(this, message);

                    //}
                    if (message.Length == 1)
                    {
                        VCTProtocol tmpInput = JsonConvert.DeserializeObject<VCTProtocol>(message);
                        tmpInput.Player = this;
                        GuessQueue.Enqueue(tmpInput);
                    }
                    else
                    {
                        VCTProtocol tmpInput = JsonConvert.DeserializeObject<VCTProtocol>(message);
                        tmpInput.Player = this;
                        myServer.Broadcast(tmpInput);
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
            w.Flush();
            string username = new BinaryReader(n).ReadString();
            VCTProtocol tmpInput = JsonConvert.DeserializeObject<VCTProtocol>(username);
            Console.WriteLine(tmpInput.Message);
            tmpInput.Player = this;
            Name = tmpInput.Message;
            myServer.BroadcastNewUser(tmpInput);
        }
    }
}
