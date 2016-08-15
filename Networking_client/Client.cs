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
        public VCTProtocol ClientVCT { get; set; }
        public List<string> ChatBox { get; set; }

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

            Console.WriteLine("GUBBHÄNG!");
            Console.WriteLine("Ange IP-Address att ansluta till");
            string connectIP = Console.ReadLine();

            client = new TcpClient(connectIP, 5000);
            //client = new TcpClient(localIP, 5000);
            ClientVCT = new VCTProtocol { Version = "0.1", Player = new Player(), AllGuesses = new List<char>(), IncorrectGuesses = new List<char>(), Players = new List<string>() };
            ChatBox = new List<string>();

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
                    ClientVCT = JsonConvert.DeserializeObject<VCTProtocol>(message);

                    if (ClientVCT.GameBoard != "")
                    {
                        if (ClientVCT.Message == "start")
                        {
                            Console.Clear();
                            DrawGameStats(ClientVCT);
                        }
                        if (ClientVCT.Guess != '\0')
                        {
                            Console.Clear();
                            DrawGameStats(ClientVCT);
                            Console.WriteLine($"{ClientVCT.Player.Name} {Environment.NewLine}gissade på {ClientVCT.Guess} - {ClientVCT.Message} {Environment.NewLine}{Environment.NewLine}Gissa på en ny bokstav: ");
                            UpdateChat();
                            ClientVCT.Guess = '\0';
                        }   
                        else
                            UpdateChat(ClientVCT);
                        //Console.WriteLine(ClientVCT.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DrawGameStats(VCTProtocol clientVCT)
        {
            int guessCursorX = 80;
            int playerCursorY = 2;

            Console.SetCursorPosition(40, 0);
            Console.Write("HÄNGA GUBBEN");
            Console.SetCursorPosition(40, 1);
            Console.Write("============");
            Console.SetCursorPosition(40, 2);
            Console.Write(clientVCT.GameBoard);

            Console.SetCursorPosition(80, 0);
            Console.Write("Runda 1      Poäng");
            Console.SetCursorPosition(80, 1);
            Console.Write("------------------");

            if (clientVCT.Players != null)
            {
                foreach (var player in clientVCT.Players)
                {
                    Console.SetCursorPosition(80, playerCursorY);
                    if (player.Length < 7)
                    {
                        Console.Write($"{player}\t\t{clientVCT.Player.Wins}");
                        playerCursorY++;
                    }
                    else
                    {
                        Console.Write($"{player} \t{clientVCT.Player.Wins}");
                        playerCursorY++;
                    }
                }
                Console.SetCursorPosition(80, ++playerCursorY);
                Console.Write("Felaktiga svar:");
                playerCursorY++;

                foreach (var item in ClientVCT.IncorrectGuesses)
                {
                    Console.SetCursorPosition(guessCursorX, playerCursorY);
                    Console.Write($"{item}.");
                    guessCursorX += 2;

                }
                Console.SetCursorPosition(0, 0);
            }
            else
            {

                Console.SetCursorPosition(80, ++playerCursorY);
                Console.Write("Felaktiga svar:");
                playerCursorY++;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Välkommen till Hang Man.");
                Console.WriteLine("En ny runda har inletts.");
                Console.WriteLine("Gissa på en bokstav!");
                ClientVCT.Message = string.Empty;
            }
        }
        private void UpdateChat()
        {
            int _chatCursorY = 12;
            if (ChatBox != null)
            {
                foreach (var item in ChatBox)
                {
                    Console.SetCursorPosition(0, _chatCursorY);
                    Console.WriteLine(item);
                    _chatCursorY++;
                }
            }
        }
        private void UpdateChat(VCTProtocol clientVCT)
        {
            int _chatCursorY = 12;
            if (ChatBox != null)
            {
                if (ChatBox.Count > 4)
                    ChatBox.RemoveAt(0);
            }

            ChatBox.Add(clientVCT.Message);

            foreach (var item in ChatBox)
            {
                Console.SetCursorPosition(0, _chatCursorY);
                Console.WriteLine(item);
                _chatCursorY++;
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
                        //Todo: för
                        //foreach (var item in tmpInput.AllGuesses)
                        //{
                        //    if (tmpInput.Guess.Equals(item))
                        //    {

                        //    }
                        //}
                        if (ClientVCT.AllGuesses != null)
                        {
                            bool alreadyGuessed = false;
                            foreach (var item in ClientVCT.AllGuesses)
                            {
                                if (item == message[0])
                                {
                                    alreadyGuessed = true;
                                }
                            }

                            if (alreadyGuessed)
                            {
                                ClientVCT.Message = "Ni har ju redan gissat på bokstaven";
                                UpdateChat(ClientVCT);
                            }
                            else
                            {
                                ClientVCT.Guess = message[0];
                                var _tmpJson = JsonConvert.SerializeObject(ClientVCT);
                                w.Write(_tmpJson);
                                w.Flush();
                            }
                        }
                    }
                    else
                    {
                        ClientVCT.Message = message;
                        var tmpJson = JsonConvert.SerializeObject(ClientVCT);
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
