using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Networking_client
{
    class Program
    {
        static void Main(string[] args)
        {


            Client myClient = new Client();

            Thread clientThread = new Thread(myClient.Start);
            clientThread.Start();
            clientThread.Join();
        }
    }
}
