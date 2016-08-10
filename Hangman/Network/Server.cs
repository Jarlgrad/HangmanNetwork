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
        public List<Player> players = new List<Player>();
        bool _gameOn = false;

        public void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            Console.WriteLine("Server up and running, waiting for players...");
            // Todo: 
            Game game = new Game();

            try
            {
                listener.Start();

                while (true)
                {
                    TcpClient c = listener.AcceptTcpClient();
                    Player newClient = new Player(c, this);
                    players.Add(newClient);

                    Thread clientThread = new Thread(newClient.Run);
                    clientThread.Start();

                    if (players.Count > 1 && _gameOn == false)
                    {
                        game.StartGame(players);
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
        public void Broadcast(string message)
        {

            foreach (Player tmpClient in players)
            {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write($"{message}");
                    w.Flush();
            }
        }

        public void Broadcast(Player client, string message)
        {
            foreach (Player tmpClient in players)
            {
                if (tmpClient != client)
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write($"{client.Name}: {message}");
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

        public void BroadcastPrivate(Player client, string message, string clientRecieving)
        {
            foreach (Player tmpClient in players)
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

        public void DisconnectClient(Player client)
        {
            players.Remove(client);
            Console.WriteLine($"{client.Name} has left the building...");
            Broadcast(client, $"{client.Name} has left the building...");
        }
    }
}
