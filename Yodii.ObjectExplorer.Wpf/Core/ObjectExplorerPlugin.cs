using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Wpf
{
    public class ObjectExplorerPlugin : IYodiiPlugin
    {
        IYodiiEngine _activeEngine;

        public ObjectExplorerPlugin( IYodiiEngine e )
        {
            Debug.Assert( e != null );

            _activeEngine = e;
        }

        #region IYodiiPlugin Members

        bool IYodiiPlugin.Setup( PluginSetupInfo info )
        {
            return true;
        }

        void IYodiiPlugin.Start()
        {
            MainWindow w = new MainWindow();
            w.Show();
        }

        void IYodiiPlugin.Stop()
        {
        }

        void IYodiiPlugin.Teardown()
        {
        }

        #endregion
    }
}
