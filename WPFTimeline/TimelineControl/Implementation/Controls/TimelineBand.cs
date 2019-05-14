using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TimelineControl.Implementation.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace TimelineControl.Implementation.Controls
{
    public delegate void TimelineEventDelegate(FrameworkElement element, TimelineDisplayEvent de);
    public delegate void DoubleClick(DateTime date, Point point);
    public delegate void ScrollViewpointChanged(object sender,double value);
    public delegate void TimelineStatusDelegate(FrameworkElement element, TimelineStatusItemDisplay de);
    public class TimelineBand : Control
    {
        #region Constants
        // Used for tracing in debug version
        private const string FMT_TRACE = "Name: {0}, Size:{1},{2}";
        // Default teaser length of events in main band
        private const int DEFAULT_TEASER_SIZE = 80;

        // Default description width    
        //默认的description宽度
        private const double DEFAULT_DESCRIPTION_WIDTH = 226;

        #endregion

        public event EventHandler TimelineReady;
        public event EventHandler TimelineUpdated;
        public event EventHandler CurrentDateTimeChanged;
        public event EventHandler SelectionChanged;
        public event EventHandler EventsRefreshed;
        public event DoubleClick TimelineDoubleClick;
        public event ScrollViewpointChanged ScrollViewChanged;

        public event TimelineEventDelegate OnEventCreated;
        public event TimelineEventDelegate OnEventDeleted;
        public event TimelineEventDelegate OnEventVisible;
        /// <summary>
        /// 状态栏创建事件
        /// </summary>
        public event TimelineStatusDelegate OnStatusCreated;

        #region fields

        private TimelineEventStore _eventStore;
        private bool _changingDate;
        private bool _initialized;
        private bool _loaded;
        private ObservableCollection<TimelineEvent> _selection = new ObservableCollection<TimelineEvent>();

        #endregion

        #region properties

        double _offset = 0;
        public double EventCanvasOffset
        {
            get
            {
                return _offset;
            }

            set
            {
                this.CanvasScrollOffset = value;
                _offset = value;
            }
        }

        //调整大小后重新计算timeline event的positions
        bool _recalcOnResize = true;
        /// <summary>
        /// Specifies if vertical event positions should be recalculated when 
        /// timeline is resized. If false event positions calculated only once when
        /// timeline tray is displayed
        /// 当timeline调整大小后event的positions是否重新计算，如果是否，当timelinetray显示后event positions只计算一次
        /// </summary>
        public bool RecalculateEventTopPosition
        {
            get
            {
                return _recalcOnResize;
            }
            set
            {
                _recalcOnResize = value;
            }
        }

        /// <summary>
        /// Return list of loaded timeline events
        /// 加载的timeline events集合
        /// </summary>
        public List<TimelineEvent> TimelineEvents
        {
            get => _eventStore.Events;
        }

        private double _descriptionWidth = DEFAULT_DESCRIPTION_WIDTH;
        /// <summary>
        /// Default width of of description element of main timeline band
        /// </summary>
        public double DescriptionWidth
        {
            get
            {
                return _descriptionWidth;
            }
            set
            {
                _descriptionWidth = value;
            }
        }

        public bool IsTimelineInitialized { get => _initialized; }

        public ObservableCollection<TimelineEvent> SelectedTimelineEvents { get => _selection; }


        #endregion

        #region Ctor

        public TimelineBand()
        {
            _eventStore = new TimelineEventStore(new List<TimelineEvent>());
            this.Loaded += OnControlLoaded;
            this.SizeChanged += OnSizeChanged;
            this.MouseWheel += OnMouseWheel;
        }

        #endregion

        #region Dependency Properties

        #region MinDateTime

        public static readonly DependencyProperty MinDateTimeProperty =
            DependencyProperty.Register("MinDateTime", typeof(DateTime),
            typeof(TimelineBand), new PropertyMetadata(DateTime.MinValue,
                OnMinDateTimeChanged));

        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime MinDateTime
        {
            get
            {
                return (DateTime)GetValue(MinDateTimeProperty);
            }
            set
            {
                SetValue(MinDateTimeProperty, value);
            }
        }

        public static void OnMinDateTimeChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            TimelineBand t;

            t = d as TimelineBand;

            if (t != null && e.NewValue != e.OldValue)
            {
                if ((DateTime)e.NewValue >= t.MaxDateTime)
                {
                    throw new ArgumentOutOfRangeException("MinDateTime cannot be more then MaxDateTime");
                }
                else if (t.CurrentDateTime < (DateTime)e.NewValue)
                {
                    t.SetValue(CurrentDateTimeProperty, e.NewValue);
                }

                //TODO
                if(this.Caculator != null && this.Calculator.Calendar != null)
                {
                    this.Calculator.Calendar.MinDateTime = ((DateTime)e.NewValue);
                    this.Calculator.BuildCurrentTimeTag();
                    this.Calculator.CreateTimerTickEvent();
                }
            }
        }


        #endregion

        #region MaxDateTime

        public static readonly DependencyProperty MaxDateTimeProperty =
            DependencyProperty.Register("MaxDateTime", typeof(DateTime),
            typeof(TimelineBand), new PropertyMetadata(DateTime.MaxValue, OnMaxDateTimeChanged));

        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime MaxDateTime
        {
            get
            {
                return (DateTime)GetValue(MaxDateTimeProperty);
            }
            set
            {
                SetValue(MaxDateTimeProperty, value);
            }
        }

        public static void OnMaxDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineBand t;

            t = d as TimelineBand;

            if (t != null && e.NewValue != e.OldValue)
            {
                if ((DateTime)e.NewValue < t.MinDateTime)
                {
                    throw new ArgumentOutOfRangeException("MaxDateTime cannot be less then MinDateTime");
                }
                else if (t.CurrentDateTime >= (DateTime)e.NewValue)
                {
                    t.SetValue(CurrentDateTimeProperty, e.NewValue);
                }
                if (this.Calculator != null && this.Calculator.Calendar != null)
                {
                    this.Calculator.Calendar.MaxDateTime = ((DateTime)e.NewValue);
                    this.Calculator.BuildCurrentTimeTag();
                    this.Calculator.CreateTimerTickEvent();
                }
                
            }
        }

        #endregion

        #region CurrentDateTime
        public static readonly DependencyProperty CurrentDateTimeProperty =
            DependencyProperty.Register("CurrentDateTime", typeof(DateTime),
            typeof(TimelineBand), new PropertyMetadata(DateTime.Now, OnCurrentDateTimeChanged));

        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime CurrentDateTime
        {
            get
            {
                return (DateTime)GetValue(CurrentDateTimeProperty);
            }
            set
            {
                SetValue(CurrentDateTimeProperty, value);
            }
        }

        public static void OnCurrentDateTimeChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            TimelineBand t;

            t = d as TimelineBand;

            if (t != null && e.NewValue != e.OldValue)
            {
                if ((DateTime)e.NewValue < t.MinDateTime)
                {
                    t.SetValue(CurrentDateTimeProperty, t.MinDateTime);
                }
                else if ((DateTime)e.NewValue > t.MaxDateTime)
                {
                    t.SetValue(CurrentDateTimeProperty, t.MaxDateTime);
                }
                else
                {
                    t.m_currentDateTime = (DateTime)e.NewValue;
                    if (t.m_mainBand != null)
                    {
                        t.m_mainBand.CurrentDateTime = t.m_currentDateTime;
                        t.m_currentDateTime = t.m_mainBand.CurrentDateTime;
                        t.m_mainBand.Calculator.ChangeData(t.m_currentDateTime);
                    }

                }
            }
        }
        #endregion

        #region TeaserSize

        public static readonly DependencyProperty TeaserSizeProperty =
            DependencyProperty.Register("TeaserSize", typeof(int),
            typeof(TimelineBand), new PropertyMetadata(DEFAULT_TEASER_SIZE));

        public int TeaserSize
        {
            get
            {
                return (int)GetValue(TeaserSizeProperty);
            }
            set
            {
                SetValue(TeaserSizeProperty, value);
            }
        }

        #endregion

        #region ImmediateDisplay

        public static readonly DependencyProperty ImmediateDisplayProperty =
            DependencyProperty.Register("ImmediateDisplay", typeof(bool),
            typeof(TimelineBand), new PropertyMetadata(true));

        public bool ImmediateDisplay
        {
            get
            {
                return (bool)GetValue(ImmediateDisplayProperty);
            }
            set
            {
                SetValue(ImmediateDisplayProperty, value);
            }
        }

        #endregion

        #region EventItemSource

        public static readonly DependencyProperty EventItemSourceProperty =
            DependencyProperty.Register("EventItemSource", typeof(ObservableCollection<TimelineEvent>),
            typeof(TimelineBand), new PropertyMetadata(null, OnEventItemSourceChanged));

        public ObservableCollection<TimelineEvent> EventItemSource
        {
            get
            {
                return (ObservableCollection<TimelineEvent>)GetValue(EventItemSourceProperty);
            }
            set
            {
                SetValue(EventItemSourceProperty, value);
            }
        }

        public static void OnEventItemSourceChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            TimelineBand t;

            t = d as TimelineBand;

            if (t != null && e.NewValue != e.OldValue)
            {
                t.TimelineEvents.Clear();
                if (e.NewValue != null)
                {
                    foreach (var item in e.NewValue as ObservableCollection<TimelineEvent>)
                    {
                        t.TimelineEvents.Add(item);
                    }
                }
                t.ResetEvents(t.TimelineEvents);
                if (t.EventItemSource != null)
                {
                    t.EventItemSource.CollectionChanged += EventItemSource_CollectionChanged;
                    foreach (var item in t.EventItemSource)
                    {
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                }
            }
        }

        private static void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO
            //TimelineEvent的StartDate字段变更
            if (e.PropertyName == "StartDate")
            {
                //获取到TimelineTray对象获取不到

                Debug.WriteLine("EventItemSource StartDate propertyChanged");
            }

            //TODO
            //TimelineEvent的EndDate字段变更
            if (e.PropertyName == "EndDate")
            {

                Debug.WriteLine("EventItemSource EndDate propertyChanged");
            }
        }

        private static void EventItemSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //TODO
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {

            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {


            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {

            }
        }
        #endregion

        #endregion

        #region function

        public ResetEvents(List<TimelineEvent> events,bool fixDates = true)
        {
            if(fixDates)
            {
                foreach(var e in events)
                {
                    if(!e.IsDuration)
                    {
                        e.EndDate = e.StartDate;
                    }
                }
            }
            _eventStore = new TimelineEventStore(events);
            ClearSelection();
            RefreshEvents();
        }

        private void ClearSelection()
        {
            if(this.SelectionChanged != null)
            {
                this.SelectedTimelineEvents.Clear();
                _selection.Clear();
            }
        }

        public void ClearEvents()
        {
            _eventStore = new TimelineEventStore(new List<TimelineEvent>());
            RefreshEvents();
        }

        public void RefreshEvents()
        {
            this.RefreshEvents(true);
        }

        /// <summary>
        /// 刷新所有的events
        /// </summary>
        /// <param name="checkInit">是否是初始化</param>
        public void RefreshEvents(bool checkInit)
        {
            if (_initialized || !checkInit)
            {
                Debug.Assert(this.Calculator != null);

                this.ClearEvents();
                this.EventStore = _eventStore;

                this.CalculateEventRows();

                this.CalculateEventPositions();

                this.DisplayEvents();

                if (TimelineUpdated != null)
                {
                    TimelineUpdated(this, EventArgs.Empty);
                }
            }

            if (EventsRefreshed != null)
            {
                EventsRefreshed(this, EventArgs.Empty);
            }
        }

        public override string ToString()
        {
            return String.Format(FMT_TRACE,Name,ActualHeight,ActualWidth);
        }

        private void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            //TODO
            //throw new NotImplementedException();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(_initialized)
            {
                RefreshEvents();
            }
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
