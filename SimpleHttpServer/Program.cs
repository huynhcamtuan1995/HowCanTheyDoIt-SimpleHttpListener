using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create a Http server and start listening for incoming connections
            HttpServer.Start();
        }
    }
}
