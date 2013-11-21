using System;
namespace Yodii.Model.Configuration
{
    interface IConfigurationManager
    {
        event EventHandler<Yodii.Model.ConfigurationChangedEventArgs> ConfigurationChanged;
        event EventHandler<Yodii.Model.ConfigurationChangingEventArgs> ConfigurationChanging;
        Yodii.Model.FinalConfiguration FinalConfiguration { get; }
        Yodii.Model.ConfigurationManager.ConfigurationLayerCollection Layers { get; }
    }
}
