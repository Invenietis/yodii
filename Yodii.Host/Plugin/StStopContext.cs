using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    class StStopContext : StContext, IPreStopContext, IStopContext
    {
        public StStopContext( PluginProxy plugin, Dictionary<object, object> shared )
            : base( plugin, shared )
        {
        }

        public Action<IStartContext> RollbackAction { get; set; }

        bool IStopContext.CancellingPreStart { get { return false; } }

        bool IStopContext.HotSwapping
        {
            get { return Status == StStatus.StoppingHotSwap; }
        }
    }
}
