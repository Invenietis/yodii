using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    class CancelPresStopContext : IStartContext
    {
        readonly Dictionary<object, object> _shared;

        public CancelPresStopContext( Dictionary<object, object> shared )
        {
            _shared = shared;
        }

        public bool CancellingPreStop { get { return true; } }

        public IDictionary<object, object> SharedMemory { get { return _shared; } }

        public bool HotSwapping { get { return false; } }
    }
}
