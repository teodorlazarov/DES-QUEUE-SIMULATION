using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTake
{
    public static class ListHelper
    {
        public static Event GetMin(this List<Event> list)
        {
            Event minTime = list.First();
            foreach (Event currentevent in list)
            {
                if (minTime.Time > currentevent.Time)
                {
                    minTime = currentevent;
                }
            }
            return minTime;
        }
    }
}
