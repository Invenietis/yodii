using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace Yodii.Model
{
    public class StaticResolution
    {
        private FinalConfiguration _finalConfiguration;
        private Discoverer _discoverer;

        internal StaticResolution( FinalConfiguration finalConfig, Discoverer discoverer )
        {
            if ( finalConfig != null && discoverer != null)
            {
                _finalConfiguration = finalConfig;
                _discoverer = discoverer;
            }
        }

        public void Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
