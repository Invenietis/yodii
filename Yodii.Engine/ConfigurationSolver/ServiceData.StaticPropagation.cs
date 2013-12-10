using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;
using Yodii.Model;
using Yodii.Engine;
using CK.Core;

namespace Yodii.Engine
{
    partial class ServiceData
    {
        // This is protected for ServiceRootData to expose it.
        protected PluginData _theOnlyPlugin;
        CommonServiceReferences _commonReferences;

        /// <summary>
        /// Captures common service references of all plugins.
        /// </summary>
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
                        if( IsEmpty ) return false;
                    }
                    spec = spec.NextSpecialization;
                }
                PluginData p = s.FirstPlugin;
                while( p != null )
                {
                    if( !p.Disabled )
                    {
                        Add( s._allServices, p );
                        if( IsEmpty ) return false;
                    }
                    p = p.NextPluginForService;
                }
                Debug.Assert( _firstRef != null );
                return true;
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
        /// Captures excluded service references of all plugins.
        /// </summary>
        class ExcludedServiceReferences
        {
            HashSet<ServiceData> _services;

            public ExcludedServiceReferences()
            {
            }

            public IEnumerable<ServiceData> Services
            {
                get { return _services; }
            }

            public void Reset()
            {
                _services = null;
            }

            public bool IsEmpty
            {
                get { return _services != null && _services.Count == 0; }
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
                        if( spec._theOnlyPlugin != null ) Add( spec._theOnlyPlugin.ExcludedServices );
                        else Add( spec );
                        if( IsEmpty ) return false;
                    }
                    spec = spec.NextSpecialization;
                }
                PluginData p = s.FirstPlugin;
                while( p != null )
                {
                    if( !p.Disabled )
                    {
                        Add( p.ExcludedServices );
                        if( IsEmpty ) return false;
                    }
                    p = p.NextPluginForService;
                }
                Debug.Assert( !IsEmpty );
                return true;
            }

            private void Add( IEnumerable<ServiceData> services )
            {
                if( _services == null ) _services = new HashSet<ServiceData>( services );
                else _services.IntersectWith( services );
            }

            public void SetDisableStatusOnExcluded()
            {
                foreach( var s in _services )
                {
                    if( !s.Disabled ) s.SetDisabled( ServiceDisabledReason.ExcludingServiceIsBlocking );
                }
            }
        }

        /// <summary>
        /// Called by OnAllPluginsAdded or OnPluginDisabled if there is at least one available plugin.
        /// Called by SetSolvedConfigurationStatus whenever the SolvedConfigurationStatus becomes Runnable.
        /// </summary>
        void InitializePropagation( int nbAvailable, bool fromConfig )
        {
            Debug.Assert( nbAvailable == TotalAvailablePluginCount );
            if( nbAvailable == 1 )
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
                if( !_theOnlyPlugin.SetSolvedConfigurationStatus( ConfigSolvedStatus, PluginRunningRequirementReason.FromServiceToSinglePlugin ) 
                    && !Disabled )
                {
                    SetDisabled( ServiceDisabledReason.RequirementPropagationToSinglePluginFailed );
                }
            }
            else if( _commonReferences != null )
            {
                if( !_commonReferences.SetSolvedConfigurationStatus( ConfigSolvedStatus, ServiceSolvedConfigStatusReason.FromServiceToCommonPluginReferences ) 
                    && !Disabled )
                {
                    SetDisabled( ServiceDisabledReason.RequirementPropagationToCommonPluginReferencesFailed );
                }
            }
            if( ConfigSolvedStatus == SolvedConfigurationStatus.Running )
            {
                var excludedServices = new ExcludedServiceReferences();
                if( excludedServices.Add( this ) )
                {
                    excludedServices.SetDisableStatusOnExcluded();
                }
            }
        }

        void RetrieveTheOnlyPlugin( bool fromConfig )
        {
            Debug.Assert( _theOnlyPlugin == null && TotalAvailablePluginCount == 1 );
            if( AvailablePluginCount == 0 )
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
            // Forget our common service references.
            _commonReferences = null;
            
            // TheOnlyPlugin can be null when a service has been disabled:
            // - The disabled service disables its plugin one after the other.
            // - When a service is disabled, its OnPluginDisabled method do not call InitializePropagation: its TheOnlyPlugin is not set.
            // - The generalization.TotalAvailablePluginCount may fall to 1 that triggers this retrieval.
            // ==> There is no TheOnlyPlugin below.
            // This happens only when disabling a service and is only a temporary situation during when TotalAvailablePluginCount == 1 ==> TheOnlyPlugin != null
            // is not true.

            if( _theOnlyPlugin != null )
            {
                // As soon as the only plugin appears, propagate our requirement to it.
                var reason = fromConfig ? PluginRunningRequirementReason.FromServiceConfigToSinglePlugin : PluginRunningRequirementReason.FromServiceToSinglePlugin;
                _theOnlyPlugin.SetSolvedConfigurationStatus( ConfigSolvedStatus, reason );
            }
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
            if( ConfigSolvedStatus == SolvedConfigurationStatus.Running )
            {
                var excludedServices = new ExcludedServiceReferences();
                if( excludedServices.Add( this ) )
                {
                    excludedServices.SetDisableStatusOnExcluded();
                }
            }
        }
        
    }
}
