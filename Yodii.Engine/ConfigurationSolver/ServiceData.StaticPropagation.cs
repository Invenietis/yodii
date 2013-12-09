using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;
using Yodii.Model;
using Yodii.Engine.ConfigurationSolver;

namespace Yodii.Engine
{
    partial class ServiceData
    {
        // This is internal for ServiceRootData to expose it.
        internal PluginData _theOnlyPlugin;
        CommonServiceReferences _commonReferences;

        class CommonServiceReferences
        {
            Ref _firstRef;
            bool _initialized;

            class Ref
            {
                public readonly ServiceData Service;
                public DependencyRequirement Requirement;
                public Ref NextRef;

                public Ref( Ref next, ServiceData s, DependencyRequirement r )
                {
                    Service = s;
                    Requirement = r;
                    NextRef = next;
                }
            }

            public CommonServiceReferences()
            {
            }

            public void Reset()
            {
                _initialized = false;
                _firstRef = null;
            }

            public bool IsEmpty
            {
                get { return _initialized && _firstRef == null; }
            }

            /// <summary>
            /// Returns true if there is at least one Service referenced by ALL of our implementations. 
            /// </summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public bool Add( ServiceData s )
            {
                ServiceData spec = s.FirstSpecialization;
                while( spec != null )
                {
                    if( !spec.Disabled )
                    {
                        if( spec._theOnlyPlugin != null ) Add( s._allServices, spec._theOnlyPlugin );
                        else if( spec._commonReferences != null ) Add( spec._commonReferences );
                        else Add( spec );
                        if( IsEmpty ) break;
                    }
                    spec = spec.NextSpecialization;
                }
                PluginData p = s.FirstPlugin;
                while( p != null )
                {
                    if( !p.Disabled )
                    {
                        Add( s._allServices, p );
                        if( IsEmpty ) break;
                    }
                    p = p.NextPluginForService;
                }
                return _firstRef != null;
            }

            public bool SetSolvedConfigurationStatus( SolvedConfigurationStatus solvedStatus, ServiceSolvedConfigStatusReason reason )
            {
                Debug.Assert( solvedStatus >= SolvedConfigurationStatus.Runnable );
                Ref r = _firstRef;
                while( r != null )
                {
                    SolvedConfigurationStatus propagate = (SolvedConfigurationStatus)r.Requirement;
                    if( propagate > solvedStatus ) propagate = solvedStatus;
                    if( !r.Service.SetSolvedConfigurationStatus( propagate, reason ) ) return false;
                    r = r.NextRef;
                }
                return true;
            }

            #region Private
            void Add( CommonServiceReferences csr )
            {
                if( _initialized ) IntersectWith( csr );
                else 
                {
                    Initialize( csr );
                    _initialized = true;
                }
            }

            void Add( Dictionary<string, ServiceData> allServices, PluginData p )
            {
                if( _initialized ) IntersectWith( p );
                else 
                {
                    Initialize( allServices, p );
                    _initialized = true;
                }
            }

            void Initialize( Dictionary<string, ServiceData> allServices, PluginData p )
            {
                foreach( IServiceReferenceInfo sr in p.PluginInfo.ServiceReferences )
                {
                    _firstRef = new Ref( _firstRef, allServices[sr.Reference.ServiceFullName], sr.Requirement );
                }
            }

            void IntersectWith( PluginData p )
            {
                if( _firstRef == null ) return;
                Ref newFirst = null;
                foreach( IServiceReferenceInfo sr in p.PluginInfo.ServiceReferences )
                {
                    Ref exist = Find( sr.Reference );
                    if( exist != null )
                    {
                        if( exist.Requirement > sr.Requirement ) exist.Requirement = sr.Requirement;
                        exist.NextRef = newFirst;
                        newFirst = exist;
                    }
                }
                _firstRef = newFirst;
            }

            void Initialize( CommonServiceReferences csr )
            {
                Ref r = csr._firstRef;
                while( r != null )
                {
                    _firstRef = new Ref( _firstRef, r.Service, r.Requirement );
                    r = r.NextRef;
                }
            }

            void IntersectWith( CommonServiceReferences csr )
            {
                if( _firstRef == null ) return;
                Ref newFirst = null;
                Ref r = csr._firstRef;
                while( r != null )
                {
                    Ref exist = Find( r.Service.ServiceInfo );
                    if( exist != null )
                    {
                        if( exist.Requirement > r.Requirement ) exist.Requirement = r.Requirement;
                        exist.NextRef = newFirst;
                        newFirst = exist;
                    }
                    r = r.NextRef;
                }
                _firstRef = newFirst;
            }

            Ref Find( IServiceInfo s )
            {
                Ref r = _firstRef;
                while( r != null )
                {
                    if( r.Service.ServiceInfo == s ) return r;
                    r = r.NextRef;
                }
                return null;
            }
            #endregion
        
        }

