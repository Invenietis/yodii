using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine.Tests.Mocks
{
    static class AssemblyInfoHelper
    {
        private static MockAssemblyInfo _thisAssemblyInfo = null;

        internal static MockAssemblyInfo ExecutingAssemblyInfo
        {
            get
            {
                if( _thisAssemblyInfo == null )
                {
                    string filename = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                    _thisAssemblyInfo = new MockAssemblyInfo( filename );
                }
                return _thisAssemblyInfo;
            }
        }
    }
}
