namespace LLMTranslate
{
    public class vLLMServerOptions
    {
        bool Generate;
        bool Chat;
        int seed;
    }
    public class vLLMServer
    {
        private ServerInfo serverInfo;
        private vLLMServerOptions options;
        public vLLMServer(ServerInfo info)
        {
            serverInfo = info;
            options = new vLLMServerOptions();
        }

        public vLLMServer(ServerInfo info, vLLMServerOptions serverOptions)
        {
            serverInfo = info;
            options = serverOptions;
        }

    }
}