using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimelineControl.Implementation.Data
{
    public class TimelineEventStore
    {
        private const int INDEX_CREATION_DELAY = 3000;
        private struct DateNode : IComparer<DateNode>
        {
            public DateTime Date;
            public TimelineEvent Event;

            public int Compare(DateNode x, DateNode y)
            {
                return DateTime.Compare(x.Date,y.Date);
            }
        }

        private DateNode[] m_byStart;
        private bool m_initialized;

        List<TimelineEvent> events;
        public List<TimelineEvent> Events { get => events; }


        public TimelineEventStore(List<TimelineEvent> events,bool sorted = false)
        {
            
        }

        public void Initialize(List<TimelineEvent> events,bool sorted = false)
        {
            if(!sorted)
            {
                events.Sort(new Comparison<TimelineEvent>((x, y) => DateTime.Compare(x.StartDate, y.StartDate)));
            }

            this.events = events;
            m_initialized = true;
        }
    }
}
