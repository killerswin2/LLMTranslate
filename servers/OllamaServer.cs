using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LLMTranslate
{
    public class OllamaServerOptions
    {
        public bool Generate { get; set; } = false;
        public bool Chat { get; set; } = false;
        public int seed { get; set; } = -1;
        public string model { get; set; } = String.Empty;
        public bool stream { get; set; } = false;
        public bool think { get; set; } = false;

    }
    public class OllamaServer
    {
        private ServerInfo serverInfo;
        private OllamaServerOptions options;
        public OllamaServer(ServerInfo info)
        {
            serverInfo = info;
            options = new OllamaServerOptions();
        }

        public OllamaServer(ServerInfo info, OllamaServerOptions serverOptions)
        {
            serverInfo = info;
            options = serverOptions;
        }

        public string ExecuteGenerator(string prompt)
        {
            var restOptions = new RestClientOptions($"http://{serverInfo.BaseURL}:{serverInfo.Port}/api/generate")
            {
                Authenticator = new HttpBasicAuthenticator("username", "password")

            };
            var client = new RestClient(restOptions);

            Generate generatorPrompt = new Generate() { model = options.model, prompt = prompt, stream = options.stream };
            string jsonString = JsonSerializer.Serialize(generatorPrompt);
            var requestTranslate = new RestRequest("", Method.Post).AddJsonBody(jsonString);
            try
            {
                var response = client.Execute(requestTranslate);
                if ((!response.IsSuccessful))
                {
                    DateTime now = DateTime.Now;

                    //Console.WriteLine($"[{"Ollama".Pastel(Color.FromArgb(168, 155, 157))}] Status: {response.StatusCode.ToString().Pastel(Color.FromArgb(207, 207, 234))}");
                    //Console.WriteLine($"[{"Ollama".Pastel(Color.FromArgb(168, 155, 157))}] Error Message: {response.ErrorMessage.Pastel(Color.FromArgb(207, 207, 234))}");
                    System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Ollama"}] Status: {response.StatusCode.ToString()}\n");
                    System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Ollama"}] Error Message: {response.ErrorMessage}\n");

                    return String.Empty;
                }
                // need to do something here about the response content being empty.
                JsonNode? json = JsonNode.Parse(response.Content);
                //json["message"]["content"]

                string respone_message = json["response"].GetValue<string>().ReplaceLineEndings("");
                return respone_message;
            }
            catch (Exception error)
            {
                System.IO.File.AppendAllText("Error.log", $"Error from exception: {error}");
                Console.WriteLine(error);
                return String.Empty;
            }
        }
    }
}