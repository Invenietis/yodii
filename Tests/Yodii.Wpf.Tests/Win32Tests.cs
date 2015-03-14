using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NUnit.Framework;
using Yodii.Wpf.Win32;

namespace Yodii.Wpf.Tests
{
    [TestFixture, Explicit]
    public class Win32ExtensionTests
    {
        [Test]
        public void Window_SysMenu_IsEnabled_ByDefault()
        {
            Assert.That( Application.Current, Is.Not.Null );
            Application.Current.Dispatcher.Invoke( () =>
            {
                TestPluginWindow w = new TestPluginWindow();
                w.Show();

                Assert.That( w.IsSysMenuEnabled(), Is.True );
                w.CloseAndWaitForClosed();
            } );
        }

        [Test]
        public void Window_CloseMenuItem_IsEnabled_ByDefault()
        {
            Assert.That( Application.Current, Is.Not.Null );
            Application.Current.Dispatcher.Invoke( () =>
            {
                TestPluginWindow w = new TestPluginWindow();
                w.Show();

                Assert.That( w.IsCloseButtonDisabled(), Is.False );

                w.CloseAndWaitForClosed();
            } );
        }

        [Test]
        public void Window_SysMenu_CanBeHidden()
        {
            Assert.That( Application.Current, Is.Not.Null );
            Application.Current.Dispatcher.Invoke( () =>
            {
                TestPluginWindow w = new TestPluginWindow();
                w.Show();

                Assert.That( w.IsSysMenuEnabled(), Is.True, "Window sysmenu must be enabled by default" );
                w.HideSysMenu();
                Assert.That( w.IsSysMenuEnabled(), Is.False, "Window sysmenu must be disabled after calling HideSysMenu()" );
                w.ShowSysMenu();
                Assert.That( w.IsSysMenuEnabled(), Is.True, "Window sysmenu must be enabled after calling ShowSysMenu()" );

                w.CloseAndWaitForClosed();
            } );
        }

        [Test]
        public void Window_Close_MenuItem_CanBeDisabled()
        {
            Assert.That( Application.Current, Is.Not.Null );
            Application.Current.Dispatcher.Invoke( () =>
            {
                TestPluginWindow w = new TestPluginWindow();
                w.Show();

                Assert.That( w.IsCloseButtonDisabled(), Is.False, "Window close button must be enabled by default" );
                w.DisableCloseButton();
                Assert.That( w.IsCloseButtonDisabled(), Is.True, "Window close button must be disabled after DisableCloseButton() is called" );
                w.EnableCloseButton();
                Assert.That( w.IsCloseButtonDisabled(), Is.False, "Window close button must be enabled after EnableCloseButton() is called" );

                w.CloseAndWaitForClosed();
            } );
        }

    }
}
