using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Garage : MonoWindowPlugin, ICarRepairService
    {
        public Garage( IYodiiEngine engine )
            : base( true, engine )
        {
        }

        public bool Repair()
        {
            return true;
        }

        protected override Window CreateWindow()
        {
            Window = new GarageView( this )
            {
                DataContext = this
            };

            return Window;
        }
    }
}
