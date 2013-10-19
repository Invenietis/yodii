using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model.CoreModel;

namespace Yodii.Model.ConfigurationSolver
{
    partial class ServiceData
    {
        internal ServiceData _nextRunnableService;

        internal RunningStatus _status;
        PluginData[] _runnables;
        internal PluginData[] _allRunnables;
        PluginServiceRelation _firstRef;

        public RunningStatus Status
        {
            get { return _status; }
        }

        public bool IsRunning
        {
            get { return _status >= RunningStatus.Running; }
        }

        internal PluginServiceRelation AddServiceRef( PluginServiceRelation r )
        {
            PluginServiceRelation first = _firstRef;
            _firstRef = r;
            return first;
        }

        public virtual PluginData RunningPlugin
        {
            get
            {
                PluginData r = GeneralizationRoot.RunningPlugin;
                return r != null && IsMyPlugin( r ) ? r : null;
            }
        }

        public bool IsMyPlugin( PluginData p )
        {
            return p.IndexInAllServiceRunnables < _allRunnables.Length;
        }

        protected virtual PluginData InitializeDynamicState( PlanCalculatorStrategy strategy )
        {
            PluginData shouldRun = null;
            ServiceData spec = FirstSpecialization;
            while( spec != null )
            {
                shouldRun = spec.InitializeDynamicState( strategy );
                spec = spec.NextSpecialization;
            }
            _runnables = _allRunnables = PluginData.EmptyArray;
            if( Disabled ) _status = RunningStatus.Disabled;
            else
            {
                if( !ServiceInfo.IsDynamicService )
                {
                    _status = RunningStatus.RunningLocked;
                    return null;
                }
                if( AvailablePluginCount > 0 )
                {
                    _runnables = new PluginData[AvailablePluginCount];
                    int ip = 0;
                    PluginData p = FirstPlugin;
                    while( p != null )
                    {
                        if( !p.Disabled ) _runnables[ip++] = p;
                        p = p.NextPluginForService;
                    }
                    Array.Sort( _runnables, ( p1, p2 ) =>
                        {
                            // Want first the one that ShouldInitiallyRun.
                            int cmp = p2.ShouldInitiallyRun.CompareTo( p1.ShouldInitiallyRun );
                            // Among them privilegiate the one that is running.
                            if( cmp == 0 ) cmp = p2.IsCurrentlyRunning.CompareTo( p1.IsCurrentlyRunning );
                            // Then privilegiate the ones that want to try start by their config.
                            if( cmp == 0 ) cmp = p2.IsTryStartByConfig.CompareTo( p1.IsTryStartByConfig );
                            // Order by their PluginId to be deterministic.
                            if( cmp == 0 ) cmp = p2.PluginInfo.CompareTo( p1.PluginInfo );
                            return cmp;
                        } );
                    GeneralizationRoot.AddRunnableService( this );
                }
                if( TotalAvailablePluginCount > 0 )
                {
                    if( TotalAvailablePluginCount == AvailablePluginCount ) _allRunnables = _runnables;
                    else
                    {
                        _allRunnables = new PluginData[TotalAvailablePluginCount];
                        int sumLen = 0;
                        spec = FirstSpecialization;
                        while( spec != null )
                        {
                            int len = spec._allRunnables.Length;
                            if( len != 0 ) 
                            {
                                Array.Copy( spec._allRunnables, 0, _allRunnables, sumLen, len );
                                sumLen += len;
                            }
                            spec = spec.NextSpecialization;
                        }
                        Array.Copy( _runnables, 0, _allRunnables, sumLen, _runnables.Length );
                        Debug.Assert( _allRunnables.All( x => x != null ) );
                    }
                }
                if( MinimalRunningRequirement == RunningRequirement.Running )
                {
                    Debug.Assert( GeneralizationRoot._mustExistSpecialization == this || this.IsGeneralizationOf( GeneralizationRoot._mustExistSpecialization ) );
                    _status = RunningStatus.RunningLocked;
                    if( shouldRun == null )
                    {
                        Debug.Assert( _allRunnables.Length > 0 );
                        shouldRun = _allRunnables[0];
                    }
                }
                else
                {
                    if( shouldRun == null && _runnables.Length > 0 && _runnables[0].ShouldInitiallyRun )
                    {
                        shouldRun = _runnables[0];
                    }
                }
            }
            return shouldRun;
        }
        
    }
}
