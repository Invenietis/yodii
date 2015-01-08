#region LGPL License
/*----------------------------------------------------------------------------
* This file (ObjectExplorer\Yodii.ObjectExplorer.Wpf\YodiiLayout.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using GraphX.GraphSharp.Algorithms.Layout;
using Yodii.Model;
using CK.Core;

namespace Yodii.ObjectExplorer.Wpf
{
    class YodiiLayout : LayoutAlgorithmBase<YodiiGraphVertex, YodiiGraphEdge, YodiiGraph>
    {
        static readonly int HORIZONTAL_MARGIN_SIZE = 30;
        static readonly int VERTICAL_MARGIN_SIZE = 30;

        public bool NextRecomputeForcesPositions = false;

        CKSortedArrayKeyList<YodiiGraphVertex, IServiceInfo> _serviceVertices;
        CKSortedArrayKeyList<YodiiGraphVertex, IPluginInfo> _pluginVertices;
        CKSortedArrayList<ServiceFamily> _rootFamilies;
        CKSortedArrayKeyList<ServiceFamily, IServiceInfo> _serviceFamilies;
        CKSortedArrayKeyList<YodiiGraphVertex, IPluginInfo> _orphanPlugins;

        protected override void InternalPreCompute()
        {

            _serviceVertices = new CKSortedArrayKeyList<YodiiGraphVertex, IServiceInfo>(
                s => s.LiveServiceInfo.ServiceInfo,
                ( a, b ) => String.Compare( a.ServiceFullName, b.ServiceFullName ),
                false
                );

            _pluginVertices = new CKSortedArrayKeyList<YodiiGraphVertex, IPluginInfo>(
                s => s.LivePluginInfo.PluginInfo,
                ( a, b ) => String.Compare( a.PluginFullName, b.PluginFullName ),
                false
                );

            _rootFamilies = new CKSortedArrayList<ServiceFamily>(
                ( a, b ) => String.Compare( a.RootService.ServiceFullName, b.RootService.ServiceFullName ),
                false
                );

            _serviceFamilies = new CKSortedArrayKeyList<ServiceFamily, IServiceInfo>(
                s => s.RootService,
                ( a, b ) => String.Compare( a.ServiceFullName, b.ServiceFullName ),
                false
                );

            _orphanPlugins = new CKSortedArrayKeyList<YodiiGraphVertex, IPluginInfo>(
                p => p.LivePluginInfo.PluginInfo,
                ( a, b ) => String.Compare( a.PluginFullName, b.PluginFullName ),
                false
                );

            _rootFamilies = new CKSortedArrayList<ServiceFamily>(
                ( a, b ) => String.Compare( a.RootService.ServiceFullName, b.RootService.ServiceFullName ),
                false
                );

        }

        protected override Point OnOriginalPosition( YodiiGraphVertex v, Point p )
        {
            // Keep existing positions
            if( p.IsValid() ) return p;

            Point newPoint = new Point( 0, 0 );
            if( VertexPositions == null ) return newPoint;
            if( v.IsService )
            {
                if( v.LiveServiceInfo.ServiceInfo.Generalization != null)
                {
                    // Find & get point of generalization
                    var generalizationQuery = VertexPositions.Where( kvp => kvp.Key.IsService && kvp.Key.LiveServiceInfo.ServiceInfo == v.LiveServiceInfo.ServiceInfo.Generalization );

                    if( generalizationQuery.Count() > 0 )
                    {
                        Point generalizationPoint = generalizationQuery.First().Value;
                        if( generalizationPoint.IsValid() )
                        {
                            newPoint.X = generalizationPoint.X;
                            newPoint.Y = generalizationPoint.Y + VERTICAL_MARGIN_SIZE;
                        }
                    }
                    
                }
            } else if (v.IsPlugin)
            {
                if( v.LivePluginInfo.PluginInfo.Service != null )
                {
                    // Find & get point of service
                    var serviceQuery = VertexPositions.Where( kvp => kvp.Key.IsService && kvp.Key.LiveServiceInfo.ServiceInfo == v.LivePluginInfo.PluginInfo.Service );

                    if( serviceQuery.Count() > 0 )
                    {
                        Point generalizationPoint = serviceQuery.First().Value;
                        if( generalizationPoint.IsValid() )
                        {
                            newPoint.X = generalizationPoint.X;
                            newPoint.Y = generalizationPoint.Y + VERTICAL_MARGIN_SIZE;
                        }
                    }

                }
            }

            return newPoint;
        }

        protected override void InternalCompute()
        {
            if( NextRecomputeForcesPositions )
            {
                CreateServiceFamilies();
                ComputeForcedPositions();

                NextRecomputeForcesPositions = false;
            }
        }

        public override bool NeedOriginalVertexPosition
        {
            get { return true; }
        }

        private void CreateServiceFamilies()
        {
            // Fill Static info => vertex dictionaries
            foreach( var v in VertexSizes.Keys )
            {
                if( v.IsService )
                {
                    _serviceVertices.Add( v );
                }
                else if( v.IsPlugin )
                {
                    _pluginVertices.Add( v );
                }
            }

            // Create service families
            foreach( var s in _serviceVertices )
            {
                var family = FindOrCreateServiceFamily( s.LiveServiceInfo.ServiceInfo );
                _serviceFamilies.Add( family );
            }

            // Bind plugins to their services
            foreach( var p in _pluginVertices )
            {
                if( p.LivePluginInfo.PluginInfo.Service != null )
                {
                    FindOrCreateServiceFamily( p.LivePluginInfo.PluginInfo.Service ).RegisterPlugin( p );
                }
                else
                {
                    _orphanPlugins.Add( p );
                }
            }
        }

        private void ComputeForcedPositions()
        {
            // Start at 0, 0
            double currentX = 0;
            double currentY = 0;

            foreach( var family in _rootFamilies )
            {
                // Add service family
                Size familySize = family.RecomputeForcedFamilyPosition( new Point( currentX, currentY ) );

                // Next X: Size + Margin
                currentX += familySize.Width + VERTICAL_MARGIN_SIZE;
            }

            // Add plugins without services at the end
            foreach( var plugin in _orphanPlugins )
            {
                Point pluginPoint = new Point( currentX, 0 );

                VertexPositions[plugin] = pluginPoint;

                currentX += VertexSizes[plugin].Width + VERTICAL_MARGIN_SIZE;
            }
        }

        private ServiceFamily FindOrCreateServiceFamily( IServiceInfo s )
        {
            bool exists;
            ServiceFamily family = _serviceFamilies.GetByKey( s, out exists );
            if( !exists )
            {
                family = new ServiceFamily( _serviceVertices.GetByKey( s ), this );

                if( family.ParentServiceFamily == null )
                {
                    _rootFamilies.Add( family );
                }
            }

            return family;
        }

        class ServiceFamily
        {
            public readonly YodiiGraphVertex RootVertex;
            public readonly IServiceInfo RootService;
            public readonly Dictionary<IPluginInfo,YodiiGraphVertex> SubPlugins;
            public readonly Dictionary<IServiceInfo,ServiceFamily> SubServices;
            public readonly Size RootVertexSize;
            public readonly ServiceFamily ParentServiceFamily;

            public Size FamilySize;

            readonly YodiiLayout _parent;

            public ServiceFamily( YodiiGraphVertex rootVertex, YodiiLayout parent )
            {
                SubPlugins = new Dictionary<IPluginInfo, YodiiGraphVertex>();
                SubServices = new Dictionary<IServiceInfo, ServiceFamily>();

                Debug.Assert( rootVertex.IsService );

                _parent = parent;

                RootVertex = rootVertex;
                RootVertexSize = _parent.VertexSizes[RootVertex];
                RootService = rootVertex.LiveServiceInfo.ServiceInfo;

                if( RootService.Generalization != null )
                {
                    var generalizationFamily = parent.FindOrCreateServiceFamily( RootService.Generalization );
                    ParentServiceFamily = generalizationFamily;
                    ParentServiceFamily.RegisterSubService( this );
                }

            }

            /// <summary>
            /// Calculates the size of this family, and adds its position to the vertex index.
            /// </summary>
            /// <param name="rootPosition">Starting position of this family (top left).</param>
            /// <returns>Size of this family, including the root, and all children.</returns>
            public Size RecomputeForcedFamilyPosition( Point rootPosition )
            {
                double width = 0;
                double height = 0;

                width = RootVertexSize.Width;
                height = RootVertexSize.Height;

                double subWidth = 0, subHeight = 0;

                double currentX = rootPosition.X;
                double currentY = rootPosition.Y + height + VERTICAL_MARGIN_SIZE;

                foreach( var family in SubServices.Values )
                {
                    // Add new family 
                    Size familySize = family.RecomputeForcedFamilyPosition( new Point( currentX, currentY ) );

                    subWidth += familySize.Width;
                    currentX += familySize.Width + HORIZONTAL_MARGIN_SIZE;

                    if( familySize.Height > subHeight ) subHeight = familySize.Height;
                }

                foreach( var plugin in SubPlugins.Values )
                {
                    Point pluginPosition =  new Point( currentX, currentY );
                    _parent.VertexPositions[plugin] = pluginPosition;

                    Size pluginSize = _parent.VertexSizes[plugin];

                    subWidth += pluginSize.Width;
                    if( pluginSize.Height > subHeight ) subHeight = pluginSize.Height;
                    currentX += pluginSize.Width + HORIZONTAL_MARGIN_SIZE;
                }

                double x = rootPosition.X;

                int subItemsCount = SubServices.Count + SubPlugins.Count;
                if( subItemsCount > 1 )
                {
                    subWidth += HORIZONTAL_MARGIN_SIZE * (subItemsCount - 1);
                    height = height + VERTICAL_MARGIN_SIZE + subHeight;

                    x = x + subWidth / 2.0 - RootVertexSize.Width / 2.0;
                }

                if( subWidth > width ) width = subWidth;

                FamilySize = new Size( width, height );

                Point position = new Point( x, rootPosition.Y );

                _parent.VertexPositions[RootVertex] = position;

                return FamilySize;
            }

            internal void RegisterSubService( ServiceFamily s )
            {
                Debug.Assert( !SubServices.Keys.Contains( s.RootService ) );
                SubServices.Add( s.RootService, s );
            }

            internal void RegisterPlugin( YodiiGraphVertex p )
            {
                Debug.Assert( !SubPlugins.Keys.Contains( p.LivePluginInfo.PluginInfo ) );
                Debug.Assert( p.LivePluginInfo.PluginInfo.Service == RootService );
                SubPlugins.Add( p.LivePluginInfo.PluginInfo, p );
            }
        }
    }

    static class PointExtensions
    {
        public static bool IsValid( this Point @this )
        {
            return !Double.IsNaN( @this.X ) && !Double.IsNaN( @this.Y );
        }
    }
}
