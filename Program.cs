namespace LLMTranslate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // handle commandline args
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
