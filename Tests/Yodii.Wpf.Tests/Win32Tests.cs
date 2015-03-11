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
    [TestFixture]
    public class Win32ExtensionTests
    {
        [Test]
        public void Window_SysMenu_IsEnabled_ByDefault()
        {
            Assert.That( Application.Current, Is.Not.Null );
            Application.Current.Dispatcher.Invoke( () =>
            {
                Window w = new Window();
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
                Window w = new Window();
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
                Window w = new Window();
                w.Show();

                Assert.That( w.IsSysMenuEnabled(), Is.True );
                w.HideSysMenu();
                Assert.That( w.IsSysMenuEnabled(), Is.False );
                w.ShowSysMenu();
                Assert.That( w.IsSysMenuEnabled(), Is.True );

                w.CloseAndWaitForClosed();
            } );
        }

        [Test]
        public void Window_Close_MenuItem_CanBeDisabled()
        {
            Assert.That( Application.Current, Is.Not.Null );
            Application.Current.Dispatcher.Invoke( () =>
            {
                Window w = new Window();
                w.Show();

                Assert.That( w.IsCloseButtonDisabled(), Is.False );
                w.DisableCloseButton();
                Assert.That( w.IsCloseButtonDisabled(), Is.True );
                w.EnableCloseButton();
                Assert.That( w.IsCloseButtonDisabled(), Is.False );

                w.CloseAndWaitForClosed();
            } );
        }

    }
}
