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

namespace Yodii.Lab
{
    /// <summary>
    /// Layout algorithm for Yodii's graph.
    /// </summary>
    class YodiiLayout : IExternalLayout<YodiiGraphVertex>
    {
        /// <summary>
        /// Margin between two elements on a horizontal line.
        /// </summary>
        static readonly int HORIZONTAL_MARGIN_SIZE = 50;

        /// <summary>
        /// Margin between two elements on a vertical line.
        /// </summary>
        static readonly int VERTICAL_MARGIN_SIZE = 50;

        readonly YodiiGraphArea _graphArea;

        /// <summary>
        /// Output positions of the vertices.
        /// </summary>
        readonly Dictionary<YodiiGraphVertex, Point> _vertexPositions;

        /// <summary>
        /// Input sizes of every vertex. Set by 
        /// </summary>
        IDictionary<YodiiGraphVertex, Size> _vertexSizes;

        readonly CKSortedArrayKeyList<YodiiGraphVertex, IServiceInfo> _serviceVertices;
        readonly CKSortedArrayKeyList<YodiiGraphVertex, IPluginInfo> _pluginVertices;

        readonly CKSortedArrayList<ServiceFamily> _rootFamilies;

        readonly CKSortedArrayKeyList<ServiceFamily, IServiceInfo> _serviceFamilies;
        readonly CKSortedArrayKeyList<YodiiGraphVertex, IPluginInfo> _orphanPlugins;

        public YodiiLayout( YodiiGraphArea graphArea )
        {
            _graphArea = graphArea;

            _serviceVertices = new CKSortedArrayKeyList<YodiiGraphVertex, IServiceInfo>(
                s => s.LabServiceInfo.ServiceInfo,
                ( a, b ) => String.Compare( a.ServiceFullName, b.ServiceFullName ),
                false
                );

            _pluginVertices = new CKSortedArrayKeyList<YodiiGraphVertex, IPluginInfo>(
                s => s.LabPluginInfo.PluginInfo,
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
                p => p.LabPluginInfo.PluginInfo,
                ( a, b ) => String.Compare( a.PluginFullName, b.PluginFullName ),
                false
                );

            _vertexPositions = new Dictionary<YodiiGraphVertex, Point>();
        }


        public void Compute()
        {
            Dictionary<YodiiGraphVertex,Point> d = null;
            Application.Current.Dispatcher.Invoke( DispatcherPriority.Normal, new Action( () =>
            {
                d = _graphArea.GetVertexPositions();

                _rootFamilies.Clear();

                _serviceVertices.Clear();
                _pluginVertices.Clear();

                _serviceFamilies.Clear();
                _orphanPlugins.Clear();
                _vertexPositions.Clear();

                CreateServiceFamilies();

                ComputeFamiliesSizes();

            } ) );
        }

        /// <summary>
        /// Whether this algorithm needs vertex sizes, and VertexSizes must be set before Compute() is called.
        /// </summary>
        public bool NeedVertexSizes
        {
            get { return true; }
        }

        /// <summary>
        /// Output vertex positions, called after Compute() returns.
        /// </summary>
        public System.Collections.Generic.IDictionary<YodiiGraphVertex, Point> VertexPositions
        {
            get { return _vertexPositions; }
        }

        /// <summary>
        /// Sizes of the vertices. Set before Compute() is called, when NeedVertexSizes is true.
        /// </summary>
        public System.Collections.Generic.IDictionary<YodiiGraphVertex, Size> VertexSizes
        {
            get
            {
                return _vertexSizes;
            }
            set
            {
                _vertexSizes = value;
            }
        }

        /// <summary>
        /// Splits all vertices between Plugin vertices and Service vertices,
        /// and generate families for all services.
        /// </summary>
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
                var family = FindOrCreateServiceFamily( s.LabServiceInfo.ServiceInfo );
                _serviceFamilies.Add( family );
            }

