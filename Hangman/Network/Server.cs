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

        public void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            Console.WriteLine("Server up and running, waiting for players...");
            Game game = new Game(this);

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

                    if (players.Count > 1 && _gameOn == false)
                    {
                        game.StartGame();
                        _gameOn = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
        public void Broadcast(VCTProtocol tmpInput)
        {

            foreach (PlayerHandler tmpClient in players)
            {
                NetworkStream n = tmpClient.tcpclient.GetStream();
                BinaryWriter w = new BinaryWriter(n);
                w.Write($"{tmpInput.Message}");
                w.Flush();
            }
        }

        public void BroadcastNewUser(VCTProtocol tmpInput)
        {
            foreach (PlayerHandler tmpClient in players)
            {
                if (tmpClient.Name != tmpInput.Player.Name)
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);

                    w.Write($"{tmpInput.Player.Name}: {tmpInput.Message}");
                    w.Flush();
                }
                else if (players.Count() == 1)
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write("You are the first, waiting for more players...");
                    w.Flush();
                }
            }
        }

        public void BroadcastPrivate(PlayerHandler client, string message, string clientRecieving)
        {
            foreach (PlayerHandler tmpClient in players)
            {
                if (tmpClient.Name.ToLower() == clientRecieving.ToLower())
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write($"{client.Name} (private): {message}");
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
            Console.WriteLine($"{client.Name} has left the building...");
            VCTProtocol tmpVCT = new VCTProtocol
            {
                Message = $"{client.Name} har lämnat spelet"
            };
            Broadcast(tmpVCT);
        }
    }
}