        /// <summary>
        /// Called by OnAllPluginsAdded or OnPluginDisabled if there is at least one available plugin.
        /// Called by SetSolvedConfigurationStatus whenever the SolvedConfigurationStatus becomes Runnable.
        /// </summary>
        void InitializePropagation( int nbAvailable, bool fromConfig )
        {
            Debug.Assert( nbAvailable == TotalAvailablePluginCount );
            if ( nbAvailable == 1 )
            {
                if( _theOnlyPlugin == null ) RetrieveTheOnlyPlugin( fromConfig );
            }
            else if( ConfigSolvedStatus >= SolvedConfigurationStatus.Runnable )
            {
                RetrieveOrUpdateTheCommonServiceReferences( fromConfig );
            }
        }

        void PropagateRunningRequirementToOnlyPluginOrCommonReferences()
        {
            if( _theOnlyPlugin != null ) 
            {
                if( !_theOnlyPlugin.SetSolvedConfigurationStatus( _configSolvedStatus, PluginRunningRequirementReason.FromServiceToSinglePlugin ) 
                    && !Disabled )
                {
                    SetDisabled( ServiceDisabledReason.RequirementPropagationToSinglePluginFailed );
                }
            }
            else if( _commonReferences != null )
            {
                if( !_commonReferences.SetSolvedConfigurationStatus( _configSolvedStatus, ServiceSolvedConfigStatusReason.FromServiceToCommonPluginReferences ) 
                    && !Disabled )
                {
                    SetDisabled( ServiceDisabledReason.RequirementPropagationToCommonPluginReferencesFailed );
                }
            }
        }

        void RetrieveTheOnlyPlugin( bool fromConfig )
        {
            Debug.Assert( _theOnlyPlugin == null && TotalAvailablePluginCount == 1 );
            if ( AvailablePluginCount == 0 )
            {
                ServiceData spec = FirstSpecialization;
                while( spec != null )
                {
                    if( spec._theOnlyPlugin != null )
                    {
                        _theOnlyPlugin = spec._theOnlyPlugin;
                        break;
                    }
                    spec = spec.NextSpecialization;
                }
            }
            else
            {
                _theOnlyPlugin = FirstPlugin;
                while( _theOnlyPlugin.Disabled ) _theOnlyPlugin = _theOnlyPlugin.NextPluginForService;
            }
            Debug.Assert( _theOnlyPlugin != null );
            // Forget our common service references.
            _commonReferences = null;
            // As soon as the only plugin appears, propagate our requirement to it.
            var reason = fromConfig ? PluginRunningRequirementReason.FromServiceConfigToSinglePlugin : PluginRunningRequirementReason.FromServiceToSinglePlugin;
            _theOnlyPlugin.SetSolvedConfigurationStatus( ConfigSolvedStatus, reason );
        }
        
        void RetrieveOrUpdateTheCommonServiceReferences( bool fromConfig )
        {
            Debug.Assert( !Disabled && ConfigSolvedStatus >= SolvedConfigurationStatus.Runnable && TotalAvailablePluginCount > 1 );
            if( _commonReferences == null ) _commonReferences = new CommonServiceReferences();
            _commonReferences.Reset();
            if( _commonReferences.Add( this ) )
            {
                var reason = fromConfig ? ServiceSolvedConfigStatusReason.FromServiceConfigToCommonPluginReferences : ServiceSolvedConfigStatusReason.FromServiceToCommonPluginReferences;
                if( !_commonReferences.SetSolvedConfigurationStatus( ConfigSolvedStatus, reason ) )
                {
                    if( !Disabled ) SetDisabled( ServiceDisabledReason.RequirementPropagationToSinglePluginFailed );
                }
            }
        }

        public bool DisableSiblingServices()
        {        
            List<ServiceData> _mustBeDisabledServices = new List<ServiceData>();
            Debug.Assert( !Disabled && ConfigSolvedStatus == SolvedConfigurationStatus.Running );
            ServiceData root = GeneralizationRoot;
            ServiceData parent = Generalization;
            ServiceData s = this;

            while ( s != root )
            {
                ServiceData sibling = parent.FirstSpecialization;
                while ( sibling != null && sibling != s)
                {
                    if ( sibling.ConfigSolvedStatus == SolvedConfigurationStatus.Running )
                    {
                        if ( !sibling.Disabled )
                        {
                            sibling.SetDisabled( ServiceDisabledReason.SiblingIsRunning );
                            _mustBeDisabledServices.Add( sibling );
                        }
                    }
                    sibling = parent.NextSpecialization;
                }
                s = parent;
                parent = parent.Generalization;   
            }
            return true;
       }
        
    }
}
