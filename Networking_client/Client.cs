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

            //client = new TcpClient("192.168.220.107", 5000);
            client = new TcpClient(localIP, 5000);
            ClientVCT = new VCTProtocol { Version = "0.1", Player = new Player(), AllGuesses = new List<char>(), IncorrectGuesses = new List<char>(), Players = new List<string>() };

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

                    if (ClientVCT.Guess != '\0')
                    {
                        Console.Clear();
                        DrawGameStats(ClientVCT);
                        Console.Write($"{ClientVCT.Player.Name} {Environment.NewLine}gissade på {ClientVCT.Guess} - {ClientVCT.Message} {Environment.NewLine}{Environment.NewLine}Gissa på en ny bokstav: ");
                    }
                    else
                        Console.WriteLine(ClientVCT.Message);
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

            if (clientVCT.Players != null)
            {
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
                        ClientVCT.Guess = message[0];
                        var tmpJson = JsonConvert.SerializeObject(ClientVCT);
                        w.Write(tmpJson);
                        w.Flush();
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
