using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangman;
using Newtonsoft.Json;

namespace Networking_client
{
    class Client
    {
        private TcpClient client;
        const string Version = "0.1";

        public void Start()
        {
            #region Get local IP
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            Console.WriteLine($"IP: {localIP}");
            #endregion

            client = new TcpClient("192.168.220.92", 5000);
            //client = new TcpClient(localIP, 5000);

            Thread listenerThread = new Thread(Send);
            listenerThread.Start();

            Thread senderThread = new Thread(Listen);
            senderThread.Start();

            senderThread.Join();
            listenerThread.Join();
        }

        public void Listen()
        {
            string message = "";

            try
            {
                while (true)
                {
                    NetworkStream n = client.GetStream();
                    message = new BinaryReader(n).ReadString();
                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Send()
        {
            string message = "";

            try
            {
                NetworkStream n = client.GetStream();

                // Todo: Ge meddelande id
                while (!message.Equals("quit"))
                {
                    message = Console.ReadLine();
                    BinaryWriter w = new BinaryWriter(n);

                    //message = new BinaryReader(n).ReadString();

                    // Todo: Privata meddelanden och byta namn, om det finns tid
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
                    //    VCTProtocol tmpInput = new VCTProtocol { Player = this, Message = $"{this.Name} changed their name to {message.Split(' ')[1]}", Version = "0.3" };
                    //    this.Name = message.Split(' ')[1];
                    //    var tmpJson = JsonConvert.SerializeObject(tmpInput);

                    //    myServer.Broadcast(this, message);

                    //}

                    if (message.Length == 1)
                    {
                        VCTProtocol tmpInput = new VCTProtocol { Guess = message[0], Version = Version };
                        var tmpJson = JsonConvert.SerializeObject(tmpInput);
                        w.Write(tmpJson);
                        w.Flush();
                    }
                    else
                    {
                        VCTProtocol tmpInput = new VCTProtocol { Message = message, Version = Version };
                        var tmpJson = JsonConvert.SerializeObject(tmpInput);
                        w.Write(tmpJson);
                        w.Flush();
                    }
                }

                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
