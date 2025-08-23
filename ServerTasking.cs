using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace LLMTranslate
{

    public class ServerTasking
    {
        private List<Thread> _WorkerThreads = new List<Thread>();
        private List<ServerInfo> _Servers = new List<ServerInfo>();
        private ConcurrentQueue<int> _IndexQueue = new ConcurrentQueue<int>();
        public void MainServerFunction()
        {

        }
        public void WorkerOllamaServerFunction(CancellationToken cancellationToken, ServerInfo info, JsonNode serverOptions)
        {
            OllamaServerOptions? options = JsonSerializer.Deserialize<OllamaServerOptions>(serverOptions);
            OllamaServer server = new OllamaServer(info, options);

            while (!cancellationToken.IsCancellationRequested)
            {
                int result;
                bool success = _IndexQueue.TryDequeue(out result);
                if (success)
                {
                    string prompt = String.Empty;
                    string output = server.ExecuteGenerator(prompt);
                }
            }
        }
        public void WorkervLLMServerFunction(CancellationToken cancellationToken, ServerInfo info, JsonNode serverOptions)
        {

            while (!cancellationToken.IsCancellationRequested)
            {
                int result;
                bool success = _IndexQueue.TryDequeue(out result);
                if (success)
                {

                }
            }
        }
        public void WorkerLlamaServerFunction(CancellationToken cancellationToken, ServerInfo info, JsonNode serverOptions)
        {

            while (!cancellationToken.IsCancellationRequested)
            {
                int result;
                bool success = _IndexQueue.TryDequeue(out result);
                if (success)
                {

                }
            }
        }
    }
}
