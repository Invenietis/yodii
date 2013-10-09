using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    interface IConfigurationItem
    {
        public string ServiceOrPluginName { get; }
        public ConfigurationStatus Status { get; }
        public bool CanChangeStatus( ConfigurationStatus newStatus );
    }
}
