using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Updater.Tests
{
    public static class AssemblyExtensions
    {
        public static string GetAssemblyDirectory( this Assembly @this )
        {
            string codeBase = @this.CodeBase;
            UriBuilder uri = new UriBuilder( codeBase );
            string path = Uri.UnescapeDataString( uri.Path );
            return Path.GetDirectoryName( path );
        }
    }
}
