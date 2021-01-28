using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleHttpServer
{
    public static class HttpServer
    {
        public static HttpListener listener;
        public static string url = "http://localhost:4567/";
        public static int requestCount = 0;
        public static string pageData = "";


        private static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)

            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                if (req.Url.AbsolutePath == "/favicon.ico")
                    continue;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine(req.ContentType);
                Console.WriteLine("------------------------------");

                //// If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                //if ((req.HttpMethod == "POST") && 
                //    (req.Url.AbsolutePath == "/shutdown"))
                //{
                //    Console.WriteLine("Shutdown requested");
                //    runServer = false;
                //}

                //todo

                // Write out to the response stream (asynchronously), then close it
                await ParseContext.WriteData(req, resp);
                resp.Close();
            }
        }

        public static void Start()
        {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            //Register refection
            Refection.RegisterFunctions();
            Refection.RegisterRoutes();

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}
