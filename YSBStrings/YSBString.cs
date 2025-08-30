using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pastel;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using YSBStrings;

public class OllamaMessage
{
    public string? role { get; set; }
    public string? content { get; set; }
}
public class OllamaPrompt
{
    public string? model { get; set; }
    public List<OllamaMessage>? messages { get; set; }
    public bool stream { get; set; }
    public bool think {  get; set; }

}

public class OllamaPromptGenerate
{
    public string? model { get; set; }
    public string? prompt { get; set; }
    public bool stream { get; set; }

}

public class TranslateCSVLine
{
    public string? OriginalText { get; set; } = String.Empty;
    public string? GoogleTranslated { get; set; } = String.Empty;
    //"huihui_ai/dolphin3-abliterated 
    public string? Dolphin3Translated { get; set; } = String.Empty;
    public string? FinalText { get; set; } = String.Empty;

    public override string ToString()
    {
        return string.Format("{0}#{1}#{2}", OriginalText, GoogleTranslated, Dolphin3Translated);
    }
}

public class TranslateCSVContainer
{
    public int index { get; set; } = -1;
    public TranslateCSVLine? Line { get; set; }
    public override string ToString()
    {
        return string.Format("INDEX:{0}#{1}", index, Line.ToString());
    }
}

class BinaryHelper
{
    private static object _MessageLock = new object();
    private static ConcurrentQueue<int> queueIndex = new ConcurrentQueue<int>();
    private static List<TranslateCSVLine> csvLines = new List<TranslateCSVLine>();
    private static bool runCSVThread = true;
    // used for count of iterations that are good, used namely on GetCompCalc.
    private static int iterationNum = 0;
    private static int idx = 0;
    private static List<TranslateCSVContainer> translationLines = new List<TranslateCSVContainer>();


    public static string ProcessTranslationUserPrompt(string originalText)
    {
        string? PromptRough = File.ReadAllText("TranslationPrompt.txt");
        PromptRough = PromptRough.Replace("${source_lang}", "Japanese");
        PromptRough = PromptRough.Replace("${target_lang}", "English");
        PromptRough = PromptRough.Replace("${tagged_text}", originalText);
        PromptRough = PromptRough.Replace("${chunk_to_translate}", originalText);
        return PromptRough;
    }

    public static string ProcessTranslationReflection(string originalText, string Translation)
    {
        string? PromptRough = File.ReadAllText("ReflectionTranslation.txt");
        PromptRough = PromptRough.Replace("${source_lang}", "Japanese");
        PromptRough = PromptRough.Replace("${target_lang}", "English");
        PromptRough = PromptRough.Replace("${source_text}", originalText);
        PromptRough = PromptRough.Replace("${translation_1}", Translation);
        return PromptRough;
    }

    public static string ProcessTranslationImprove(string originalText, string Translation, string Reflection)
    {
        string? PromptRough = File.ReadAllText("ImproveTranslation.txt");
        PromptRough = PromptRough.Replace("${source_lang}", "Japanese");
        PromptRough = PromptRough.Replace("${target_lang}", "English");
        PromptRough = PromptRough.Replace("${source_text}", originalText);
        PromptRough = PromptRough.Replace("${translation_1}", Translation);
        PromptRough = PromptRough.Replace("${reflection}", Reflection);
        return PromptRough;
    }

    public static void OllamaThreadFunction(int offset, int inc, string ip)
    {
        for (int i = offset; i < translationLines.Count; i = i + inc)
        {
            SendOllama(translationLines[i].Line, ip);
            csvLines[translationLines[i].index] = translationLines[i].Line;
            queueIndex.Enqueue(translationLines[i].index);
        }
    }

    public static void SendOllama(TranslateCSVLine line, string ip)
    {
        var options = new RestClientOptions($"http://{ip}:11434/api/generate")
        {
            Authenticator = new HttpBasicAuthenticator("username", "password")

        };

        var client = new RestClient(options);


        //OllamaPrompt promptTranslation = new OllamaPrompt() { model = "huihui_ai/qwen3-abliterated:16b", messages = new List<OllamaMessage>(), stream = false, think = false };
        //promptTranslation.messages.Add(new OllamaMessage() {role = "system", content = "You are an expert linguist specializing in translations from Japanese to English" });
        //promptTranslation.messages.Add(new OllamaMessage() { role = "user", content = ProcessTranslationUserPrompt(line.OriginalText)});

        OllamaPromptGenerate promptTranslation = new OllamaPromptGenerate() { model = "hf.co/mradermacher/vntl-llama3-8b-v2-hf-GGUF:Q6_K", prompt = line.OriginalText, stream = false };


        string jsonString = System.Text.Json.JsonSerializer.Serialize(promptTranslation);
        var requestTranslate = new RestRequest("", Method.Post).AddJsonBody(jsonString);
        try
        {
            var response = client.Execute(requestTranslate);
            if ((!response.IsSuccessful))
            {
                DateTime now = DateTime.Now;
                lock (_MessageLock)
                {
                    //Console.WriteLine($"[{"Ollama".Pastel(Color.FromArgb(168, 155, 157))}] Status: {response.StatusCode.ToString().Pastel(Color.FromArgb(207, 207, 234))}");
                    //Console.WriteLine($"[{"Ollama".Pastel(Color.FromArgb(168, 155, 157))}] Error Message: {response.ErrorMessage.Pastel(Color.FromArgb(207, 207, 234))}");
                    System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Ollama"}] Status: {response.StatusCode.ToString()}\n");
                    System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Ollama"}] Error Message: {response.ErrorMessage}\n");

                }
                return;
            }
            // need to do something here about the response content being empty.
            JObject json = JObject.Parse(response.Content);
            //json["message"]["content"]

