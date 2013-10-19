using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model.CoreModel;

namespace Yodii.Model.ConfigurationSolver
{
    class PluginServiceRelation
    {
        public readonly PluginData Plugin;
        public readonly RunningRequirement Requirement;
        public readonly ServiceData Service;
        
        public PluginServiceRelation( PluginData p, RunningRequirement r, ServiceData s )
        {
            Requirement = r;
            Service = s;
            Plugin = p;
            NextServiceRef = s.AddServiceRef( this );
        }

        /// <summary>
        /// Support for the linked list of plugin references for a service.
        /// </summary>
        public readonly PluginServiceRelation NextServiceRef;

        /// <summary>
        /// Gets whether this relation is necessarily satisfied regardless of dynamic state of the system.
        /// </summary>
        public bool IsStructurallySatisfied
        {
            get 
            {
                // When Requirement is MustExistAndRun, the service must be running.
                if( Requirement == RunningRequirement.Running ) return Service.MinimalRunningRequirement == RunningRequirement.Running;
                // When Requirement is MustExist or MustExistTryStart, the service must exist. 
                if( Requirement >= RunningRequirement.Runnable ) return Service.MinimalRunningRequirement >= RunningRequirement.Runnable;
                // When Requirement is optional, the service can be in any state.
                return true;
            }
        }
    }
}
