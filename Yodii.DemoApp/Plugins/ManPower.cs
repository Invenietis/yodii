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
            _nbEmployees = _maxAvailable;
        }
        const int _maxAvailable=20;
        int _nbEmployees;
        public int NBemployees { get { return _nbEmployees; } set { _nbEmployees = value; RaisePropertyChanged(); } }
        protected override Window CreateWindow()
        {
            Window = new ManPowerView()
            {
                DataContext = this
            };
            return Window;
        }

        public bool GetEmployees()
        {
            NBemployees--;
            return true;
        }
        public void  ReturnEmployees(int nbReturned)
        {
            NBemployees += nbReturned;
        }
    }
}
