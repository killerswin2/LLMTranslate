using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace LLMTranslate
{

    public class ServerTasking
    {
        private List<Thread> _WorkerThreads = new List<Thread>();
        private List<ServerInfo> _Servers = new List<ServerInfo>();
        public ConcurrentQueue<int> _IndexQueue = new ConcurrentQueue<int>();
        

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
                var thread = new Thread(new ThreadStart(() => WorkervLLMServerFunction(source.Token, serverInfo, serverOpts)));
                _WorkerThreads.Add(thread);
                thread.Start();

                return source;
            }
            if(serverInfo.Type == "Ollama")
            {
                var thread = new Thread(new ThreadStart(() => WorkerOllamaServerFunction(source.Token, serverInfo, serverOpts)));
                _WorkerThreads.Add(thread);
                thread.Start();

                return source;
            }
            if(serverInfo.Type == "Llama")
            {
                var thread = new Thread(new ThreadStart(() => WorkerLlamaServerFunction(source.Token, serverInfo, serverOpts)));
                _WorkerThreads.Add(thread);
                thread.Start();

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

            while (!cancellationToken.IsCancellationRequested && !_IndexQueue.IsEmpty)
            {
                int result;
                bool success = _IndexQueue.TryDequeue(out result);
                if (success)
                {
                    
                    string output = String.Empty;
                    string prompt = Program.originalLines[result];
                    if(server.IsGenerate())
                    {
                         output = server.ExecuteGenerator(prompt);

                    }
                    else
                    {
                        output = server.ExecuteChat(prompt);
                    }
                    Program.translatedLines[result] = output;
                    Program.CompletionTime.GetCompCalcThreaded();
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
                    Program.CompletionTime.GetCompCalcThreaded();
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
                    Program.CompletionTime.GetCompCalcThreaded();
                }
            }
        }
    }
}
