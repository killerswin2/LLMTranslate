namespace LLMTranslate
{
    public class ServerInfo
    {
        public string BaseURL { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

    }

    public class Message
    {
        public string? role { get; set; }
        public string? content { get; set; }
    }
    public class Chat
    {
        public string? model { get; set; }
        public List<Message>? messages { get; set; }
        public bool stream { get; set; }
        public bool think { get; set; }

    }

    public class Generate
    {
        public string? model { get; set; }
        public string? prompt { get; set; }
        public bool stream { get; set; }

    }
}