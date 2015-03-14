using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Yodii.Model;
using Yodii.Wpf;

namespace Yodii.ConfigurationManager
{
    public class ConfigurationManagerPlugin : WindowPluginBase, IConfigurationManagerService
    {
        public ConfigurationManagerPlugin( IYodiiEngineProxy e ) : base(e)
        {
            this.AutomaticallyDisableCloseButton = true;
            this.StopPluginWhenWindowCloses = true;
            this.ShowClosingFailedMessageBox = true;

        }

        protected override Window CreateWindow()
        {
            return new Window();
        }

        #region IConfigurationManagerService Members

        public void ActivateWindow()
        {
            Window.Activate();
        }

        #endregion
    }
}
