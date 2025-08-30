using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


    class HttpServer
    {
        public static HttpListener listener;
        public static int TotalTranslated { get; set; } = 0;
        public static int TotalCount { get; set; } = 0;
        public static DateTime EstimateTime { get; set; } = DateTime.MinValue;
        public static string pageData =
                "<!DOCTYPE>" +
                "<html>" +
                "  <head>" +
                "    <title>Total Translated</title>" +
                "  </head>" +
                "  <body>" +
                "    <p>Total Translated: {0}</p>" +
                "    <p>Total Items: {1}</p>" +
                "    <p>Estimated Time left: {2}</p>" +
                "  </body>" +
                "</html>";
        public static bool runServer = true;

    public static async Task HandleIncomingConnections()
        {
            

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request

                //Console.WriteLine(req.Url.ToString());
                //Console.WriteLine(req.HttpMethod);
                //Console.WriteLine(req.UserHostName);
                //Console.WriteLine(req.UserAgent);
                //Console.WriteLine();

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    //Console.WriteLine("Shutdown requested");
                    runServer = false;
                }

                // Make sure we don't increment the page views counter if `favicon.ico` is requested


                // Write the response info
                byte[] data = Encoding.UTF8.GetBytes(String.Format(pageData, TotalTranslated, TotalCount, EstimateTime.ToString("s")));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }
    }
