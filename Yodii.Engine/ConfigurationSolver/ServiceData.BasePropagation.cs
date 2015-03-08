#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\ServiceData.BasePropagation.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
            ServiceData _theOnlyService;
            int _nbTotalAvailablePlugins;
            int _nbAvailablePlugins;
            int _nbAvailableServices;

            /// <summary>
            /// Initializes a new StaticPropagation.
            /// </summary>
            protected BasePropagation( ServiceData s )
            {
                Service = s;
                _inclServices = new IEnumerable<ServiceData>[16];
                _exclServices = new IEnumerable<ServiceData>[16];
                _nbTotalAvailablePlugins = -1;
                _nbAvailablePlugins = -1;
                _nbAvailableServices = -1;
            }

            /// <summary>
            /// Initializes a new DynamicPropagation based on the StaticPropagation.
            /// </summary>
            protected BasePropagation( BasePropagation staticPropagation )
                : this( staticPropagation.Service )
            {
                Service = staticPropagation.Service;
                _nbTotalAvailablePlugins = staticPropagation._nbTotalAvailablePlugins;
                _nbAvailablePlugins = staticPropagation._nbAvailablePlugins;
                _nbAvailableServices = staticPropagation._nbAvailableServices;
                _theOnlyPlugin = staticPropagation._theOnlyPlugin;
                _theOnlyService = staticPropagation._theOnlyService;
                if( _theOnlyPlugin != null && _theOnlyService != null )
                {
                    Copy( staticPropagation._inclServices, _inclServices );
                    Copy( staticPropagation._exclServices, _exclServices );
                }
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

            public ServiceData TheOnlyService { get { return _theOnlyService; } }

            protected void Refresh( int nbTotalAvalaiblePlugins, int nbAvailablePlugins, int nbAvailableServices )
            {
                if( _nbTotalAvailablePlugins == nbTotalAvalaiblePlugins 
                    && _nbAvailablePlugins == nbAvailablePlugins
                    && _nbAvailableServices == nbAvailableServices ) return;

                _nbTotalAvailablePlugins = nbTotalAvalaiblePlugins;
                _nbAvailablePlugins = nbAvailablePlugins;
                _nbAvailableServices = nbAvailableServices;

                Debug.Assert( _nbTotalAvailablePlugins >= 1 );
                _theOnlyPlugin = null;
                _theOnlyService = null;
                Array.Clear( _inclServices, 0, 16 );
                Array.Clear( _exclServices, 0, 16 );
                // Retrieves the potential only plugin.
                if( _nbTotalAvailablePlugins == 1 )
                {
                    if( _nbAvailablePlugins == 0 )
                    {
                        ServiceData spec = Service.FirstSpecialization;
                        while( spec != null )
                        {
                            if( IsValidSpecialization( spec ) )
                            {
                                BasePropagation propSpec = GetPropagationInfo( spec );
                                Debug.Assert( propSpec != null );
                                if( propSpec.TheOnlyPlugin != null )
                                {
                                    Debug.Assert( _theOnlyPlugin == null );
                                    _theOnlyPlugin = propSpec.TheOnlyPlugin;
                                }
                            }
                            spec = spec.NextSpecialization;
                        }
                    }
                    else
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
                    Debug.Assert( _theOnlyPlugin != null && IsValidPlugin( _theOnlyPlugin ) );
                }
                else if( _nbAvailablePlugins == 0 && _nbAvailableServices == 1 )
                {
                    ServiceData spec = Service.FirstSpecialization;
                    while( spec != null )
                    {
                        if( IsValidSpecialization( spec ) )
                        {
                            Debug.Assert( _theOnlyService == null );
                            _theOnlyService = spec;
                        }
                        spec = spec.NextSpecialization;
                    }
                    Debug.Assert( _theOnlyService != null && IsValidSpecialization( _theOnlyService ) );
                }
                Debug.Assert( (_nbTotalAvailablePlugins == 1) == (_theOnlyPlugin != null) );
            }

            public abstract void Refresh();

            protected abstract bool IsValidPlugin( PluginData p );

            protected abstract bool IsValidSpecialization( ServiceData s );

            protected abstract BasePropagation GetPropagationInfo( ServiceData s );

            public IEnumerable<ServiceData> GetExcludedServices( StartDependencyImpact impact )
            {
                if( _theOnlyPlugin != null ) return _theOnlyPlugin.GetExcludedServices( impact );
                if( _theOnlyService != null ) return GetPropagationInfo( _theOnlyService ).GetExcludedServices( impact );

                impact = impact.ClearAllTryBits();
                int i = (int)impact >> 1;
                IEnumerable<ServiceData> exclExist = _exclServices[i];
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
                    _exclServices[i] = exclExist = excl ?? Enumerable.Empty<ServiceData>();
                }
                return exclExist;
            }

            public IEnumerable<ServiceData> GetIncludedServices( StartDependencyImpact impact )
            {
                if( _theOnlyPlugin != null ) return _theOnlyPlugin.GetIncludedServices( impact );
                if( _theOnlyService != null ) return GetPropagationInfo( _theOnlyService ).GetIncludedServices( impact );

                impact = impact.ClearAllTryBits();
                int i = (int)impact >> 1;
                IEnumerable<ServiceData> inclExist = _inclServices[ i ];
                if( inclExist == null )
                {
                    HashSet<ServiceData> incl = null;
                    ServiceData spec = Service.FirstSpecialization;
                    while( spec != null )
                    {
                        BasePropagation prop = GetPropagationInfo( spec );
                        if( prop != null )
                        {
                            if( incl == null ) incl = new HashSet<ServiceData>( prop.GetIncludedServices( impact ) );
                            else incl.IntersectWith( prop.GetIncludedServices( impact ) );
                        }
                        spec = spec.NextSpecialization;
                    }
                    PluginData p = Service.FirstPlugin;
                    while( p != null )
                    {
                        if( !p.Disabled )
                        {
                            if( incl == null ) incl = new HashSet<ServiceData>( p.GetIncludedServices( impact ) );
                            else incl.IntersectWith( p.GetIncludedServices( impact ) );
                        }
                        p = p.NextPluginForService;
                    }
                    _inclServices[i] = inclExist = incl ?? Service.InheritedServicesWithThis;
                }
                return inclExist;
            }
        }        
    }
}