            string firstTranslation = json["response"].ToString().ReplaceLineEndings("");
            line.Dolphin3Translated = firstTranslation;
            //int index = firstTranslation.IndexOf("</think>");
            //if(index != -1)
            //{
            //    firstTranslation = firstTranslation.Substring(index+8);
            //}

            //OllamaPrompt promptTranslationReflection = new OllamaPrompt() { model = "huihui_ai/qwen3-abliterated:16b", messages = new List<OllamaMessage>(), stream = false, think = false };
            //promptTranslation.messages.Add(new OllamaMessage() { role = "system", content = "You are an expert linguist specializing in translations from Japanese to English.\nYou will be provided with a source text and its translation and your goal is to improve the translation." });
            //promptTranslation.messages.Add(new OllamaMessage() { role = "user", content = ProcessTranslationReflection(line.OriginalText, firstTranslation) });


            //string jsonStringReflection = System.Text.Json.JsonSerializer.Serialize(promptTranslationReflection);

            //var requestReflection = new RestRequest("", Method.Post).AddJsonBody(jsonStringReflection);
            //response = client.Execute(requestReflection);
            //if ((!response.IsSuccessful))
            //{
            //    DateTime now = DateTime.Now;
            //    lock (_MessageLock)
            //    {
            //        //Console.WriteLine($"[{"Ollama".Pastel(Color.FromArgb(168, 155, 157))}] Status: {response.StatusCode.ToString().Pastel(Color.FromArgb(207, 207, 234))}");
            //        //Console.WriteLine($"[{"Ollama".Pastel(Color.FromArgb(168, 155, 157))}] Error Message: {response.ErrorMessage.Pastel(Color.FromArgb(207, 207, 234))}");
            //        System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Ollama"}] Status: {response.StatusCode.ToString()}\n");
            //        System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Ollama"}] Error Message: {response.ErrorMessage}\n");

            //    }
            //    return;
            //}

            //json = JObject.Parse(response.Content);
            //string translationReflection = json["message"]["content"].ToString().ReplaceLineEndings("");

            //index = translationReflection.IndexOf("</think>");
            //if (index != -1)
            //{
            //    translationReflection = translationReflection.Substring(index + 8);
            //}


            //OllamaPrompt promptTranslationImprove = new OllamaPrompt() { model = "huihui_ai/qwen3-abliterated:16b", messages = new List<OllamaMessage>(), stream = false, think = false };
            //promptTranslation.messages.Add(new OllamaMessage() { role = "system", content = "You are an expert linguist specializing in translation editing from Japanese to English" });
            //promptTranslation.messages.Add(new OllamaMessage() { role = "user", content = ProcessTranslationImprove(line.OriginalText, firstTranslation, translationReflection) });


            //string jsonStringImprove = System.Text.Json.JsonSerializer.Serialize(promptTranslationImprove);

            //var requestImprove = new RestRequest("", Method.Post).AddJsonBody(jsonStringImprove);
            //response = client.Execute(requestImprove);
            //if ((!response.IsSuccessful))
            //{
            //    DateTime now = DateTime.Now;
            //    lock (_MessageLock)
            //    {
            //        //Console.WriteLine($"[{"Ollama".Pastel(Color.FromArgb(168, 155, 157))}] Status: {response.StatusCode.ToString().Pastel(Color.FromArgb(207, 207, 234))}");
            //        //Console.WriteLine($"[{"Ollama".Pastel(Color.FromArgb(168, 155, 157))}] Error Message: {response.ErrorMessage.Pastel(Color.FromArgb(207, 207, 234))}");
            //        System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Ollama"}] Status: {response.StatusCode.ToString()}\n");
            //        System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Ollama"}] Error Message: {response.ErrorMessage}\n");

            //    }
            //    return;
            //}


            //json = JObject.Parse(response.Content);
            //string translationImproved = json["message"]["content"].ToString().ReplaceLineEndings("");

