using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Engine
{
    partial class ServiceData
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
        /// <summary>
        /// It goes likes this until we have gone through the whole graph (i.e. until the GeneralizationRoot):
        /// -> From the Service to Start, go to the upper node and set its RunningStatus to Running
        /// -> Set RunningStatus to Stopped to all child nodes except the one we come from (in this case, the one we want started in the first place)
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            if ( CanStart() )
            {
                DoStart();
                return true;
            }
            else return false;
        }

        public bool DoStart()
        {
            ServiceData rootNode = GeneralizationRoot;
            if ( rootNode != this )
            {
                ServiceData NeedsToRun = this;
                ServiceData Parent = Generalization;
                Debug.Assert( NeedsToRun.IsStrictGeneralizationOf( Parent ) );
                while ( Parent != rootNode )
                {
                    ServiceData s = Parent.FirstSpecialization;
                    while ( s != null && s != NeedsToRun )
                    {
                        s.DynamicStatus = RunningStatus.Stopped;
                        s = s.NextSpecialization;
                    }
                    NeedsToRun.DynamicStatus = RunningStatus.Running;
                    NeedsToRun = Parent;
                    Parent = Parent.Generalization;
                }
                ServiceData s1 = rootNode.FirstSpecialization;
                while ( s1 != null )
                {
                    if ( !s1.IsStrictGeneralizationOf( this ) ) s1.DynamicStatus = RunningStatus.Stopped;
                    s1 = s1.NextSpecialization;
                }
            }
            else
            {
                //In this case, we are the root so all we need to do is to Start.
                //No need to start the upper nodes (there is none) or to stop the upper nodes or upper nodes siblings
                _dynamicStatus = RunningStatus.Running;
            }
            return true;
        }
        public bool Stop()
        {
            if ( CanStop() ) 
            {
                DoStop();
                return true;
            }
            return false;
        }

        private void DoStop()
        {
            ServiceData rootNode = GeneralizationRoot;
            if ( rootNode != this )
            {
                ServiceData NeedsToRun = this;
                ServiceData Parent = Generalization;
                Debug.Assert( NeedsToRun.IsStrictGeneralizationOf( Parent ) );
                while ( Parent != rootNode )
                {
                    ServiceData s = Parent.FirstSpecialization;
                    while ( s != null && s != NeedsToRun )
                    {
                        s.DynamicStatus = RunningStatus.Stopped;
                        s = s.NextSpecialization;
                    }
                    NeedsToRun.DynamicStatus = RunningStatus.Stopped;
                    NeedsToRun = Parent;
                    Parent = Parent.Generalization;
                }
                ServiceData s1 = rootNode.FirstSpecialization;
                while ( s1 != null )
                {
                    if ( !s1.IsStrictGeneralizationOf( this ) ) s1.DynamicStatus = RunningStatus.Stopped;
                    s1 = s1.NextSpecialization;
                }
            }
            else
            {
                //In this case, we are the root so all we need to do is to Start.
                //No need to start the upper nodes (there is none) or to stop the upper nodes or upper nodes siblings
                _dynamicStatus = RunningStatus.Running;
            }
        }

        private bool CanStop()
        {
            if ( DynamicStatus.HasValue )
            {

            }
            return true;
        }
        public bool SetDynamicStatus(RunningStatus runningStatus)
        {
            return true;
        }

        internal bool CanStart()
        {
            ServiceData rootNode = GeneralizationRoot;
            if ( rootNode != this )
            {
                ServiceData NeedsToRun = this;
                ServiceData Parent = Generalization;
                Debug.Assert( NeedsToRun.IsStrictGeneralizationOf( Parent ) );
                while ( Parent != rootNode )
                {
                    if ( NeedsToRun.ThisMinimalRunningRequirement >= SolvedConfigurationStatus.Runnable
                        && Parent.ThisMinimalRunningRequirement >= SolvedConfigurationStatus.Runnable )
                    {
                        NeedsToRun = Parent;
                        Parent = Parent.Generalization;
                    }
                    else return false;
                }
                return true;
            }
            else
            {
                if ( rootNode.ThisMinimalRunningRequirement >= SolvedConfigurationStatus.Runnable ) return true;
                return false;
            }
        }
    }
}
