using System;
using CK.Core;
using System.Collections.Generic;

namespace Yodii.Model
{
    public interface IConfigurationLayerCollection : ICKObservableReadOnlyList<IConfigurationLayer>
    {
        IConfigurationLayer Create( string layerName = null );
        IYodiiEngineResult Remove( IConfigurationLayer layer );
        IReadOnlyCollection<IConfigurationLayer> this[string layerName] { get; }
    }
}
