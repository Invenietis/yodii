using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class ManPower : MonoWindowPlugin, IOutSourcingService
    {
        public ManPower()
            : base( true )
        {
        }

        protected override Window CreateWindow()
        {
            Window = new ManPowerView()
            {
                DataContext = this
            };
            return Window;
        }

        public void GetEmployees()
        {

        }
    }
}
