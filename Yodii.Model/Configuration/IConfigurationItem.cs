using System;
using System.ComponentModel;
namespace Yodii.Model
{
    public interface IConfigurationItem : INotifyPropertyChanged
    {
        IConfigurationLayer Layer { get; }
        string ServiceOrPluginId { get; }
        IYodiiEngineResult SetStatus( ConfigurationStatus newStatus, string statusReason = "" );
        ConfigurationStatus Status { get; }
        string StatusReason { get; set; }
    }
}
