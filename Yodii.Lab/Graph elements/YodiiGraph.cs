using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using CK.Core;
using System.Diagnostics;
using System.Collections.Specialized;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Yodii graph. Handles binding and converting from ServiceInfo/PluginInfo collections.
    /// </summary>
    public class YodiiGraph : BidirectionalGraph<YodiiGraphVertex, YodiiGraphEdge>
    {
        readonly ICKObservableReadOnlyCollection<LiveServiceInfo> _serviceInfos;
        readonly ICKObservableReadOnlyCollection<LivePluginInfo> _pluginInfos;

        #region Constructor
        internal YodiiGraph( ICKObservableReadOnlyCollection<LiveServiceInfo> serviceInfos, ICKObservableReadOnlyCollection<LivePluginInfo> pluginInfos )
            : base()
        {
            Debug.Assert( serviceInfos != null );
            Debug.Assert( pluginInfos != null );

            _serviceInfos = serviceInfos;
            _pluginInfos = pluginInfos;

            _serviceInfos.CollectionChanged += _serviceInfos_CollectionChanged;
            _pluginInfos.CollectionChanged += _pluginInfos_CollectionChanged;
        }
        #endregion Constructor

        #region Private methods
        YodiiGraphVertex CreateServiceVertex( LiveServiceInfo liveService )
        {
            YodiiGraphVertex serviceVertex = new YodiiGraphVertex( liveService );
            this.AddVertex( serviceVertex );
            liveService.ServiceInfo.PropertyChanged += ServiceInfo_PropertyChanged;

            if( liveService.Generalization != null )
            {
                YodiiGraphVertex generalizationVertex = FindOrCreateServiceVertex( liveService.Generalization );

                YodiiGraphEdge edge = new YodiiGraphEdge( serviceVertex, generalizationVertex, YodiiGraphEdgeType.Specialization );

                this.AddEdge( edge );
            }


            return serviceVertex;
        }

        YodiiGraphVertex FindOrCreateServiceVertex( LiveServiceInfo service )
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
            LiveServiceInfo info = _serviceInfos.Where( s => s.ServiceInfo == serviceInfo ).First();

            return FindOrCreateServiceVertex( info );
        }

        YodiiGraphVertex CreatePluginVertex( LivePluginInfo livePlugin )
        {
            YodiiGraphVertex pluginVertex = new YodiiGraphVertex( livePlugin );
            this.AddVertex( pluginVertex );
            livePlugin.PluginInfo.PropertyChanged += PluginInfo_PropertyChanged;

            if( livePlugin.PluginInfo.Service != null )
            {
                YodiiGraphVertex serviceVertex = FindOrCreateServiceVertex( livePlugin.Service );

                YodiiGraphEdge serviceEdge = new YodiiGraphEdge( pluginVertex, serviceVertex, YodiiGraphEdgeType.Implementation );
                this.AddEdge( serviceEdge );
            }

            foreach( IServiceReferenceInfo reference in livePlugin.PluginInfo.ServiceReferences )
            {
                YodiiGraphVertex refVertex = FindOrCreateServiceVertex( reference.Reference );

                YodiiGraphEdge refEdge = new YodiiGraphEdge( pluginVertex, refVertex, reference.Requirement );
                this.AddEdge( refEdge );
            }

            livePlugin.PluginInfo.InternalServiceReferences.CollectionChanged += ObservableServiceReferences_CollectionChanged;

            return pluginVertex;
        }

        YodiiGraphVertex FindOrCreatePluginVertex( LivePluginInfo plugin )
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
            LivePluginInfo info = _pluginInfos.Where( p => p.PluginInfo == pluginInfo ).First();

            return FindOrCreatePluginVertex( info );
        }

        void RemovePluginVertex( LivePluginInfo plugin )
        {
            YodiiGraphVertex toRemove = Vertices.Where( v => v.LivePluginInfo == plugin ).FirstOrDefault();

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
                vtx.LivePluginInfo.PluginInfo.InternalServiceReferences.CollectionChanged -= ObservableServiceReferences_CollectionChanged;
                vtx.LivePluginInfo.PluginInfo.PropertyChanged -= PluginInfo_PropertyChanged;
            }
            this.RemoveVertexIf( v => v.IsPlugin );
        }

        void RemoveServiceVertex( LiveServiceInfo service )
        {
            YodiiGraphVertex toRemove = Vertices.Where( v => v.LiveServiceInfo == service ).FirstOrDefault();
            service.ServiceInfo.PropertyChanged -= ServiceInfo_PropertyChanged;

            if( toRemove != null )
            {
                RemoveVertex( toRemove );
            }
        }

        void ClearServiceVertices()
        {
            foreach(var vtx in Vertices.Where( v => v.IsService))
            {
                vtx.LiveServiceInfo.ServiceInfo.PropertyChanged -= ServiceInfo_PropertyChanged;
            }
            this.RemoveVertexIf( v => v.IsService );
        }
        #endregion Private methods

        #region Event handlers
        void _pluginInfos_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == NotifyCollectionChangedAction.Add )
            {
                CreatePluginVertex( (LivePluginInfo)e.NewItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                RemovePluginVertex( (LivePluginInfo)e.OldItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Replace )
            {
                RemovePluginVertex( (LivePluginInfo)e.OldItems[0] );
                CreatePluginVertex( (LivePluginInfo)e.NewItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Reset )
            {
                ClearPluginVertices();
            }
        }

        void _serviceInfos_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == NotifyCollectionChangedAction.Add )
            {
                CreateServiceVertex( (LiveServiceInfo)e.NewItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                RemoveServiceVertex( (LiveServiceInfo)e.OldItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Replace )
            {
                RemoveServiceVertex( (LiveServiceInfo)e.OldItems[0] );
                CreateServiceVertex( (LiveServiceInfo)e.NewItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Reset )
            {
                ClearServiceVertices();
            }
        }

        void ObservableServiceReferences_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == NotifyCollectionChangedAction.Add )
            {
                IServiceReferenceInfo reference = (IServiceReferenceInfo)e.NewItems[e.NewStartingIndex];
                YodiiGraphVertex pluginVertex = FindOrCreatePluginVertex( (PluginInfo)reference.Owner );
                YodiiGraphVertex refVertex = FindOrCreateServiceVertex( (IServiceInfo)reference.Reference );

                YodiiGraphEdge refEdge = new YodiiGraphEdge( pluginVertex, refVertex, reference.Requirement );
                this.AddEdge( refEdge );
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                IServiceReferenceInfo reference = (IServiceReferenceInfo)e.OldItems[e.OldStartingIndex];

                this.RemoveEdgeIf( e2 => e2.Source.LivePluginInfo == reference.Owner && e2.Target.LiveServiceInfo == reference.Reference && e2.ReferenceRequirement == reference.Requirement );
            }
        }

        void ServiceInfo_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            ServiceInfo service = sender as ServiceInfo;
            if( e.PropertyName == "Generalization" )
            {
                // Clear old generalization
                YodiiGraphVertex serviceVertex = Vertices.Where( x => x.IsService && x.LiveServiceInfo.ServiceInfo == service ).First();
                YodiiGraphEdge oldEdge = Edges.Where( x => x.IsSpecialization && x.Source == serviceVertex ).FirstOrDefault();

                if( oldEdge != null ) RemoveEdge( oldEdge );

                // Create new generalization
                if( service.Generalization != null )
                {
                    YodiiGraphVertex newGeneralizationVertex = FindOrCreateServiceVertex( service.Generalization );
                    YodiiGraphEdge newEdge = new YodiiGraphEdge( serviceVertex, newGeneralizationVertex, YodiiGraphEdgeType.Specialization );
                    AddEdge( newEdge );
                }
            }
        }

        void PluginInfo_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            PluginInfo pluginInfo = sender as PluginInfo;

            if( e.PropertyName == "Service" )
            {
                // Clear old service edge
                YodiiGraphVertex pluginVertex = Vertices.Where( x => x.IsPlugin && x.LivePluginInfo.PluginInfo == pluginInfo ).First();
                YodiiGraphEdge oldEdge = Edges.Where( x => x.IsImplementation && x.Source == pluginVertex ).FirstOrDefault();

                if( oldEdge != null ) RemoveEdge( oldEdge );

                // Create new service edge
                if( pluginInfo.Service != null )
                {
                    YodiiGraphVertex newServiceVertex = FindOrCreateServiceVertex( pluginInfo.Service );
                    YodiiGraphEdge newEdge = new YodiiGraphEdge( pluginVertex, newServiceVertex, YodiiGraphEdgeType.Implementation );
                    AddEdge( newEdge );
                }
            }
        }
        #endregion Event handlers
    }
}
