using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public struct ConfigurationConflict
    {
        ConfigurationItem _disablerItem;
        ConfigurationItem _runnerItem;

        internal ConfigurationConflict( ConfigurationItem disabler, ConfigurationItem runner )
        {
            _disablerItem = disabler;
            _runnerItem = runner;
        }

        public ConfigurationItem DisablerItem
        {
            get { return _disablerItem; }
        }

        public ConfigurationItem RunnerItem
        {
            get { return _runnerItem; }
        }
    }
}
