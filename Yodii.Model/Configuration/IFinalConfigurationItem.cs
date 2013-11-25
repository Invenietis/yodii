using System;
namespace Yodii.Model
{
    interface IFinalConfigurationItem
    {
        string ServiceOrPluginId { get; }
        ConfigurationStatus Status { get; }
    }
}
