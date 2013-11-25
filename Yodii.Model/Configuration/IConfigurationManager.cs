using System;
using System.ComponentModel;
namespace Yodii.Model
{
    public interface IConfigurationManager : INotifyPropertyChanged
    {
        event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
        event EventHandler<ConfigurationChangingEventArgs> ConfigurationChanging;
        FinalConfiguration FinalConfiguration { get; }
        IConfigurationLayerCollection Layers { get; }
    }
}
