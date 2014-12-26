using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using CK.Core;
using QuickGraph;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// Yodii graph. Handles binding and converting from ServiceInfo/PluginInfo collections.
    /// </summary>
    [DebuggerDisplay( "Vertices = {Vertices.Count}, Edges = {Edges.Count}, Services = {_serviceInfos.Count}, Plugins = {_pluginInfos.Count} " )]
    public class YodiiGraph : BidirectionalGraph<YodiiGraphVertex, YodiiGraphEdge>
    {
        int currentId = 0;
        /// <summary>
        /// Fired when graph content changes, requesting a new layout update.
        /// </summary>
        public event EventHandler<EventArgs> GraphUpdateRequested;

        readonly IObservableReadOnlyList<ILiveServiceInfo> _serviceInfos;
        readonly IObservableReadOnlyList<ILivePluginInfo> _pluginInfos;

        internal bool LockGraphUpdates = false;

        readonly IYodiiEngine _engine;

        /// <summary>
        /// Used for GraphX serialization. Not implemented.
        /// </summary>
        public YodiiGraph()
        { }

        internal YodiiGraph( IYodiiEngine engine )
            : base()
        {
            Debug.Assert( engine != null );
            _engine = engine;
            _serviceInfos = engine.LiveInfo.Services;
            _pluginInfos = engine.LiveInfo.Plugins;

            _serviceInfos.CollectionChanged += _serviceInfos_CollectionChanged;
            _pluginInfos.CollectionChanged += _pluginInfos_CollectionChanged;
            _engine.Configuration.ConfigurationChanged += _configurationManager_ConfigurationChanged;

            BuildInitialGraph();
        }

        void BuildInitialGraph()
        {
            this.Clear();
            foreach( var service in _serviceInfos ) CreateServiceVertex( service );
            foreach( var plugin in _pluginInfos ) CreatePluginVertex( plugin );
            RaiseGraphUpdateRequested();
            ReprocessVerticesWithConfiguration( _engine.Configuration.FinalConfiguration );
        }

        public IYodiiEngine Engine { get { return _engine; } }

        #region Properties
        internal IConfigurationManager ConfigurationManager
        {
            get { return _engine.Configuration; }
            //set
            //{
            //    Debug.Assert( value != null );
            //    _configurationManager = value;
            //    ReprocessVerticesWithConfiguration( _configurationManager.FinalConfiguration );
            //    _configurationManager.ConfigurationChanged += _configurationManager_ConfigurationChanged;
            //}
        }

        void _configurationManager_ConfigurationChanged( object sender, ConfigurationChangedEventArgs e )
        {
            ReprocessVerticesWithConfiguration( e.FinalConfiguration );
        }
        #endregion

        #region Private methods
        private void ReprocessVerticesWithConfiguration( FinalConfiguration config )
        {
            foreach( var v in Vertices ) SetVertexConfiguration( v, config );
        }

        static void SetVertexConfiguration( YodiiGraphVertex v, FinalConfiguration config = null )
        {
            if( config == null )
            {
                v.HasConfiguration = false;
                v.ConfigurationStatus = ConfigurationStatus.Optional;
            }
            else
            {
                FinalConfigurationItem item = config.Items.Where( x => x.ServiceOrPluginFullName == v.LiveObject.FullName ).SingleOrDefault();

                if( item.ServiceOrPluginFullName != null )
                {
                    v.HasConfiguration = true;
                    v.ConfigurationStatus = item.Status;
                }
                else
                {
                    v.HasConfiguration = false;
                    v.ConfigurationStatus = ConfigurationStatus.Optional;
                }
            }
        }

        private void RaiseVertexStatusChange()
        {
            foreach( YodiiGraphVertex vertex in this.Vertices )
            {
                vertex.RaiseStatusChange();
            }
        }

        YodiiGraphVertex CreateServiceVertex( ILiveServiceInfo liveService )
        {
            YodiiGraphVertex serviceVertex = new YodiiGraphVertex( this, liveService ) { ID = currentId++ };
            SetVertexConfiguration( serviceVertex, _engine.Configuration.FinalConfiguration );
            this.AddVertex( serviceVertex );


            if( liveService.ServiceInfo.Generalization != null )
            {
                YodiiGraphVertex generalizationVertex = FindOrCreateServiceVertex( liveService.ServiceInfo.Generalization );

                YodiiGraphEdge edge = new YodiiGraphEdge( serviceVertex, generalizationVertex, YodiiGraphEdgeType.Specialization ) { ID = currentId++ };

                this.AddEdge( edge );
            }


            return serviceVertex;
        }

        YodiiGraphVertex FindOrCreateServiceVertex( ILiveServiceInfo service )
        {
            YodiiGraphVertex serviceVertex = this.Vertices.FirstOrDefault( v => v.LiveServiceInfo == service );
            if( serviceVertex == null )
            {
                serviceVertex = CreateServiceVertex( service );
            }
            return serviceVertex;
        }

        YodiiGraphVertex FindOrCreateServiceVertex( IServiceInfo serviceInfo )
        {
            ILiveServiceInfo info = _serviceInfos.Where( s => s.ServiceInfo == serviceInfo ).First();

            return FindOrCreateServiceVertex( info );
        }

        YodiiGraphVertex CreatePluginVertex( ILivePluginInfo livePlugin )
        {
            YodiiGraphVertex pluginVertex = new YodiiGraphVertex( this, livePlugin ) { ID = this.currentId++ };
            SetVertexConfiguration( pluginVertex, _engine.Configuration.FinalConfiguration );
            this.AddVertex( pluginVertex );

            if( livePlugin.PluginInfo.Service != null )
            {
                YodiiGraphVertex serviceVertex = FindOrCreateServiceVertex( livePlugin.PluginInfo.Service );

                YodiiGraphEdge serviceEdge = new YodiiGraphEdge( pluginVertex, serviceVertex, YodiiGraphEdgeType.Implementation ) { ID = currentId++ };
                this.AddEdge( serviceEdge );
            }

            foreach( IServiceReferenceInfo reference in livePlugin.PluginInfo.ServiceReferences )
            {
                YodiiGraphVertex refVertex = FindOrCreateServiceVertex( reference.Reference );

                YodiiGraphEdge refEdge = new YodiiGraphEdge( pluginVertex, refVertex, reference ) { ID = currentId++ };
                this.AddEdge( refEdge );
            }

            return pluginVertex;
        }

        YodiiGraphVertex FindOrCreatePluginVertex( ILivePluginInfo plugin )
        {
            YodiiGraphVertex pluginVertex = this.Vertices.FirstOrDefault( v => v.LivePluginInfo == plugin );
            if( plugin == null )
            {
                pluginVertex = CreatePluginVertex( plugin );
            }
            return pluginVertex;
        }

        YodiiGraphVertex FindOrCreatePluginVertex( IPluginInfo pluginInfo )
        {
            ILivePluginInfo info = _pluginInfos.Where( p => p.PluginInfo == pluginInfo ).First();

            return FindOrCreatePluginVertex( info );
        }

        void RemovePluginVertex( ILivePluginInfo plugin )
        {
            YodiiGraphVertex toRemove = Vertices.Where( v => v.LivePluginInfo == plugin ).FirstOrDefault();

            if( toRemove != null )
            {
                RemoveVertex( toRemove );
            }
        }

        void ClearPluginVertices()
        {
            this.RemoveVertexIf( v => v.IsPlugin );
        }

        void RemoveServiceVertex( ILiveServiceInfo service )
        {
            YodiiGraphVertex toRemove = Vertices.Where( v => v.LiveServiceInfo == service ).FirstOrDefault();

            if( toRemove != null )
            {
                RemoveVertex( toRemove );
            }
        }

        void ClearServiceVertices()
        {
            this.RemoveVertexIf( v => v.IsService );
        }

        internal void RaiseGraphUpdateRequested()
        {
            if( this.GraphUpdateRequested != null && !LockGraphUpdates )
            {
                this.GraphUpdateRequested( this, new EventArgs() );
            }
        }
        #endregion Private methods

        #region Event handlers
        void _pluginInfos_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == NotifyCollectionChangedAction.Add )
            {
                foreach( var item in e.NewItems )
                {
                    CreatePluginVertex( (ILivePluginInfo)item );
                }
                RaiseGraphUpdateRequested();
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                foreach( var item in e.OldItems )
                {
                    RemovePluginVertex( (ILivePluginInfo)item );
                }
                RaiseGraphUpdateRequested();
            }
            else if( e.Action == NotifyCollectionChangedAction.Replace )
            {
                throw new NotImplementedException();
            }
            else if( e.Action == NotifyCollectionChangedAction.Reset )
            {
                ClearPluginVertices();
                RaiseGraphUpdateRequested();
            }
        }

        void _serviceInfos_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == NotifyCollectionChangedAction.Add )
            {
                foreach( var item in e.NewItems )
                {
                    CreateServiceVertex( (ILiveServiceInfo)item );
                    RaiseGraphUpdateRequested();
                }
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                foreach( var item in e.OldItems )
                {
                    RemoveServiceVertex( (ILiveServiceInfo)e.OldItems[0] );
                    RaiseGraphUpdateRequested();
                }
            }
            else if( e.Action == NotifyCollectionChangedAction.Replace )
            {
                throw new NotImplementedException();
            }
            else if( e.Action == NotifyCollectionChangedAction.Reset )
            {
                ClearServiceVertices();
                RaiseGraphUpdateRequested();
            }
        }
        #endregion Event handlers
    }
}
