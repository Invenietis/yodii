using System;
using System.Windows;
using Yodii.Model;
using Yodii.DemoApp.Examples.Plugins.Views;

namespace Yodii.DemoApp
{
    public class LaPoste : MonoWindowPlugin, ISecuredDeliveryService
    {
        public LaPoste()
            : base( true ) 
        {
        }

        protected override Window CreateWindow()
        {
            Window = new LaPosteView()
            {
                DataContext = this
            };
            return Window;
        }

        void ISecuredDeliveryService.DeliverSecurely( IProductInfo product, IClientInfo client )
        {
        }

        void IDeliveryService.Deliver( IProductInfo product, IClientInfo client )
        {
        }
    }
}
