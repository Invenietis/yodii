using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class UPS : MonoWindowPlugin, ISecuredDeliveryService
    {
        public UPS()
            : base( true )
        {
        }

        protected override Window CreateWindow()
        {
            Window = new UPSView()
            {
                DataContext = this
            };

            return Window;
        }

        void ISecuredDeliveryService.DeliverSecurely(IProductInfo product, IClientInfo client)
        {
        }

        void IDeliveryService.Deliver(IProductInfo product, IClientInfo client)
        {
        }
    }
}
