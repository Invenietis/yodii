using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model.CoreModel;

namespace Yodii.Model
{
    public interface IYodiiEngine
    {
        ICKObservableReadOnlyCollection<ILivePluginInfo> Plugins { get; }

        ICKObservableReadOnlyCollection<ILiveServiceInfo> Services { get; }

        ILiveServiceInfo FindService( IServiceInfo p );

        ILivePluginInfo FindPlugin( IPluginInfo p );

    }
}
