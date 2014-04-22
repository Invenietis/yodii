using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Discoverer
{
    internal sealed class AssemblyInfo : IAssemblyInfo
    {
        readonly Uri _location;

        internal AssemblyInfo( Uri location )
        {
            _location = location;
        }

        public Uri AssemblyLocation
        {
            get { return _location; }
        }
    }
}
