using LLMTranslate.Components;
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
        public static TimeCompletionCalc CompletionTime;

        public static Tuple<int, int> CompletedInfo()
        {
            if (tasking == null)
            {
                return Tuple.Create(0, 0);
            }

            int remaining = tasking._IndexQueue.Count;
            int completed = originalLines.Count - remaining;
            return Tuple.Create(completed, remaining);
        }


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
            FillOriginalLines();
            tasking = new ServerTasking(originalLines.Count);
            CompletionTime = new TimeCompletionCalc(originalLines.Count - 1);
            // handle commandline args
            var builder = WebApplication.CreateBuilder(args);
            // listen to any connection
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            var app = builder.Build();
            
            // listen to any connection, DANGER if used WRONG
            app.Urls.Add("http://*:5000");

            //app.MapGet("/", () => "Hello World!");
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
                        tasking.AddServer(info, serverOpts);
                    }
                    return Results.Ok();
                }
                return Results.BadRequest();
            });
            app.MapGet("/status", () => "nope");

            //app.UseHttpsRedirection();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
            app.Run();
        }
        public static void PrintResults()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("Shift-JIS");
            FileStream file = File.Create("Corrections.csv");
            for (int i = 0; i < originalLines.Count; i++)
            {
                string lineStr = $"{originalLines[i]}#{translatedLines[i]}\n";
                file.Write(encoding.GetBytes(lineStr), 0, encoding.GetByteCount(lineStr));
            }
            file.Close();
        }
        
    }
}
