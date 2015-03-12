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
        public ObjectExplorerPlugin(IYodiiEngineBase e)
            : base( e )
        {

        }

        protected override Window CreateWindow()
        {
            throw new NotImplementedException();
        }
    }
}
