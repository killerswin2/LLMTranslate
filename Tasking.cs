using System.Collections.Concurrent;

namespace LLMTranslate
{
    
    public class Tasking
    {
        private static List<Thread> _WorkerThreads = new List<Thread>();
        private static ConcurrentQueue<int> _IndexQueue = new ConcurrentQueue<int>();
        public static void ControllerFunction()
        {

        }
        public static void WorkerFunction()
        {

        }
    }
}
