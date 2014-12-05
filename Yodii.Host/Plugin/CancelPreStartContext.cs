using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    class CancelPreStartContext : IStopContext
    {
        readonly Dictionary<object, object> _shared;

        public CancelPreStartContext( Dictionary<object, object> shared )
        {
            _shared = shared;
        }

        public bool CancellingPreStart { get { return true; } }

        public IDictionary<object, object> SharedMemory { get { return _shared; } }

        public bool HotSwapping { get { return false; } }

    }
}