            //index = translationImproved.IndexOf("</think>");
            //if (index != -1)
            //{
            //    translationImproved = translationImproved.Substring(index + 8);
            //}

            //if (translationImproved == String.Empty)
            //{
            //    line.Dolphin3Translated = firstTranslation;
            //}
            //else
            //{
            //    line.Dolphin3Translated = translationImproved;
            //}


        }
        catch (Exception error)
        {
            System.IO.File.AppendAllText("Error.log", $"Error from exception: {error}");
            Console.WriteLine(error);
        }
    }

    public static void SendGoogle(TranslateCSVLine line)
    {
        String RestGETURL = String.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl=ja&tl=en&dt=t&q={0}", line.OriginalText);
        var options = new RestClientOptions(RestGETURL)
        { };

        var client = new RestClient(options);
        var request = new RestRequest("", Method.Get);
        try
        {
            var response = client.Execute(request);
            if ((!response.IsSuccessful))
            {
                DateTime now = DateTime.Now;
                lock (_MessageLock)
                {
                    Console.WriteLine($"[{"Google".Pastel(Color.FromArgb(87, 90, 75))}] Status: {response.StatusCode.ToString().Pastel(Color.FromArgb(207, 207, 234))}");
                    Console.WriteLine($"[{"Google".Pastel(Color.FromArgb(87, 90, 75))}] Error Message: {response.ErrorMessage.Pastel(Color.FromArgb(207, 207, 234))}");
                    System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Google"}] Status: {response.StatusCode.ToString()}\n");
                    System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Google"}] Error Message: {response.ErrorMessage}\n");
                    System.IO.File.AppendAllText("Error.log", $"{now.ToString("s")} [{"Google"}] GET URL SENT: {RestGETURL}\n");
                }
                return;
            }
            JArray json = JArray.Parse(response.Content);
            //Console.WriteLine($"{json[0][0][0]}");
            line.GoogleTranslated = json[0][0][0].ToString();


        }
        catch (Exception error)
        {
            Console.WriteLine(error);
        }
    }
    public static bool GoodSpawnTread(TranslateCSVLine csvLine)
    {
        return !(csvLine.OriginalText == "/HF" || csvLine.OriginalText == "/SF" || csvLine.OriginalText.Contains(".ysc"));
    }

    public static void CSVWriterThreadFunc()
    {
        while (runCSVThread)
        {
            int result;
            bool success = queueIndex.TryDequeue(out result);
            while (success)
            {
                System.IO.File.AppendAllText("Temp.csv", $"INDEX:{result}#{csvLines[result].ToString()}\n");
                success = queueIndex.TryDequeue(out result);
            }
        }
    }

    public static void ServerThreadFunc()
    {
        HttpServer.listener.Start();
        Task handler = HttpServer.HandleIncomingConnections();
        handler.GetAwaiter().GetResult();
    }

    public static void ProcessExitHandler(object? sender, EventArgs args)
    {
        //System.IO.File.AppendAllText("SystemEnd.log", $"{idx}\n{iterationNum}\n");
    }

    // this function populates the csv lines after a restart if the SystemEnd.log file exist.
    public static void PopulatedCSVLines()
    {
        


        string[] list = File.ReadAllLines("Temp.csv");
        for(int i = 0; i < list.Length; i++)
        {
            string[] splitLine = list[i].Split('#');
            if (splitLine[0] == csvLines[i].OriginalText)
            {
                csvLines[i].GoogleTranslated = splitLine[1];
                csvLines[i].Dolphin3Translated = splitLine[2];
                idx++;
                if (GoodSpawnTread(csvLines[i]))
                {
                    iterationNum++;
                }
            }
        }

        // delete the temp file now.
        File.Copy("Temp.csv", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}-Temp.csv");
        File.Delete("Temp.csv");

        // now create it again to the point of success.
        for(int i = 0; i < idx; i++)
        {
            System.IO.File.AppendAllText("Temp.csv", csvLines[i].ToString() + "\n");
        }

    }

    public static void Main(string[] args)
    {

        // event handlers
        AppDomain.CurrentDomain.ProcessExit += ProcessExitHandler;

        int workerThreads;
        int portThreads;
        ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
        //Console.WriteLine($"Max Threads for pool: {workerThreads}\nCompletion port threads: {portThreads}");
        HttpServer.listener = new HttpListener();
        HttpServer.listener.Prefixes.Add("http://+:11111/count/");

        //Console.OutputEncoding = Encoding.UTF8;
        //Console.WriteLine("Beginning String Transformation to CSV");

        //csvString.Append("Location, Original String, Translated String\n");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding shiftJisEncoding = Encoding.GetEncoding("Shift-JIS");

        string filepath = "D:\\cs_code\\Binary_helper\\ScriptOrdered.txt";

        byte[] fileBytes = File.ReadAllBytes(filepath);
        //StreamReader reader = new StreamReader(filepath, shiftJisEncoding);

        string decodedText = shiftJisEncoding.GetString(fileBytes);
        //Console.WriteLine(fileBytes.Length);

        StringReader reader = new StringReader(decodedText);

        string line = reader.ReadLine();
        while (line != null)
        {
            if (line != String.Empty)
            {
                csvLines.Add(new TranslateCSVLine() { OriginalText = line });
            }
            line = reader.ReadLine();
        }

        // this is the count of data that actually needs to be processed in the main loop, usefully for calculating completion times.
        int dataLines = 0;


        // alright we have weird data that we can preprocess instead of doing in the the main loop.
        for (int i = 0; i < csvLines.Count; i++)
        {
            if (!GoodSpawnTread(csvLines[i]))
            {
                csvLines[i].GoogleTranslated = csvLines[i].OriginalText;
                csvLines[i].Dolphin3Translated = csvLines[i].OriginalText;
                csvLines[i].FinalText = csvLines[i].OriginalText;
                dataLines++;
            }
            else
            {
                translationLines.Add(new TranslateCSVContainer() { index = i, Line = csvLines[i] });
            }
        }

        HttpServer.TotalCount = csvLines.Count - dataLines;
        Thread ServerThread = new Thread(new ThreadStart(() => ServerThreadFunc()));
        ServerThread.Start();

        Thread CSVWriterThread = new Thread(new ThreadStart(() => CSVWriterThreadFunc()));
        CSVWriterThread.Start();


        DateTime totalStartTime = DateTime.Now;
        TimeCompletionCalc time = new TimeCompletionCalc(csvLines.Count - dataLines - 1);

        if (File.Exists("Temp.csv"))
        {
            PopulatedCSVLines();
        }

        // spawn three threads or more for later stuff.
        Thread[] threads = new Thread[3];



        threads[0] = new Thread(new ThreadStart(() => OllamaThreadFunction(0, 2, "localhost")));
        threads[1] = new Thread(new ThreadStart(() => OllamaThreadFunction(1, 2, "192.168.1.125")));
        //threads[2] = new Thread(new ThreadStart(() => OllamaThreadFunction(2, 3, "192.168.1.20")));

        threads[0].Start();
        threads[1].Start();
        //threads[2].Start();

        threads[0].Join();
        threads[1].Join();
        //threads[2].Join();


        for (; idx < csvLines.Count; idx++)
        {
            DateTime initalLoopTime = DateTime.Now;

            if (!GoodSpawnTread(csvLines[idx]))
            {
                //Task.Run(() => System.IO.File.AppendAllText("translation.csv", csvLines[i].ToString() + "\n"));
                // don't process this data, it should already be processed above.
                queueIndex.Enqueue(idx);
                continue;
            }
            else
            {


                
                Task GoogleTask = Task.Factory.StartNew(() => SendGoogle(csvLines[idx]));
                GoogleTask.Wait();
                iterationNum++;
            }

            DateTime compeletionTime = time.GetCompCalc(iterationNum);

            HttpServer.TotalTranslated = iterationNum;
            HttpServer.EstimateTime = compeletionTime;

            string percent = String.Format("{0:0.####}", (100 * ((double)(iterationNum + 1) / (double)dataLines)));
            //Console.WriteLine($"Finished Translating: {csvLines[i].OriginalText} \t{percent.Pastel(Color.FromArgb(75, 63, 114))}% Done \tEstimated time remaining: {compeletionTime.ToString("s")}");
            Task.Run(() => System.IO.File.AppendAllText("log.data", $"{DateTime.Now.ToString("s")} Finished Translating: {csvLines[idx].OriginalText} \t{percent}% Done \tEstimated time remaining: {compeletionTime.ToString("s")}" + "\n"));


        }

        DateTime totalEndTime = DateTime.Now;

        //write it all to a file.
        FileStream file = File.Create("Translation_Output.csv");

        foreach (TranslateCSVLine csvline in csvLines)
        {
            string lineStr = csvline.ToString() + "\n";
            file.Write(shiftJisEncoding.GetBytes(lineStr), 0, shiftJisEncoding.GetByteCount(lineStr));
        }
        file.Close();

        if(File.Exists("Temp.csv"))
        {
            File.Delete("Temp.csv");
        }

        //HttpServer.listener.Close();

        //Console.WriteLine("END");
        TimeSpan eTime = totalEndTime - totalStartTime;
        Task.Run(() => System.IO.File.AppendAllText("log.data", $"Total time was: {eTime:%d} days {eTime:%h} hours {eTime:%m} minutes {eTime:%s} seconds" + "\n"));
        

        HttpServer.runServer = false;
        runCSVThread = false;
    }

}
