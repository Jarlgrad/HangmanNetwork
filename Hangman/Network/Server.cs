using Newtonsoft.Json;
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
    public class Server
    {
        public List<PlayerHandler> players = new List<PlayerHandler>();
        bool _gameOn = false;
        Game game;
        public string Name { get; set; }

        public void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            Console.WriteLine("Server up and running, waiting for players...");
            game = new Game(this);

            try
            {
                listener.Start();

                while (true)
                {
                    TcpClient c = listener.AcceptTcpClient();
                    PlayerHandler newClient = new PlayerHandler(c, this, game.GuessQueue);
                    players.Add(newClient);

                    Thread clientThread = new Thread(newClient.Run);
                    clientThread.Start();
                    // Sätter trådens Name.Property
                    //Thread.CurrentThread.Name = "clientHandlerThread";


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "I metoden Run i Server");
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }

        public void StartGame()
        {
            if (players.Count > 1 && _gameOn == false)
            {
                game.StartGame();
                _gameOn = true;
                Thread queueThread = new Thread(game.GetGuessQueue);
                queueThread.Start();
                //game.GetGuessQueue();
            }
        }

        public void BroadcastNewUser(VCTProtocol tmpInput)
        {
            VCTProtocol tmpVCT = new VCTProtocol();
            var tmpString = $"{tmpInput.Player.Name} joined the room.";
            tmpInput.Message = tmpString;
            var tmpJson = JsonConvert.SerializeObject(tmpInput);

            foreach (PlayerHandler tmpClient in players)
            {
                NetworkStream n = tmpClient.tcpclient.GetStream();
                BinaryWriter w = new BinaryWriter(n);
                w.Write(tmpJson);
                if (players.Count() == 1)
                {
                    tmpVCT.Message = "You are the first, waiting for more players...";
                    w.Write(JsonConvert.SerializeObject(tmpVCT));
                }
                w.Flush();
            }
        }

        public void ServerBroadcast(VCTProtocol tmpInput)
        {
            Console.WriteLine("Nu skickas meddelande till alla");
            foreach (PlayerHandler tmpClient in players)
            {
                NetworkStream n = tmpClient.tcpclient.GetStream();
                BinaryWriter w = new BinaryWriter(n);
                var tmpJson = JsonConvert.SerializeObject(tmpInput);
                w.Write(tmpJson);
                w.Flush();
            }
        }
        public void BroadcastGuess(VCTProtocol tmpInput)
        {
            foreach (PlayerHandler tmpClient in players)
            {
                if (tmpClient.PlayerData.Player.Name != tmpInput.Player.Name)
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    var tmpJson = JsonConvert.SerializeObject(tmpInput);
                    w.Write(tmpJson);
                    w.Flush();
                }
            }
        }

        public void Broadcast(VCTProtocol tmpInput)
        {
            foreach (PlayerHandler tmpClient in players)
            {
                if (tmpClient.PlayerData.Player.Name != tmpInput.Player.Name)
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);

                    w.Write($"{tmpInput.Player.Name}: {tmpInput.Message}");
                    w.Flush();
                }
            }
        }

        public void BroadcastPrivate(PlayerHandler client, string message, string clientRecieving)
        {
            foreach (PlayerHandler tmpClient in players)
            {
                if (tmpClient.PlayerData.Player.Name.ToLower() == clientRecieving.ToLower())
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write($"{client.PlayerData.Player.Name} (private): {message}");
                    w.Flush();
                }
                else if (players.Count() == 1)
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write("Sorry, you are alone...");
                    w.Flush();
                }
            }
        }

        public void DisconnectClient(PlayerHandler client)
        {
            players.Remove(client);
            Console.WriteLine($"{client.PlayerData.Player.Name} has left the building...");
            VCTProtocol tmpVCT = new VCTProtocol
            {
                Message = $"{client.PlayerData.Player.Name} har lämnat spelet"
            };
            Broadcast(tmpVCT);
        }
    }
}

