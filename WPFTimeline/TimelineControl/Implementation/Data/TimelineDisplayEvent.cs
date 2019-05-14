using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TimelineControl.Implementation.Controls;

namespace TimelineControl.Implementation.Data
{
    public class TimelineDisplayEvent : ContentControl,INotifyPropertyChanged
    {
        private const string EVENT_LINE_RESOURCE_NAME = "TimelineEventLine";
        public static int TeaserSize;
        private TimelineEvent _timelineEvent;
        private bool _selected;


        public TimelineDisplayEvent(TimelineEvent e,TimelineBand band,TimelineBuilder builder)
        {
            _timelineEvent = e;
            if(_timelineEvent != null)
            {
                _timelineEvent.PropertyChanged += OnEventPropertyChanged;
            }
            _selected = e.Selected;
            TimelineBuilder = builder;
        }

        private void OnEventPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Length == 0)
            {
                RaisePropertyChanged(String.Empty);
            }
            else if (e.PropertyName == "EventColor")
            {
                RaisePropertyChanged("EventColorBrush");
            }
            else if (e.PropertyName == "Description")
            {
                UpdateDisplayEvent();
            }
            else if (e.PropertyName == "StartDate" || e.PropertyName == "EndDate")
            {
                RaisePropertyChanged("EventTime");
            }
            else if (e.PropertyName == "Link")
            {
                UpdateDisplayEvent();
            }
        }

        public TimelineEvent Event { get => _timelineEvent; }

        public TimelineBuilder TimelineBuilder { get; set; }

        double _eventDescriptionWidth;
        /// <summary>
        /// Default width of description for main timeline band events
        /// </summary>
        public double DescriptionWidth
        {
            get
            {
                return _eventDescriptionWidth;
            }

            set
            {
                _eventDescriptionWidth = value;
                _timelineEvent.WidthOverride = value;
                RaisePropertyChanged("DescriptionWidth");
            }
        }

        double _eventDescriptionHeight;
        /// <summary>
        /// Default height of description for main timeline band events
        /// </summary>
        public double DescriptionHeight
        {
            get
            {
                return _eventDescriptionHeight;
            }

            set
            {
                _eventDescriptionHeight = value;
                _timelineEvent.HeightOverride = value;
                RaisePropertyChanged("DescriptionHeight");
            }
        }

        double _eventPixWidth;
        public double EventPixelWidth
        {
            get
            {
                return _eventPixWidth;
            }
            set
            {
                _eventPixWidth = value;

                ActualEventPixelWidth = TimelineBuilder.TimeSpanToPixels(Event.EndDate - Event.StartDate);
                ActualEventPixelWidth = Math.Max(3.0, ActualEventPixelWidth);

                RaisePropertyChanged("EventPixelWidth");
                RaisePropertyChanged("ActualEventPixelWidth");
            }
        }

        public double ActualEventPixelWidth { get; set; }

        public Brush EventColorBrush
        {
            get
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }

        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                _timelineEvent.Selected = _selected;
                RaisePropertyChanged(String.Empty);
            }
        }

        public Thickness SelectionBorder
        {
            get
            {
                return Selected ? new Thickness(3) : new Thickness(1);
            }
        }

        string m_teaser;
        public string Teaser
        {
            get
            {
                return m_teaser;
            }
            set
            {
                m_teaser = value;
                RaisePropertyChanged("Teaser");
            }
        }

        public string EventTime
        {
            get
            {
                string res;

                if (Event.IsDuration)
                {
                    if (Event.StartDate.ToShortDateString() == Event.EndDate.ToShortDateString())
                    {
                        res = Event.StartDate.ToShortDateString() + " " +
                              Event.StartDate.ToShortTimeString() + ".." +
                              Event.EndDate.ToShortTimeString();
                    }
                    else
                    {
                        res = Event.StartDate.ToShortDateString() + " " +
                              Event.StartDate.ToShortTimeString() + ".." +
                              Event.EndDate.ToShortDateString() + " " +
                              Event.EndDate.ToShortTimeString();

                    }

                }
                else
                {
                    res = Event.StartDate.ToShortDateString() + " " +
                              Event.StartDate.ToShortTimeString();
                }

                return res;
            }
        }


        private void UpdateDisplayEvent()
        {
            if (TeaserSize > 0 && Event.Description.Length > TeaserSize)
            {
                Teaser = Event.Description.Substring(0, TeaserSize) + "...";
            }
            else
            {
                Teaser = Event.Description;
            }
        }


        public void Recalculate(bool fireUpdate = true)
        {
            ActualEventPixelWidth = TimelineBuilder.TimeSpanToPixels(Event.EndDate - Event.StartDate);
            ActualEventPixelWidth = Math.Max(3.0, ActualEventPixelWidth);
            EventPixelWidth = Math.Min(TimelineBuilder.PixelWidth * 2, ActualEventPixelWidth);

            RaisePropertyChanged("DescriptionWidth");
            RaisePropertyChanged("DescriptionHeight");
            RaisePropertyChanged("EventPixelWidth");
            RaisePropertyChanged("ActualEventPixelWidth");
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(name));
        }
    }
}
