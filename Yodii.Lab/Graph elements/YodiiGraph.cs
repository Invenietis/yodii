using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using CK.Core;
using QuickGraph;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab
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

        readonly ICKObservableReadOnlyCollection<LabServiceInfo> _serviceInfos;
        readonly ICKObservableReadOnlyCollection<LabPluginInfo> _pluginInfos;
        readonly LabStateManager _serviceManager;

        internal bool LockGraphUpdates = false;

        IConfigurationManager _configurationManager;

        #region Constructor
        /// <summary>
        /// Used for GraphX serialization. Not implemented.
        /// </summary>
        public YodiiGraph()
        { }

        internal YodiiGraph( IConfigurationManager configManager, LabStateManager serviceManager )
            : base()
        {
            Debug.Assert( serviceManager != null );
            Debug.Assert( configManager != null );

            _serviceInfos = serviceManager.LabServiceInfos;
            _pluginInfos = serviceManager.LabPluginInfos;
            _configurationManager = configManager;
            _serviceManager = serviceManager;

            _serviceInfos.CollectionChanged += _serviceInfos_CollectionChanged;
            _pluginInfos.CollectionChanged += _pluginInfos_CollectionChanged;
            _configurationManager.ConfigurationChanged += _configurationManager_ConfigurationChanged;

            UpdateVerticesWithConfiguration( _configurationManager.FinalConfiguration );
        }
        #endregion Constructor

        #region Properties
        internal IConfigurationManager ConfigurationManager
        {
            get { return _configurationManager; }
            set
            {
                Debug.Assert( value != null );
                _configurationManager = value;
                UpdateVerticesWithConfiguration( _configurationManager.FinalConfiguration );
                _configurationManager.ConfigurationChanged += _configurationManager_ConfigurationChanged;
            }
        }

        void _configurationManager_ConfigurationChanged( object sender, ConfigurationChangedEventArgs e )
        {
            UpdateVerticesWithConfiguration( e.FinalConfiguration );
        }
        #endregion

        #region Internal methods
        internal void RemoveService( ServiceInfo service )
        {
            _serviceManager.RemoveService( service );
        }
        internal void RemovePlugin( PluginInfo plugin )
        {
            _serviceManager.RemovePlugin( plugin );
        }
        #endregion Internal methods

        #region Private methods
        private void UpdateVerticesWithConfiguration( FinalConfiguration config )
        {
            if( config == null )
            {
                foreach( var v in Vertices )
                {
                    v.HasConfiguration = false;
                    v.ConfigurationStatus = ConfigurationStatus.Optional;
                }
            }
            else
            {

                foreach( var v in Vertices )
                {
                    string identifier;
                    if( v.IsService )
                        identifier = v.LabServiceInfo.ServiceInfo.ServiceFullName;
                    else
                        identifier = v.LabPluginInfo.PluginInfo.PluginFullName;

                    var items = config.Items.Where( x => x.ServiceOrPluginFullName == identifier );
                    if( items.Count() > 0 )
                    {
                        v.HasConfiguration = true;
                        v.ConfigurationStatus = items.First().Status;
                    }
                    else
                    {
                        v.HasConfiguration = false;
                        v.ConfigurationStatus = ConfigurationStatus.Optional;
                    }
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

        YodiiGraphVertex CreateServiceVertex( LabServiceInfo liveService )
        {
            YodiiGraphVertex serviceVertex = new YodiiGraphVertex( this, liveService ) { ID = currentId++ };
            this.AddVertex( serviceVertex );
            liveService.ServiceInfo.PropertyChanged += ServiceInfo_PropertyChanged;


            if( liveService.ServiceInfo.Generalization != null )
            {
                YodiiGraphVertex generalizationVertex = FindOrCreateServiceVertex( liveService.ServiceInfo.Generalization );

                YodiiGraphEdge edge = new YodiiGraphEdge( serviceVertex, generalizationVertex, YodiiGraphEdgeType.Specialization ) { ID = currentId++ };

                this.AddEdge( edge );
            }


            return serviceVertex;
        }

        YodiiGraphVertex FindOrCreateServiceVertex( LabServiceInfo service )
        {
            YodiiGraphVertex serviceVertex = this.Vertices.FirstOrDefault( v => v.LabServiceInfo == service );
            if( serviceVertex == null )
            {
                serviceVertex = CreateServiceVertex( service );
            }
            return serviceVertex;
        }

        YodiiGraphVertex FindOrCreateServiceVertex( IServiceInfo serviceInfo )
        {
            LabServiceInfo info = _serviceInfos.Where( s => s.ServiceInfo == serviceInfo ).First();

            return FindOrCreateServiceVertex( info );
        }

        YodiiGraphVertex CreatePluginVertex( LabPluginInfo livePlugin )
        {
            YodiiGraphVertex pluginVertex = new YodiiGraphVertex( this, livePlugin ) { ID = this.currentId++ };
            this.AddVertex( pluginVertex );
            livePlugin.PluginInfo.PropertyChanged += PluginInfo_PropertyChanged;

            if( livePlugin.PluginInfo.Service != null )
            {
                YodiiGraphVertex serviceVertex = FindOrCreateServiceVertex( livePlugin.PluginInfo.Service );

                YodiiGraphEdge serviceEdge = new YodiiGraphEdge( pluginVertex, serviceVertex, YodiiGraphEdgeType.Implementation ) { ID = currentId++ };
                this.AddEdge( serviceEdge );
            }

            foreach( MockServiceReferenceInfo reference in livePlugin.PluginInfo.InternalServiceReferences )
            {
                YodiiGraphVertex refVertex = FindOrCreateServiceVertex( reference.Reference );

                YodiiGraphEdge refEdge = new YodiiGraphEdge( pluginVertex, refVertex, reference ) { ID = currentId++ };
                this.AddEdge( refEdge );
            }

            livePlugin.PluginInfo.InternalServiceReferences.CollectionChanged += ObservableServiceReferences_CollectionChanged;

            return pluginVertex;
        }

        YodiiGraphVertex FindOrCreatePluginVertex( LabPluginInfo plugin )
        {
            YodiiGraphVertex pluginVertex = this.Vertices.FirstOrDefault( v => v.LabPluginInfo == plugin );
            if( plugin == null )
            {
                pluginVertex = CreatePluginVertex( plugin );
            }
            return pluginVertex;
        }

        YodiiGraphVertex FindOrCreatePluginVertex( IPluginInfo pluginInfo )
        {
            LabPluginInfo info = _pluginInfos.Where( p => p.PluginInfo == pluginInfo ).First();

            return FindOrCreatePluginVertex( info );
        }

        void RemovePluginVertex( LabPluginInfo plugin )
        {
            YodiiGraphVertex toRemove = Vertices.Where( v => v.LabPluginInfo == plugin ).FirstOrDefault();

            plugin.PluginInfo.InternalServiceReferences.CollectionChanged -= ObservableServiceReferences_CollectionChanged;
            plugin.PluginInfo.PropertyChanged -= PluginInfo_PropertyChanged;

            if( toRemove != null )
            {
                RemoveVertex( toRemove );
            }
        }

        void ClearPluginVertices()
        {
            foreach( var vtx in Vertices.Where( v => v.IsPlugin ) )
            {
                vtx.LabPluginInfo.PluginInfo.InternalServiceReferences.CollectionChanged -= ObservableServiceReferences_CollectionChanged;
                vtx.LabPluginInfo.PluginInfo.PropertyChanged -= PluginInfo_PropertyChanged;
            }
            this.RemoveVertexIf( v => v.IsPlugin );
        }

        void RemoveServiceVertex( LabServiceInfo service )
        {
            YodiiGraphVertex toRemove = Vertices.Where( v => v.LabServiceInfo == service ).FirstOrDefault();
            service.ServiceInfo.PropertyChanged -= ServiceInfo_PropertyChanged;

            if( toRemove != null )
            {
                RemoveVertex( toRemove );
            }
        }

        void ClearServiceVertices()
        {
            foreach( var vtx in Vertices.Where( v => v.IsService ) )
            {
                vtx.LabServiceInfo.ServiceInfo.PropertyChanged -= ServiceInfo_PropertyChanged;
            }
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
                    CreatePluginVertex( (LabPluginInfo)item );
                }
                RaiseGraphUpdateRequested();
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                foreach( var item in e.OldItems )
                {
                    RemovePluginVertex( (LabPluginInfo)item );
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
                    CreateServiceVertex( (LabServiceInfo)item );
                    RaiseGraphUpdateRequested();
                }
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                foreach( var item in e.OldItems )
                {
                    RemoveServiceVertex( (LabServiceInfo)e.OldItems[0] );
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

        void ObservableServiceReferences_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == NotifyCollectionChangedAction.Add )
            {
                MockServiceReferenceInfo reference = (MockServiceReferenceInfo)e.NewItems[0];
                YodiiGraphVertex pluginVertex = FindOrCreatePluginVertex( (PluginInfo)reference.Owner );
                YodiiGraphVertex refVertex = FindOrCreateServiceVertex( (IServiceInfo)reference.Reference );

                YodiiGraphEdge refEdge = new YodiiGraphEdge( pluginVertex, refVertex, reference ) { ID = currentId++ };
                this.AddEdge( refEdge );
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                IServiceReferenceInfo reference = (IServiceReferenceInfo)e.OldItems[0];
                Debug.Assert( reference != null );

                this.RemoveEdgeIf(
                    e2 => e2.IsServiceReference &&
                        e2.Source.IsPlugin && e2.Source.LabPluginInfo.PluginInfo == reference.Owner
                        && e2.Target.IsService && e2.Target.LabServiceInfo.ServiceInfo == reference.Reference );
            }
            RaiseGraphUpdateRequested();
        }

        void ServiceInfo_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            ServiceInfo service = sender as ServiceInfo;
            if( e.PropertyName == "Generalization" )
            {
                // Clear old generalization
                YodiiGraphVertex serviceVertex = Vertices.Where( x => x.IsService && x.LabServiceInfo.ServiceInfo == service ).First();
                YodiiGraphEdge oldEdge = Edges.Where( x => x.IsSpecialization && x.Source == serviceVertex ).FirstOrDefault();

                if( oldEdge != null ) RemoveEdge( oldEdge );

                // Create new generalization
                if( service.Generalization != null )
                {
                    YodiiGraphVertex newGeneralizationVertex = FindOrCreateServiceVertex( service.Generalization );
                    YodiiGraphEdge newEdge = new YodiiGraphEdge( serviceVertex, newGeneralizationVertex, YodiiGraphEdgeType.Specialization ) { ID = currentId++ };
                    AddEdge( newEdge );
                }

            }
            RaiseGraphUpdateRequested();
        }

        void PluginInfo_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            PluginInfo pluginInfo = sender as PluginInfo;

            if( e.PropertyName == "Service" )
            {
                // Clear old service edge
                YodiiGraphVertex pluginVertex = Vertices.Where( x => x.IsPlugin && x.LabPluginInfo.PluginInfo == pluginInfo ).First();
                YodiiGraphEdge oldEdge = Edges.Where( x => x.IsImplementation && x.Source == pluginVertex ).FirstOrDefault();

                if( oldEdge != null ) RemoveEdge( oldEdge );

                // Create new service edge
                if( pluginInfo.Service != null )
                {
                    YodiiGraphVertex newServiceVertex = FindOrCreateServiceVertex( pluginInfo.Service );
                    YodiiGraphEdge newEdge = new YodiiGraphEdge( pluginVertex, newServiceVertex, YodiiGraphEdgeType.Implementation ) { ID = currentId++ };
                    AddEdge( newEdge );
                }
            }
            RaiseGraphUpdateRequested();
        }
        #endregion Event handlers
    }
}
