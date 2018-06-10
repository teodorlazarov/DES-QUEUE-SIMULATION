using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTake
{
    public class Event
    {
        public double Time { get; private set; }
        public int Type { get; private set; }
        public Event(int type, double time)
        {
            Time = time;
            Type = type;
        }
    }
}
