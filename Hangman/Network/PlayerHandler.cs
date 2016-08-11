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
                    var tempVCT = new VCTProtocol();
                    tempVCT = JsonConvert.DeserializeObject<VCTProtocol>(message);
                    tempVCT.Player = this;

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

                    if (tempVCT.Guess != '\0')
                    {
                        GuessQueue.Enqueue(tempVCT);
                        Console.WriteLine(tempVCT);
                    }
                    else
                    {
                        myServer.Broadcast(tempVCT);
                        Console.WriteLine(tempVCT);
                    }
                }

                myServer.DisconnectClient(this);
                tcpclient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "i metoden Run i PlayerHandler");
            }
        }

        public void SetUserName()
        {
            Console.WriteLine("Nu välkomnas en ny användare");
            NetworkStream n = tcpclient.GetStream();
            BinaryWriter w = new BinaryWriter(n);
            var tempVCT = new VCTProtocol();
            tempVCT.Message = "Välkommen, sätt ett användarnamn";
            var tmpMsg = JsonConvert.SerializeObject(tempVCT);
            w.Write(tmpMsg);
            w.Flush();
            Console.WriteLine("Nu väntar servern in ett användarnamn");
            string username = new BinaryReader(n).ReadString();
            tempVCT = JsonConvert.DeserializeObject<VCTProtocol>(username);
            this.Name = tempVCT.Message;
            tempVCT.Player = this;
            Console.WriteLine($"User {tempVCT.Player.Name} set username");
            myServer.BroadcastNewUser(tempVCT);
        }
    }
}
