using System;
using System.Windows;
using Yodii.Model;
using Yodii.DemoApp.Examples.Plugins.Views;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Yodii.DemoApp
{
    public class LaPoste : MonoWindowPlugin, ISecuredDeliveryService
    {
        readonly IMarketPlaceService _marketPlace;
        readonly ITimerService _timer;
        readonly IOutSourcingService _outsourcingService;
        ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>> _toBeDelivered;
        ObservableCollection<ToBeDeliveredSecurely> _toBeDeliveredSecurely;
        public ObservableCollection<ToBeDeliveredSecurely> SecuredDelivery { get { return _toBeDeliveredSecurely; } }
        public ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>> Delivery { get { return _toBeDelivered; } }

        const int _permanentEmployees=5;
        
        
        public class ToBeDeliveredSecurely
        {
            public ToBeDeliveredSecurely( IClientInfo clientInfo, MarketPlace.Product product)
            {
                ClientInfo = clientInfo;
                Product = product;
            }
            public  IClientInfo ClientInfo { get; private set; }
            public  MarketPlace.Product Product { get; private set; }
            public int NbBeforeReturned{get; set;}
        }

        public LaPoste( IMarketPlaceService market, ITimerService timer, IOutSourcingService outSourcingService )
            : base( true ) 
        {
            _marketPlace = market;
            _timer = timer;
            _outsourcingService = outSourcingService;
            _toBeDelivered = new ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>>();
            _toBeDeliveredSecurely = new ObservableCollection<ToBeDeliveredSecurely>();
            _timer.SubscribeToTimerEvent( TimeElapsed );
        }

        protected override Window CreateWindow()
        {
            Window = new LaPosteView()
            {
                DataContext = this
            };
            Window.Show();
            return Window;
        }

        void ISecuredDeliveryService.DeliverSecurely( Tuple<IClientInfo, MarketPlace.Product> order )
        {
            _toBeDeliveredSecurely.Add(new ToBeDeliveredSecurely(order.Item1, order.Item2));
        }

        void IDeliveryService.Deliver( Tuple<IClientInfo, MarketPlace.Product> order )
        {
            IConsumer client = _marketPlace.Consumers.Find( c => c.Info == order.Item1 );
            _toBeDelivered.Add( order );
        }
        void TimeElapsed( object sender, EventArgs e )
        {
            int tmpEmployees=0;
            while( _toBeDeliveredSecurely.Count > _permanentEmployees+tmpEmployees )
            {
                if( _outsourcingService.GetEmployees() )
                    tmpEmployees++;
                else
                    break;
            }

            int count=_toBeDeliveredSecurely.Count;
            int i=tmpEmployees+_permanentEmployees;
            int j=0;
            while(i>0 && j<count)
            {
                if( _marketPlace.Consumers.Find( c => c.Info == _toBeDeliveredSecurely[j].ClientInfo ) != null )
                {
                    if( _marketPlace.Consumers.Find( c => c.Info == _toBeDeliveredSecurely[j].ClientInfo ).ReceiveDelivery( _toBeDeliveredSecurely[j].Product ) ) 
                    {
                        _toBeDeliveredSecurely.Remove( _toBeDeliveredSecurely[j] );
                    }
                    else
                        _toBeDeliveredSecurely[j].NbBeforeReturned++;
                }
                else
                {
                    if( _toBeDeliveredSecurely[j].NbBeforeReturned > 5 )
                    { }//retour au fabricant?
                    else
                        _toBeDeliveredSecurely[j].NbBeforeReturned++;
                }
                i--;
                j++;
            }
            if( i > 0 )
            {
                foreach( Tuple<IClientInfo, MarketPlace.Product> order in _toBeDelivered )
                {
                    if( _marketPlace.Consumers.Find( c => c.Info == order.Item1 ) != null )
                    {
                        _marketPlace.Consumers.Find( c => c.Info == order.Item1 ).ReceiveDelivery( order.Item2 );
                    }
                    _toBeDelivered.Remove( order );
                }
            }
        }
    }
}
