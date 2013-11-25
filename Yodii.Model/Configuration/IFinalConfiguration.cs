using System;
using System.Collections.Generic;
namespace Yodii.Model
{
    interface IFinalConfiguration
    {
        ConfigurationStatus GetStatus( string serviceOrPluginId );
        IReadOnlyList<IFinalConfigurationItem> Items { get; }
    }
}
