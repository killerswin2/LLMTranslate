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
        

        public ServerTasking(int workSize)
        {
            for (int i = 0; i < workSize; i++)
            {
                _IndexQueue.Enqueue(i);
            } 
                
        }

        public CancellationTokenSource? AddServer(ServerInfo serverInfo, JsonNode serverOpts)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            if (serverInfo.Type == "vLLM")
            {
                
                _WorkerThreads.Add(new Thread(new ThreadStart(() => WorkervLLMServerFunction(source.Token, serverInfo, serverOpts))));
                return source;
            }
            if(serverInfo.Type == "Ollama")
            {
                _WorkerThreads.Add(new Thread(new ThreadStart(() => WorkerOllamaServerFunction(source.Token, serverInfo, serverOpts))));
                return source;
            }
            if(serverInfo.Type == "Llama")
            {
                _WorkerThreads.Add(new Thread(new ThreadStart(() => WorkerLlamaServerFunction(source.Token, serverInfo, serverOpts))));
                return source;
            }
            return null;
        }

        public void RemoveServer(CancellationToken cancellationToken, ServerInfo info)
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
                    
                    string prompt = Program.originalLines[result];
                    string output = server.ExecuteGenerator(prompt);
                    Program.translatedLines[result] = output;
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
