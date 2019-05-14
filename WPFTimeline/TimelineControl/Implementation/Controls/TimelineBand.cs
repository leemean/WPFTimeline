using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TimelineControl.Implementation.Data;

namespace TimelineControl.Implementation.Controls
{
    public class TimelineBand : Control
    {
        #region Constants

        // Default teaser length of events in main band
        private const int DEFAULT_TEASER_SIZE = 80;

        // Default description width    
        //默认的description宽度
        private const double DEFAULT_DESCRIPTION_WIDTH = 226;

        #endregion

        #region fields

        private TimelineEventStore _eventStore;

        private bool _changingDate;

        #endregion

        #region properties



        #endregion


    }
}
