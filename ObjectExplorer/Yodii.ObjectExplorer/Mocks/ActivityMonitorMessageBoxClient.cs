#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CK.Core;

namespace Yodii.ObjectExplorer.Mocks
{
    class ActivityMonitorMessageBoxClient : CK.Core.ActivityMonitorTextWriterClient
    {
        public ActivityMonitorMessageBoxClient()
            : base( s => ShowMessage( s ), new LogFilter( LogLevelFilter.Error, LogLevelFilter.Warn ) )
        {

        }

        private static void ShowMessage( string s )
        {
            MessageBox.Show( s );
        }
    }
}
#endif