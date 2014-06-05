﻿using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company2 : MonoWindowPlugin
    {
        IService<IMarketPlaceService> _serviceRef1;
        IService<IDeliveryService> _serviceRef2;
        ITimerService _timer;

        public Company2( IRunningService<IMarketPlaceService> ServiceRef1, IRunningService<IDeliveryService> ServiceRef2, ITimerService timer, bool runningLifetimeWindow )
            : base( runningLifetimeWindow )
        {
            _serviceRef1 = ServiceRef1;
            _serviceRef2 = ServiceRef2;
            _timer = timer;
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new Company2View()
            {
                DataContext = this
            };
            Window.Show();
            return Window;
        }

        protected override void DestroyWindow()
        {
            if( Window != null ) Window.Close();
        }
    }
}