            // Bind plugins to their services
            foreach( var p in _pluginVertices )
            {
                if( p.LabPluginInfo.PluginInfo.Service != null )
                {
                    FindOrCreateServiceFamily( p.LabPluginInfo.PluginInfo.Service ).RegisterPlugin( p );
                }
                else
                {
                    _orphanPlugins.Add( p );
                }
            }
        }

        /// <summary>
        /// Computes sizes and positions for all service families,
        /// after they've been filled by CreateServiceFamilies.
        /// </summary>
        private void ComputeFamiliesSizes()
        {
            // Positions to add the vertices on. Starts at 0,0.
            double currentX = 0;
            double currentY = 0;

            foreach( var family in _rootFamilies )
            {
                // Set family at given point, set children positions, get total size
                Size familySize = family.ComputeFamilyPosition( new Point( currentX, currentY ) );

                // Next family will be added after a margin
                currentX += familySize.Width + VERTICAL_MARGIN_SIZE;
            }

            // Add plugins without services at the end
            foreach( var plugin in _orphanPlugins )
            {
                Point pluginPoint = new Point( currentX, 0 );

                // Set plugin output position
                _vertexPositions.Add( plugin, new Point( currentX, 0 ) );

                // Next orphan plugin added after a margin
                currentX += _vertexSizes[plugin].Width + VERTICAL_MARGIN_SIZE;
            }
        }

        /// <summary>
        /// Fins or attempts to create a ServiceFamily in the local collection.
        /// </summary>
        /// <param name="s">Service to look up.</param>
        /// <returns>Existing or new Service family.</returns>
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

        /// <summary>
        /// Service family for a given service vertex.
        /// Handles size, positioning, for itself and its children plugin/services.
        /// </summary>
        class ServiceFamily
        {
            // Root service info. Set in constructor.
            public readonly YodiiGraphVertex RootVertex;
            public readonly IServiceInfo RootService;

            // Children plugins and services.
            public readonly Dictionary<IPluginInfo,YodiiGraphVertex> SubPlugins;
            public readonly Dictionary<IServiceInfo,ServiceFamily> SubServices;

            /// <summary>
            /// Size of the service vertex at the root of this family.
            /// </summary>
            public readonly Size RootVertexSize;

            /// <summary>
            /// Service family of the service's Generalization, if applicable.
            /// Can be null.
            /// </summary>
            public readonly ServiceFamily ParentServiceFamily;

            /// <summary>
            /// Total size of the service family:
            /// Includes the service vertex at its root, and the size of all its childrens
            /// (in-between margins included)
            /// </summary>
            public Size FamilySize;

            readonly YodiiLayout _parent;

            /// <summary>
            /// Creates a new service family, setting the given service vertex at its root.
            /// </summary>
            /// <param name="rootVertex">Root vertex. Must be a service vertex.</param>
            /// <param name="parent">Parent layout. Used to query other vertice's sizes, among other things.</param>
            public ServiceFamily( YodiiGraphVertex rootVertex, YodiiLayout parent )
            {
                SubPlugins = new Dictionary<IPluginInfo, YodiiGraphVertex>();
                SubServices = new Dictionary<IServiceInfo, ServiceFamily>();

                Debug.Assert( rootVertex.IsService );

                _parent = parent;

                RootVertex = rootVertex;
                RootVertexSize = _parent.VertexSizes[RootVertex];
                RootService = rootVertex.LabServiceInfo.ServiceInfo;

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
            public Size ComputeFamilyPosition( Point rootPosition )
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
                    Size familySize = family.ComputeFamilyPosition( new Point( currentX, currentY ) );

                    subWidth += familySize.Width;
                    currentX += familySize.Width + HORIZONTAL_MARGIN_SIZE;

                    if( familySize.Height > subHeight ) subHeight = familySize.Height;
                }

                foreach( var plugin in SubPlugins.Values )
                {
                    _parent._vertexPositions.Add( plugin, new Point( currentX, currentY ) );

                    Size pluginSize = _parent._vertexSizes[plugin];

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

                _parent._vertexPositions.Add( RootVertex, position );

                return FamilySize;
            }

            internal void RegisterSubService( ServiceFamily s )
            {
                Debug.Assert( !SubServices.Keys.Contains( s.RootService ) );
                SubServices.Add( s.RootService, s );
            }

            internal void RegisterPlugin( YodiiGraphVertex p )
            {
                Debug.Assert( !SubPlugins.Keys.Contains( p.LabPluginInfo.PluginInfo ) );
                Debug.Assert( p.LabPluginInfo.PluginInfo.Service == RootService );
                SubPlugins.Add( p.LabPluginInfo.PluginInfo, p );
            }
        }
    }

}
