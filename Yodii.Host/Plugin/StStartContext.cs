using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    class StStartContext : StContext, IPreStartContext, IStartContext
    {
        public StStartContext( PluginProxy plugin, Dictionary<object, object> shared )
            : base( plugin, shared )
        {
        }

        public Action<IStopContext> RollbackAction { get; set; }

        public IYodiiService PreviousPluginCommonService { get; set; }

        public StContext PreviousImpl { get; set; }

        IYodiiPlugin IPreStartContext.PreviousPlugin { get { return PreviousImpl != null ? PreviousImpl.Plugin.RealPlugin : null; } }

        bool IPreStartContext.PreviousHotSwapping
        {
            get { return Status == StStatus.StartingHotSwap; }
            set
            {
                if( PreviousImpl == null ) throw new InvalidOperationException( R.PreviousPluginMustNotBeNull );
                PreviousImpl.HotSwapped = value;
                HotSwapped = value;
            }
        }

        bool IStartContext.CancellingPreStop
        {
            get { return false; }
        }

        bool IStartContext.HotSwapping
        {
            get { return Status == StStatus.StartingHotSwap; }
        }

    }

}
