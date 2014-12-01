using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Host
{
    public class InvalidPluginDefinitionException : Exception
    {
        public InvalidPluginDefinitionException( string message )
            : base( message )
        {

        }
    }
}
