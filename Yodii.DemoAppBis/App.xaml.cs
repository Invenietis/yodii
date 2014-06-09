using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void AppStartup( object sender, StartupEventArgs e )
        {
            ITimerService timer = new Timer();
            IMarketPlaceService market = new MarketPlace( timer );
            ICarRepairService carRepair = new Garage( timer );
            IOutSourcingService outsourcing = new ManPower( timer );
            IDeliveryService delivery = new LivrExpress( carRepair, outsourcing, timer );

            Client1 client = new Client1( market, timer );
            Company1 company = new Company1( market, delivery, timer );
            ( (IYodiiPlugin)client ).Start();
            ( (IYodiiPlugin)company ).Start();
        }
    }
}
