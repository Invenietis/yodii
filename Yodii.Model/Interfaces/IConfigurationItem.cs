using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    interface IConfigurationItem
    {
        string ServiceOrPluginName { get; }
        ConfigurationStatus Status { get; }
        bool CanChangeStatus( ConfigurationStatus newStatus );
    }
}
