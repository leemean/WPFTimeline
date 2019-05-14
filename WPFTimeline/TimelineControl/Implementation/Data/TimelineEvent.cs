using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimelineControl.Implementation.Data
{
    public class TimelineEvent : INotifyPropertyChanged, IComparer<TimelineEvent>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region fields

        #endregion

        #region properties

        string id;
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                RaisePropertyChanged("Id");
            }
        }

        string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                RaisePropertyChanged("Title");
            }
        }

        string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                RaisePropertyChanged("Description");
            }
        }

        DateTime startDate;
        public DateTime StartDate
        {
            get
            {
                return startDate;
            }
            set
            {
                startDate = value;
                RaisePropertyChanged("StartDate");
            }
        }

        DateTime endDate;
        public DateTime EndDate
        {
            get
            {
                return endDate;
            }
            set
            {
                endDate = value;
                RaisePropertyChanged("EndDate");
            }
        }

        string link;
        public string Link
        {
            get
            {
                return link;
            }
            set
            {
                link = value;
                RaisePropertyChanged("Link");
            }
        }

        bool isDuration;
        public bool IsDuration
        {
            get
            {
                return isDuration;
            }
            set
            {
                isDuration = value;
                RaisePropertyChanged("IsDuration");
            }
        }

        string eventImage;
        public string EventImage
        {
            get
            {
                return eventImage;
            }
            set
            {
                eventImage = value;
                RaisePropertyChanged("EventImage");
            }
        }

        string teaserImage;
        public string TeaserEventImage
        {
            get
            {
                return teaserImage;
            }
            set
            {
                teaserImage = value;
                RaisePropertyChanged("TeaserEventImage");
            }
        }

        string eventColor;
        public string EventColor
        {
            get
            {
                return eventColor;
            }
            set
            {
                eventColor = value;
                RaisePropertyChanged("EventColor");
            }
        }

        int rowOverride;
        public int RowOverride
        {
            get
            {
                return rowOverride;
            }

            set
            {
                rowOverride = value;
                RaisePropertyChanged("RowOverride");
            }
        }

        double widthOverride;
        public double WidthOverride
        {
            get
            {
                return widthOverride;
            }

            set
            {
                widthOverride = value;
                RaisePropertyChanged("WidthOverride");
            }
        }

        double heightOverride;
        public double HeightOverride
        {
            get
            {
                return heightOverride;
            }

            set
            {
                heightOverride = value;
                RaisePropertyChanged("HeightOverride");
            }
        }

        double topOverride;
        public double TopOverride
        {
            get
            {
                return topOverride;
            }

            set
            {
                topOverride = value;
                RaisePropertyChanged("TopOverride");
            }
        }

        object tag;
        public object Tag
        {
            get
            {
                return tag;
            }

            set
            {
                tag = value;
                RaisePropertyChanged("Tag");
            }
        }

        int row;
        public int Row
        {
            get
            {
                return row;
            }
            set
            {
                row = value;
                RaisePropertyChanged("Row");
            }
        }

        bool selected;
        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                RaisePropertyChanged("Selected");
            }
        }

        Dictionary<string, EventTag> eventTags;
        public Dictionary<string,EventTag> EventTags
        {
            get
            {
                if(eventTags == null)
                {
                    eventTags = new Dictionary<string, EventTag>();
                }
                return eventTags;
            }
        }

        #endregion

        public string this[string tagType]
        {
            get
            {
                if (eventTags.ContainsKey(tagType))
                {
                    return eventTags[tagType].Value;
                }
                return String.Empty;
            }
        }

        public void AddTag(string tagType, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                if (eventTags == null)
                {
                    eventTags = new Dictionary<string, EventTag>();
                }

                if (!eventTags.ContainsKey(tagType))
                {
                    eventTags.Add(tagType, new EventTag(tagType, value));
                }
                else
                {
                    if (!eventTags[tagType].Values.Contains(value))
                    {
                        eventTags[tagType].Values.Add(value);
                    }
                }
            }
        }

        public int Compare(TimelineEvent x, TimelineEvent y)
        {
            if (x.StartDate == y.EndDate)
                return 0;
            return x.StartDate > y.EndDate ? -1 : 1;
        }

        public bool InRange(DateTime from,DateTime to)
        {
            return !((StartDate < from && EndDate < from) || (StartDate > to && EndDate > to))
        }
    }
}
