using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class ObjectExplorerViewModel : EmptyPropertyChangedHandler
    {
        [AllowNull]
        public IYodiiEngine Engine { get; private set; }
        [AllowNull]
        public EngineViewModel EngineViewModel { get; private set; }

        public ObjectExplorerViewModel()
        {

        }

        public void LoadEngine( IYodiiEngine engine )
        {
            if( Engine != null ) { throw new InvalidOperationException( "Cannot load an Engine twice." ); }
            Engine = engine;

            EngineViewModel engineVm = new EngineViewModel();
            engineVm.LoadEngine( engine );

            EngineViewModel = engineVm;
        }
    }


}
