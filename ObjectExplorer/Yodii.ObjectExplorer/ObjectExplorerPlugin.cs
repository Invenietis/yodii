using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Yodii.Model;
using Yodii.Wpf;

namespace Yodii.ObjectExplorer
{
    public class ObjectExplorerPlugin : WindowPluginBase
    {
        IYodiiEngineProxy _engine;

        public ObjectExplorerPlugin( IYodiiEngineProxy e )
            : base( e )
        {
            if( e == null ) { throw new ArgumentNullException( "e" ); }

            this.AutomaticallyDisableCloseButton = true;
            this.ShowClosingFailedMessageBox = true;
            this.StopPluginWhenWindowCloses = true;

            _engine = e;
        }

        protected override Window CreateWindow()
        {
            Window w = new Windows.ObjectExplorerWindow();

            ViewModels.ObjectExplorerViewModel vm = new ViewModels.ObjectExplorerViewModel();
            vm.LoadEngine(_engine);

            w.DataContext = vm;

            return w;
        }
    }
}
