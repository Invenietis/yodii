//PluginData.Dynamic makes up with the PluginData partial class to work. 
//This file implements the strategy feature which is not our first concern at the moment... But we'll get back to it !

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;
using Yodii.Model.ConfigurationSolver;
using Yodii.Model.CoreModel;

namespace Yodii.Model.ConfigurationSolver
{
    partial class PluginData
    {
        RunningStatus _status;
        bool _shouldInitiallyRun;
        bool _isStructurallyRunnable;
        PluginServiceRelation[] _serviceReferences;

        internal int IndexInAllServiceRunnables;


        public bool ShouldInitiallyRun
        {
            get { return _shouldInitiallyRun; }
        }

        public bool IsStructurallyRunnable
        {
            get { return _isStructurallyRunnable; }
        }

        public RunningStatus Status
        {
            get { return _status; }
        }

        internal void SetStatusFromService( RunningStatus s )
        {
            Debug.Assert( s == RunningStatus.Stopped || s == RunningStatus.Running );
            Debug.Assert( _status != RunningStatus.RunningLocked || s == RunningStatus.Running, "RunningLocked (ie. this MustExistAndRun) => Service sets it to running." );
            if ( _status != RunningStatus.RunningLocked ) _status = s;
        }

        public int ComputeRunningCost()
        {
            int cost = 0;
            foreach ( var r in _serviceReferences )
            {
                switch ( r.Requirement )
                {
                    case RunningRequirement.Running:
                        if ( !r.Service.IsRunning ) return 0xFFFFFF;
                        break;
                    case RunningRequirement.RunnableTryStart:
                        if ( r.Service.Disabled ) return 0xFFFFFF;
                        if ( !r.Service.IsRunning ) cost += 10;
                        break;
                    case RunningRequirement.Runnable:
                        if ( r.Service.Disabled ) return 0xFFFFFF;
                        break;
                    case RunningRequirement.OptionalTryStart:
                        // If a service is disabled, it is useless to increment the cost
                        // since no configuration will be able to satisfy it.
                        if ( r.Service.Status == RunningStatus.Stopped ) cost += 10;
                        break;
                }
            }
            return cost;
        }

        private bool ThisMoveNext()
        {
            if ( _status == RunningStatus.Running )
            {
                _status = RunningStatus.Stopped;
                return _shouldInitiallyRun;
            }
            _status = RunningStatus.Running;
            return !_shouldInitiallyRun;
        }
    }
}
