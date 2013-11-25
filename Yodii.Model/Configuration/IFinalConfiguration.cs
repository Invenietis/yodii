using System;
using System.Collections.Generic;
namespace Yodii.Model
{
    public interface IFinalConfiguration
    {
        ConfigurationStatus GetStatus( string serviceOrPluginId );
        IReadOnlyList<IFinalConfigurationItem> Items { get; }
    }
}
