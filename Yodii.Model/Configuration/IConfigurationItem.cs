using System;
using System.ComponentModel;
namespace Yodii.Model
{
    interface IConfigurationItem : INotifyPropertyChanged
    {
        ConfigurationLayer Layer { get; }
        string ServiceOrPluginId { get; }
        IYodiiEngineResult SetStatus( Yodii.Model.ConfigurationStatus newStatus, string statusReason = "" );
        ConfigurationStatus Status { get; }
        string StatusReason { get; set; }
    }
}
