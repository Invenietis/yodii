using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Wpf.Tests
{
    public class TestWindowPlugin : WindowPluginBase
    {
        public static bool StopPluginWhenWindowClosesConfig { get; set; }
        public static bool ShowClosingFailedMessageBoxConfig { get; set; }
        public static bool AutomaticallyDisableCloseButtonConfig { get; set; }

        public TestWindowPlugin( IYodiiEngine injectedEngine )
            : base( injectedEngine )
        {
            Assert.That( injectedEngine, Is.Not.Null );

            this.StopPluginWhenWindowCloses = StopPluginWhenWindowClosesConfig;
            this.ShowClosingFailedMessageBox = ShowClosingFailedMessageBoxConfig;
            this.AutomaticallyDisableCloseButton = AutomaticallyDisableCloseButtonConfig;
        }

        protected override Window CreateWindow()
        {
            return new TestPluginWindow();
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            base.PluginPreStart( c );
        }

        protected override void PluginStart( IStartContext c )
        {
            base.PluginStart( c );
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            base.PluginPreStop( c );
        }
        protected override void PluginStop( IStopContext c )
        {
            base.PluginStop( c );
        }
    }

    public class TestPluginWindow : Window
    {
    }
}
