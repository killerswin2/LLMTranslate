using Microsoft.AspNetCore.SignalR;

namespace LLMTranslate
{
    public class TimeCompletionCalc
    {
        private readonly DateTime _dateStart;
        private readonly int _maxIndex; // max index allowed, aka the total items that will be processed.
        private double _lastMSTime = 0.0;
        private Double _alpha = 0.1;
        private DateTime _lastDateTime;
        private int _currentIteration = 1;
        private readonly Lock _lockObj = new();

        public DateTime GetLastDateTime()
        {
            return _lastDateTime;
        }

        public TimeCompletionCalc(int maxIndex)
        {
            _dateStart = DateTime.Now;
            _maxIndex = maxIndex;
        }

        // this is the multi-threaded version.
        public void GetCompCalcThreaded()
        {
            lock (_lockObj)
            {
                GetCompCalc(_currentIteration);
                _currentIteration++;
            }
        }
        // NOT THREAD SAFE
        public DateTime GetCompCalc(int iterationNum)
        {

            int totalIt = _maxIndex + 1;
            var msTime = DateTime.Now.Subtract(_dateStart).TotalMilliseconds;

           
            
            // first time, base s0
            if (msTime == 0.0)
            {
                _lastMSTime = msTime;
            }
            else
            {
                msTime = _alpha * msTime + ((1 - _alpha) * _lastMSTime);
                _lastMSTime = msTime;
            }


            var TimeIt = msTime / (double)iterationNum;
            var calc = DateTime.Now.AddMilliseconds(TimeIt * (totalIt - iterationNum));

            _lastDateTime = calc;
            return calc;
        }
    }
}
