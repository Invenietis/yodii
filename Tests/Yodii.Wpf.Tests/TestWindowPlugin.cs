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
        public TestWindowPlugin( IYodiiEngineBase injectedEngine )
            : base( injectedEngine )
        {
            Assert.That( injectedEngine, Is.Not.Null );

            // Defaults check
            Assert.That( this.StopPluginWhenWindowCloses, Is.False );
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

            // Can't assert that: Application.Current is never reset
            //Assert.That( Application.Current, Is.Null );
        }

        protected override Window CreateWindow()
        {
            return new Window();
        }
    }
}
