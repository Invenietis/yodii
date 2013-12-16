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
        internal abstract class BasePropagation
        {
            readonly HashSet<ServiceData>[] _inclServices;
            readonly HashSet<ServiceData>[] _exclServices;
            PluginData _theOnlyPlugin;
            int _nbAvailablePlugins;

            protected BasePropagation( ServiceData s )
            {
                Service = s;
                _inclServices = new HashSet<ServiceData>[10];
                _exclServices = new HashSet<ServiceData>[5];
            }

            protected BasePropagation( BasePropagation staticPropagation )
                : this( staticPropagation.Service )
            {
                Service = staticPropagation.Service;
                Copy( staticPropagation._inclServices, _inclServices );
                Copy( staticPropagation._exclServices, _exclServices );
                _nbAvailablePlugins = staticPropagation._nbAvailablePlugins;
            }

            static void Copy( HashSet<ServiceData>[] source, HashSet<ServiceData>[] dest )
            {
                for( int i = 0; i < source.Length; ++i )
                {
                    dest[i] = source[i] != null ? new HashSet<ServiceData>( source[i] ) : null;
                }
            }

            protected readonly ServiceData Service;

            public PluginData TheOnlyPlugin { get { return _theOnlyPlugin; } }

            protected void Refresh( int nbAvalaiblePlugins )
            {
                if( _nbAvailablePlugins == nbAvalaiblePlugins ) return;
                _nbAvailablePlugins = nbAvalaiblePlugins;
                Debug.Assert( _nbAvailablePlugins >= 1 );
                _theOnlyPlugin = null;
                Array.Clear( _inclServices, 0, 10 );
                Array.Clear( _exclServices, 0, 5 );
                // Retrieves the potential only plugin.
                if( _nbAvailablePlugins == 1 )
                {
                    ServiceData spec = Service.FirstSpecialization;
                    while( spec != null )
                    {
                        if( !spec.Disabled )
                        {
                            BasePropagation propSpec = GetUsefulPropagationInfo( spec );
                            if( propSpec.TheOnlyPlugin != null )
                            {
                                Debug.Assert( _theOnlyPlugin == null );
                                _theOnlyPlugin = propSpec.TheOnlyPlugin;
                            }
                        }
                        spec = spec.NextSpecialization;
                    }
                    if( _theOnlyPlugin == null )
                    {
                        PluginData p = Service.FirstPlugin;
                        while( p != null )
                        {
                            if( IsValidPlugin( p ) )
                            {
                                Debug.Assert( _theOnlyPlugin == null );
                                _theOnlyPlugin = p;
                            }
                            p = p.NextPluginForService;
                        }
                    }
                }
                Debug.Assert( _theOnlyPlugin == null || IsValidPlugin( _theOnlyPlugin ) );
                Debug.Assert( (_nbAvailablePlugins == 1) == (_theOnlyPlugin != null) );
            }

            public abstract void Refresh();

            protected abstract bool IsValidPlugin( PluginData p );

            protected abstract BasePropagation GetUsefulPropagationInfo( ServiceData s );

            public IEnumerable<ServiceData> GetExcludedServices( StartDependencyImpact impact )
            {
                if( _theOnlyPlugin != null ) return _theOnlyPlugin.GetExcludedServices( impact );
                HashSet<ServiceData> excl = _exclServices[(int)impact - 1];
                if( excl == null )
                {
                    ServiceData spec = Service.FirstSpecialization;
                    while( spec != null )
                    {
                        BasePropagation prop = GetUsefulPropagationInfo( spec );
                        if( prop != null )
                        {
                            if( excl == null ) excl = new HashSet<ServiceData>( prop.GetExcludedServices( impact ) );
                            else excl.IntersectWith( prop.GetExcludedServices( impact ) );
                        }
                        spec = spec.NextSpecialization;
                    }
                    PluginData p = Service.FirstPlugin;
                    while( p != null )
                    {
                        if( !p.Disabled )
                        {
                            if( excl == null ) excl = new HashSet<ServiceData>( p.GetExcludedServices( impact ) );
                            else excl.IntersectWith( p.GetExcludedServices( impact ) );
                        }
                        p = p.NextPluginForService;
                    }
                    _exclServices[(int)impact - 1] = excl;
                }
                return excl;
            }

            public IEnumerable<ServiceData> GetIncludedServices( StartDependencyImpact impact, bool forRunnableStatus )
            {
                if( _theOnlyPlugin != null ) return _theOnlyPlugin.GetIncludedServices( impact, forRunnableStatus );
                
                int iImpact = (int)impact;
                if( forRunnableStatus ) iImpact *= 2;
                --iImpact;
                
                HashSet<ServiceData> incl = _inclServices[iImpact];
                if( incl == null )
                {
                    ServiceData spec = Service.FirstSpecialization;
                    while( spec != null )
                    {
                        BasePropagation prop = GetUsefulPropagationInfo( spec );
                        if( prop != null )
                        {
                            if( incl == null ) incl = new HashSet<ServiceData>( prop.GetIncludedServices( impact, forRunnableStatus ) );
                            else incl.IntersectWith( prop.GetIncludedServices( impact, forRunnableStatus ) );
                        }
                        spec = spec.NextSpecialization;
                    }
                    PluginData p = Service.FirstPlugin;
                    while( p != null )
                    {
                        if( !p.Disabled )
                        {
                            if( incl == null ) incl = new HashSet<ServiceData>( p.GetIncludedServices( impact, forRunnableStatus ) );
                            else incl.IntersectWith( p.GetIncludedServices( impact, forRunnableStatus ) );
                        }
                        p = p.NextPluginForService;
                    }
                    _inclServices[iImpact] = incl;
                }
                return incl;
            }
        }        
    }
}
