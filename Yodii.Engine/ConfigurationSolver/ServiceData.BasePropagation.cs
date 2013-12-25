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
            readonly IEnumerable<ServiceData>[] _inclServices;
            readonly IEnumerable<ServiceData>[] _exclServices;
            PluginData _theOnlyPlugin;
            int _nbAvailablePlugins;

            protected BasePropagation( ServiceData s )
            {
                Service = s;
                _inclServices = new IEnumerable<ServiceData>[10];
                _exclServices = new IEnumerable<ServiceData>[5];
                _nbAvailablePlugins = -1;
            }

            protected BasePropagation( BasePropagation staticPropagation )
                : this( staticPropagation.Service )
            {
                Service = staticPropagation.Service;
                Copy( staticPropagation._inclServices, _inclServices );
                Copy( staticPropagation._exclServices, _exclServices );
                _nbAvailablePlugins = -1;
            }

            static void Copy( IEnumerable<ServiceData>[] source, IEnumerable<ServiceData>[] dest )
            {
                for( int i = 0; i < source.Length; ++i )
                {
                    dest[i] = source[i] != null ? source[i].ToReadOnlyList() : null;
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
                        if( IsValidSpecialization( spec ) )
                        {
                            BasePropagation propSpec = GetPropagationInfo( spec );
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

            protected abstract bool IsValidSpecialization( ServiceData s );

            protected abstract BasePropagation GetPropagationInfo( ServiceData s );

            public IEnumerable<ServiceData> GetExcludedServices( StartDependencyImpact impact )
            {
                if( _theOnlyPlugin != null ) return _theOnlyPlugin.GetExcludedServices( impact );
                IEnumerable<ServiceData> exclExist = _exclServices[(int)impact - 1];
                if( exclExist == null )
                {
                    HashSet<ServiceData> excl = null;
                    ServiceData spec = Service.FirstSpecialization;
                    while( spec != null )
                    {
                        BasePropagation prop = GetPropagationInfo( spec );
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
                    _exclServices[(int)impact - 1] = exclExist = excl ?? Enumerable.Empty<ServiceData>();
                }
                return exclExist;
            }

            public IEnumerable<ServiceData> GetIncludedServices( StartDependencyImpact impact, bool forRunnableStatus )
            {
                if( _theOnlyPlugin != null ) return _theOnlyPlugin.GetIncludedServices( impact, forRunnableStatus );
                
                int iImpact = (int)impact;
                if( forRunnableStatus ) iImpact *= 2;
                --iImpact;

                IEnumerable<ServiceData> inclExist = _inclServices[iImpact];
                if( inclExist == null )
                {
                    HashSet<ServiceData> incl = null;
                    ServiceData spec = Service.FirstSpecialization;
                    while( spec != null )
                    {
                        BasePropagation prop = GetPropagationInfo( spec );
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
                    _inclServices[iImpact] = inclExist = incl ?? Service.InheritedServicesWithThis;
                }
                return inclExist;
            }
        }        
    }
}
