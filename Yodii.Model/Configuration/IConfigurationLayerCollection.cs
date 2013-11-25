using System;
using CK.Core;
namespace Yodii.Model
{
    interface IConfigurationLayerCollection : ICKObservableReadOnlyList<IConfigurationLayer>
    {
        IYodiiEngineResult Add( IConfigurationLayer layer );
        IYodiiEngineResult Remove( IConfigurationLayer layer );
    }
}
