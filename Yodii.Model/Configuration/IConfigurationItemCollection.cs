using System;
using CK.Core;
namespace Yodii.Model
{
    public interface IConfigurationItemCollection : ICKObservableReadOnlyList<IConfigurationItem>
    {
        IYodiiEngineResult Add( string serviceOrPluginId, ConfigurationStatus status, string statusReason = "" );
        IYodiiEngineResult Remove( string serviceOrPluginId );
        IConfigurationItem this[string key] { get; }
    }
}
