using System;
namespace Yodii.Model
{
    public interface IFinalConfigurationItem
    {
        string ServiceOrPluginId { get; }
        ConfigurationStatus Status { get; }
    }
}
