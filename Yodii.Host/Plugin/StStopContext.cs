using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    class StStopContext : StContext, IPreStopContext, IStopContext
    {
        public StStopContext( PluginProxy plugin, Dictionary<object, object> shared, bool mustDisable, bool disableOnly )
            : base( plugin, shared )
        {
            MustDisable = mustDisable;
            IsDisabledOnly = disableOnly;
        }

        internal readonly bool IsDisabledOnly;

        internal readonly bool MustDisable;

        public Action<IStartContext> RollbackAction { get; set; }

        bool IStopContext.CancellingPreStart { get { return false; } }

        bool IStopContext.HotSwapping
        {
            get { return Status == StStatus.StoppingHotSwap; }
        }

        public override string ToString()
        {
            return String.Format( "Stop: {0}, MustDisable={1}, IsDisabledOnly={2}", Plugin.PluginInfo.PluginFullName, MustDisable, IsDisabledOnly );
        }
    }
}
