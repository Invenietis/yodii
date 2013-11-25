using System;
using System.ComponentModel;
namespace Yodii.Model
{
    interface IConfigurationLayer : INotifyPropertyChanged
    {
        IConfigurationManager ConfigurationManager { get; }
        IConfigurationItemCollection Items { get; }
        string LayerName { get; set; }
    }
}
