using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimelineControl.Implementation.Data
{
    /// <summary>
    /// timeline 当前时间刻度线
    /// </summary>
    public class CurrentTimeTagItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        double m_height;

        public double Height
        {
            get
            {
                return m_height;
            }
            set
            {
                m_height = value;
                RaisePropertyChanged("Height");
            }
        }

        DateTime m_startTime;
        public DateTime StartTime
        {
            get
            {
                return m_startTime;
            }
            set
            {
                m_startTime = value;
                RaisePropertyChanged("StartTime");
            }
        }

        DateTime m_endTime;
        public DateTime EndTime
        {
            get
            {
                return m_endTime;
            }
            set
            {
                m_endTime = value;
                RaisePropertyChanged("EndTime");
            }
        }
    }
}
