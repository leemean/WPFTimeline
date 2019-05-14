using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using TimelineControl.Implementation.Controls;

namespace TimelineControl.Implementation.Data
{
    /// <summary>
    /// This class provides mapping between screen coordinates/sizes and time and 
    /// timeline event placement
    /// 此类提供屏幕坐标/大小和时间之间的映射，以及timeline event构建和放置
    /// </summary>
    public class TimelineBuilder
    {
        // max number of events that we display on the screen at the same time
        // (just need some reasonable limit to avoid perf issues)
        // 屏幕中可放下最大的event数量
        public const int MAX_EVENTS = 100;
        // colums which are not visible, but used for smothing of scroll
        private const int EXTRA_COLUMNS = 0;

        private TimelineBand m_parent;
        private DateTime m_currDate;

        private FrameworkElement[] m_columns;
        private FrameworkElement[] m_columnMarkers;
        private int m_columnCount;

        private Canvas m_canvas;
        private Canvas m_statusCanvas;

        private TimelineEventStore m_events;
        private double m_maxEventHeight;
        private Dictionary<TimelineEvent, TimelineDisplayEvent>
                                                m_dispEvents;

        private DataTemplate m_template;
        private DataTemplate m_markerTemplate;
        private DataTemplate m_eventTemplate;

        private TimeSpan m_maxDescriptionWidth = new TimeSpan();
        private Stack<UIElement> m_elementPool = new Stack<UIElement>();

        private long m_displayExecTime = 0;
        private double m_viewSize;

        private DataTemplate m_currentTimeTagTemplate;
        private FrameworkElement m_currentTimeTag;
        private CurrentTimeTagItem m_currentTimeTagItem;

        public TimelineBuilder(
            TimelineBand band,
            Canvas canvas,
            DataTemplate template,
            DataTemplate textTemplate,
            int columnCount,
            TimelineCalendar timeline,
            DataTemplate eventTemplate,
            double maxEventHeight,
            bool assignRows,
            DateTime currDateTime,
            DataTemplate currentTimeTagTemplate
        )
        {
            m_parent = band;
            m_eventTemplate = eventTemplate;
            m_canvas = canvas;
            m_template = template; ;
            m_columnCount = columnCount;
            //m_timeline = timeline;
            m_markerTemplate = textTemplate;

            m_dispEvents = new Dictionary<TimelineEvent, TimelineDisplayEvent>();
            m_maxEventHeight = maxEventHeight;
            m_currentTimeTagTemplate = currentTimeTagTemplate;
            CurrentDateTime = currDateTime;
        }

        #region Properties

        public double PixelWidth
        {
            get
            {
                return m_canvas.ActualWidth;
            }
        }

        public double PixelHeight
        {
            get
            {
                return m_canvas.ActualHeight;
            }
        }

        public double ColumnPixelWidth
        {
            get
            {
                if (m_columns == null)
                {
                    return 0;
                }
                return PixelWidth / m_columnCount;
            }
        }

        #endregion


        /// <summary>
        /// Build columns (one for each years, dates, etc.)
        /// </summary>
        public void BuildColumns(
            bool animate = false,
            bool displayEvents = false,
            Size? newSize = null
        )
        {
            double step;
            double width;
            double height;
            double left;

            FrameworkElement[] columns;
            FrameworkElement[] markers;
            int i;

            Debug.Print("Build Columns");

            //旧尺寸
            if (newSize == null)
            {
                width = PixelWidth;
                height = PixelHeight;
            }//新尺寸
            else
            {
                width = newSize.Value.Width;
                height = newSize.Value.Height;
            }

            //每列宽度
            step = ColumnPixelWidth;

            if (m_columns == null || m_columns.Length != m_columnCount + EXTRA_COLUMNS)
            {
                columns = new FrameworkElement[m_columnCount + EXTRA_COLUMNS];
                markers = new FrameworkElement[m_columnCount + EXTRA_COLUMNS];

                if (m_columns != null)
                {
                    Array.Copy(m_columns, columns, Math.Min(columns.Length, m_columns.Length));

                    for (i = columns.Length; i < m_columns.Length; ++i)
                    {
                        m_canvas.Children.Remove(m_columns[i]);
                    }
                }

                if (m_columnMarkers != null)
                {
                    Array.Copy(m_columnMarkers, markers, Math.Min(markers.Length, m_columnMarkers.Length));

                    for (i = markers.Length; i < m_columnMarkers.Length; ++i)
                    {
                        m_canvas.Children.Remove(m_columnMarkers[i]);
                    }
                }
                m_columns = columns;
                m_columnMarkers = markers;
            }

            for (i = 0; i < m_columnCount + EXTRA_COLUMNS; ++i)
            {
                left = ColumnPixelWidth * (i - 1);

                if (m_columns[i] == null)
                {
                    m_columns[i] = m_template.LoadContent() as FrameworkElement;
                    m_columns[i].DataContext = null;
                    m_canvas.Children.Add(m_columns[i]);
                }

                if (m_markerTemplate != null && m_columnMarkers[i] == null)
                {
                    m_columnMarkers[i] = m_markerTemplate.LoadContent() as FrameworkElement;
                    m_columnMarkers[i].DataContext = null;
                    m_canvas.Children.Add(m_columnMarkers[i]);
                }
            }

            FixPositions(displayEvents, animate, true);
        }
    }
}
