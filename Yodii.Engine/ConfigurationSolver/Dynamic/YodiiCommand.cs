using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class YodiiCommand
    {
        public readonly object Caller;
        public readonly bool Start;
        public readonly Guid PluginId;
        public readonly StartDependencyImpact Impact;
        public readonly string FullName;

    }
}
