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
        public bool IsChat()
        {
            return options.Chat;
        }

        public bool IsGenerate()
        {
            return options.Generate;
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
        public string ExecuteChat(string prompt)
        {
            var restOptions = new RestClientOptions($"http://{serverInfo.BaseURL}:{serverInfo.Port}/api/chat")
            {
                Authenticator = new HttpBasicAuthenticator("username", "password")

            };
            var client = new RestClient(restOptions);

            

            Chat promptTranslation = new Chat() { model = "hf.co/sugoitoolkit/Sugoi-14B-Ultra-GGUF:Q4_K_M", messages = new List<Message>(), stream = false, think = false };
            promptTranslation.messages.Add(new Message() { role = "system", content = "You are a professional localizer whose primary goal is to translate Japanese to English. You should use colloquial or slang or nsfw vocabulary if it makes the translation more accurate. Always respond in English." });
            promptTranslation.messages.Add(new Message() { role = "user", content = prompt });


            string jsonString = JsonSerializer.Serialize(promptTranslation);
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
                //System.IO.File.AppendAllText("Json2.Log", $"{DateTime.Now.ToString("s")}: {json.ToJsonString()}\n");

                string respone_message = json["message"]["content"].GetValue<string>();

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