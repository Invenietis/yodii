using System;
using System.Collections.Generic;
using System.Linq;
using PropertyChanged;
using Yodii.Model;
using Yodii.ObjectExplorer.ViewModels;

namespace Yodii.ObjectExplorer.Mocks
{
    [ImplementPropertyChanged]
    public class MockObjectExplorerViewModel : ObjectExplorerViewModel
    {
#if DEBUG
        System.Timers.Timer _timer;
        object _timerLock = new object();


        public IYodiiEngineHost Host { get; private set; }

        public MockObjectExplorerViewModel()
            : base()
        {
            MockEngineHelper.StartDesignTimeYodiiEngine();

            // Start a timer to auto-switch SelectedItem
            _timer = new System.Timers.Timer( 2000 );
            _timer.Elapsed += timer_Elapsed;
            _timer.Start();

            this.LoadEngine( new MockYodiiEngineProxy( MockEngineHelper.Engine ) );
        }

        void timer_Elapsed( object sender, System.Timers.ElapsedEventArgs e )
        {
            lock( _timerLock )
            {
                if( System.Windows.Application.Current != null )
                {
                    System.Windows.Application.Current.Dispatcher.Invoke( UpdateTimerFromDispatcher );
                }
            }
        }

        private void UpdateTimerFromDispatcher()
        {
            if( this.EngineViewModel.SelectedItem == null )
            {
                this.EngineViewModel.SelectedItem = PickRandom( EngineViewModel.Services );
            }
            else if( this.EngineViewModel.SelectedItem is ServiceViewModel )
            {
                this.EngineViewModel.SelectedItem = PickRandom( EngineViewModel.Plugins );
            }
            else if( this.EngineViewModel.SelectedItem is PluginViewModel )
            {
                this.EngineViewModel.SelectedItem = null;
            }
        }

        static Random r = new Random();
        private static T PickRandom<T>( IEnumerable<T> collection )
        {
            if( collection == null ) return default( T );

            var a = collection.ToArray();

            if( a.Length == 0 ) return default( T );

            return a[r.Next( 0, a.Length )];
        }
#endif
    }

}
