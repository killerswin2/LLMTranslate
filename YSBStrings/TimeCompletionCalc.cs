using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSBStrings
{
    public class TimeCompletionCalc
    {
        private readonly DateTime _dateStart;
        private readonly int _maxIndex; // max index allowed, aka the total items that will be processed.
        private double _lastMSTime = 0.0;
        private Double _alpha = 0.1;


        public TimeCompletionCalc(int maxIndex)
        {
            _dateStart = DateTime.Now;
            _maxIndex = maxIndex;
        }

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
            return calc;
        }
    }
}
