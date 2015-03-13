#if DEBUG
using PropertyChanged;
using Yodii.Model;
using Yodii.ObjectExplorer.ViewModels;

namespace Yodii.ObjectExplorer.Mocks
{
    [ImplementPropertyChanged]
    public class MockLivePluginInfo : EmptyPropertyChangedHandler, ILivePluginInfo
    {

        #region ILivePluginInfo Members

        public ILiveServiceInfo Service { get; set; }

        public IPluginHostApplyCancellationInfo CurrentError { get; set; }

        #endregion

        #region ILiveYodiiItem Members

        public bool IsRunning { get; set; }

        public ILiveRunCapability Capability { get; set; }

        #endregion

        #region IDynamicSolvedYodiiItem Members

        public string FullName { get; set; }

        public bool IsPlugin { get; set; }

        public RunningStatus RunningStatus { get; set; }

        public string DisabledReason { get; set; }

        public ConfigurationStatus ConfigOriginalStatus { get; set; }

        public SolvedConfigurationStatus WantedConfigSolvedStatus { get; set; }

        public SolvedConfigurationStatus FinalConfigSolvedStatus { get; set; }

        public StartDependencyImpact ConfigOriginalImpact { get; set; }

        public StartDependencyImpact ConfigSolvedImpact { get; set; }

        #endregion

        #region IDynamicSolvedPlugin Members

        public IPluginInfo PluginInfo { get; set; }

        #endregion
    }
}
#endif