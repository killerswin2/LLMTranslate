namespace LLMTranslate
{
    // Not really a server, but it does help with processing on google servers, needs an API key
    public class GeminiServerOptions
    {
        public bool Generate { get; set; } = false;
        public bool Chat { get; set; } = false;
        public int seed { get; set; } = -1;
        public string model { get; set; } = String.Empty;
        public bool stream { get; set; } = false;
        public bool think { get; set; } = false;

    }
    public class GoogleGemini
    {
        public GoogleGemini()
        {

        }
    }
}
