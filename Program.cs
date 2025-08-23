using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LLMTranslate
{
    public class Program
    {
        public static List<string> originalLines = new List<string>();
        public static List<string> translatedLines = new List<string>();
        private static ServerTasking tasking;

        public static void FillOriginalLines()
        {
            if (File.Exists("LLMTranslateLines.txt"))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                byte[] fileBytes = File.ReadAllBytes("LLMTranslateLines.txt");
                Encoding encoding = Encoding.GetEncoding("Shift-JIS");

                string decodedText = encoding.GetString(fileBytes);
                StringReader reader = new StringReader(decodedText);

                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line != String.Empty)
                    {
                        originalLines.Add(line);
                        translatedLines.Add(String.Empty);
                    }
                    line = reader.ReadLine();
                }
            }
        }
        public static void Main(string[] args)
        {
            tasking = new ServerTasking(originalLines.Count);
            // handle commandline args
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");
            app.MapPost("/Add", async (HttpContext context) =>
            {
                if(context.Request.HasJsonContentType())
                {
                    ServerInfo? info;
                    StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                    string Jsonstring = await reader.ReadToEndAsync();
                    JsonNode? JsonRoot = JsonNode.Parse(Jsonstring);
                    JsonNode serverInfo = JsonRoot["ServerInfo"];
                    JsonNode serverOpts = JsonRoot["Options"];
                    if (serverInfo != null )
                    {
                        info = JsonSerializer.Deserialize<ServerInfo>(serverInfo);
                        //tasking.AddServer(info, serverOpts);
                    }
                    return Results.Ok();
                }
                return Results.BadRequest();
            });

            app.Run();
        }

        
    }
}
