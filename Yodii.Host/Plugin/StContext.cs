using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Host
{
    class StContext
    {
        readonly Dictionary<object, object> _shared;
        CancellationInfo _info;

        public readonly PluginProxy Plugin;

        public StContext( PluginProxy plugin, Dictionary<object, object> shared )
        {
            Plugin = plugin;
            _shared = shared;
        }

        public void Cancel( string message = null, Exception ex = null )
        {
            _info = new CancellationInfo( Plugin.PluginKey ) { ErrorMessage = message, Error = ex };
        }

        public bool Success
        {
            get { return _info == null; }
        }

        public bool HandleSuccess( List<CancellationInfo> errors, bool isPreStart )
        {
            if( _info == null ) return true;
            _info.IsStartCanceled = isPreStart;
            errors.Add( _info );
            return false;
        }

        public IDictionary<object, object> SharedMemory { get { return _shared; } }

        public enum StStatus
        {
            Stopping = 1,
            Starting = 2,
            IsSwapping = 8,
            StoppingSwap = Stopping | IsSwapping,
            StartingSwap = Starting | IsSwapping,
            IsHotSwapping = IsSwapping + 16,
            StoppingHotSwap = Stopping | IsHotSwapping,
            StartingHotSwap = Starting | IsHotSwapping,
        }

        public StStatus Status { get; set; }

        /// <summary>
        /// Used when this is a PreStopContext that is a PreStartContext.PreviousPlugin.
        /// </summary>
        internal bool HotSwapped 
        {
            get { return Status > StStatus.IsHotSwapping; }
            set 
            { 
                if( value ) Status |= StStatus.IsHotSwapping; 
                else Status &= ~StStatus.IsHotSwapping; 
            } 
        }
    }

}
