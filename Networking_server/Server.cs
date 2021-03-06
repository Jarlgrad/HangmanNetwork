﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangman;

namespace Networking_server
{
    public class Server
    {
        public List<ClientHandler> clients = new List<ClientHandler>();
        bool _gameOn = false;


        public void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            Console.WriteLine("Server up and running, waiting for messages...");
            // Todo: 
            Game game = new Game();

            try
            {
                listener.Start();

                while (true)
                {
                    TcpClient c = listener.AcceptTcpClient();
                    ClientHandler newClient = new ClientHandler(c, this);
                    clients.Add(newClient);

                    Thread clientThread = new Thread(newClient.Run);
                    clientThread.Start();

                    if (clients.Count > 2 && _gameOn == false)
                    {
                        //game.StartGame();
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

            foreach (ClientHandler tmpClient in clients)
            {
               
                
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write($"{message}");
                    w.Flush();
                

            }
        }

        public void Broadcast(ClientHandler client, string message)
        {
            foreach (ClientHandler tmpClient in clients)
            {
                if (tmpClient != client)
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write($"{client.Name}: {message}");
                    w.Flush();
                }
                else if (clients.Count() == 1)
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write("Sorry, you are alone...");
                    w.Flush();
                }
            }
        }

        public void BroadcastPrivate(ClientHandler client, string message, string clientRecieving)
        {
            foreach (ClientHandler tmpClient in clients)
            {
                if (tmpClient.Name.ToLower() == clientRecieving.ToLower())
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write($"{client.Name} (private): {message}");
                    w.Flush();
                }
                else if (clients.Count() == 1)
                {
                    NetworkStream n = tmpClient.tcpclient.GetStream();
                    BinaryWriter w = new BinaryWriter(n);
                    w.Write("Sorry, you are alone...");
                    w.Flush();
                }
            }
        }

        public void DisconnectClient(ClientHandler client)
        {
            clients.Remove(client);
            Console.WriteLine($"{client.Name} has left the building...");
            Broadcast(client, $"{client.Name} has left the building...");
        }
    }
}
