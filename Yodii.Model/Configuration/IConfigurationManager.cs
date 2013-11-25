using System;
using System.ComponentModel;
namespace Yodii.Model
{
    interface IConfigurationManager : INotifyPropertyChanged
    {
        event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
        event EventHandler<ConfigurationChangingEventArgs> ConfigurationChanging;
        IFinalConfiguration FinalConfiguration { get; }
        IConfigurationLayerCollection Layers { get; }
    }
}
