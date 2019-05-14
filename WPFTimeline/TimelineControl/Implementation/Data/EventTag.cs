using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimelineControl.Implementation.Data
{
    /// <summary>
    /// Evnet标签
    /// </summary>
    public class EventTag
    {
        public HashSet<string> Values { get; } = new HashSet<string>();

        public string Value
        {
            get => String.Join("|",Values);
            set {
                Values.Clear();
                foreach(var s in value.Split('|'))
                {
                    Values.Add(s);
                }
            }
        }

        public string TagType { get; set; }

        public EventTag(string type,string value)
        {
            TagType = type;
            Value = value;
        }
    }
}
