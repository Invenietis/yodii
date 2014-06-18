using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Garage : MonoWindowPlugin, ICarRepairService
    {
        public Garage()
            : base( true )
        {
        }

        public void Repair()
        {

        }

        protected override Window CreateWindow()
        {
            Window = new GarageView()
            {
                DataContext = this
            };

            return Window;
        }
    }
}
