using System;
using CK.Core;
namespace Yodii.Model
{
    interface IConfigurationItemCollection : ICKObservableReadOnlyList<IConfigurationItem>
    {
        IYodiiEngineResult Add( string serviceOrPluginId, ConfigurationStatus status, string statusReason = "" );
        bool Contains( object item );
        int Count { get; }
        int IndexOf( object item );
        IYodiiEngineResult Remove( string serviceOrPluginId );
        IConfigurationItem this[int index] { get; }
        IConfigurationItem this[string key] { get; }
    }
}
