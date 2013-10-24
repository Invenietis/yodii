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
    }
}
