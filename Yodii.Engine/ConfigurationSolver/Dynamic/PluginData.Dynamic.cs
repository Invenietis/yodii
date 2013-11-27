using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Engine
{
    partial class PluginData
    {
        RunningStatus? _dynamicStatus;

        public RunningStatus? DynamicStatus { get { return _dynamicStatus; } set { _dynamicStatus = value; } }

        public void ResetDynamicState()
        {
            switch ( ConfigSolvedStatus )
            {
                case SolvedConfigurationStatus.Disabled: _dynamicStatus = RunningStatus.Disabled; break;
                case SolvedConfigurationStatus.Running: _dynamicStatus = RunningStatus.RunningLocked; break;
                default: _dynamicStatus = null; break;
            }
        }

        public bool Start( StartDependencyImpact impact)
        {
            Debug.Assert( !Disabled && _configSolvedStatus >= SolvedConfigurationStatus.Runnable );
            Debug.Assert( _dynamicStatus == RunningStatus.Stopped );
            if(impact == StartDependencyImpact.None)
            {
                PropagationNoneImpact( Service );
                
            }
            
            if ( true )
            {
                //The plugin can be started now
                _dynamicStatus = RunningStatus.Running;
                Debug.Assert( _dynamicStatus == RunningStatus.Running );
            }
            //The plugin could not be started
            Debug.Assert( _dynamicStatus == RunningStatus.Stopped );
            return false;
        }
        bool PropagationNoneImpact()
        {
            //parcours tout le graph à partir du root, peut être trouver plus rapide ? 
            StopConcurrentPlugin( Service.GeneralizationRoot );

            List<IServiceReferenceInfo> serviceInfos = new List<IServiceReferenceInfo>();
            List<ServiceData> servicesToStart = new List<ServiceData>();

            foreach( var s in PluginInfo.ServiceReferences)
            {
                serviceInfos.Add(s);
            }
        }
        bool StopConcurrentPlugin(ServiceData service)
        {
            if ( service.DynamicStatus == RunningStatus.Stopped || service.DynamicStatus == RunningStatus.Disabled )
            {
                if ( service.NextSpecialization != null )
                    return StopConcurrentPlugin( service.NextSpecialization );
                else return false;
            }
            PluginData pd = service.FirstPlugin;
            while ( pd != null )
            {
                if ( pd.DynamicStatus == RunningStatus.Running )
                {
                    pd.DynamicStatus = RunningStatus.Stopped;
                    return true;
                }
                pd = pd.NextPluginForService;
            }
            return StopConcurrentPlugin( service.FirstSpecialization );
        }
        bool CheckReferencesWhenMustRun()
        {
            foreach ( var sRef in PluginInfo.ServiceReferences )
            {
                SolvedConfigurationStatus propagation = (SolvedConfigurationStatus)sRef.Requirement;
                if ( _configSolvedStatus < propagation ) propagation = _configSolvedStatus;

                ServiceData sr = _allServices[sRef.Reference];
                if ( !sr.SetSolvedConfigurationStatus( propagation, ServiceSolvedConfigStatusReason.FromMustExistReference ) )
                {
                    if ( !Disabled ) SetDisabled( PluginDisabledReason.RequirementPropagationToReferenceFailed );
                    break;
                }
            }
            return true;
        }


        public bool Stop()
        {
            return true;
        }
    }
}
