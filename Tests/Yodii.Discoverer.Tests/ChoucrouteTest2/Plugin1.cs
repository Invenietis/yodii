using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Discoverer.Tests
{
    public class Plugin1 : YodiiPluginBase, IYodiiPlugin, IService2
    {
        readonly string _pluginFullName;

        public Plugin1( string pluginFullName )
        {
            _pluginFullName = pluginFullName;
        }

        public Plugin1()
        {
        }
    }
}
