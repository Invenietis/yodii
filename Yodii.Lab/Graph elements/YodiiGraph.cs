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
    public class YodiiGraph : BidirectionalGraph<YodiiGraphVertex, YodiiGraphEdge>
    {
        readonly ICKObservableReadOnlyCollection<LiveServiceInfo> _serviceInfos;
        readonly ICKObservableReadOnlyCollection<LivePluginInfo> _pluginInfos;

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

        private YodiiGraphVertex FindOrCreateServiceVertex( LiveServiceInfo service )
        {
            YodiiGraphVertex serviceVertex = this.Vertices.FirstOrDefault( v => v.LiveServiceInfo == service );
            if( serviceVertex == null )
            {
                serviceVertex = CreateServiceVertex( service );
            }
            return serviceVertex;
        }

        private YodiiGraphVertex FindOrCreatePluginVertex( LivePluginInfo plugin )
        {
            YodiiGraphVertex pluginVertex = this.Vertices.FirstOrDefault( v => v.LivePluginInfo == plugin );
            if( plugin == null )
            {
                pluginVertex = CreatePluginVertex( plugin );
            }
            return pluginVertex;
        }

        private YodiiGraphVertex CreatePluginVertex( LivePluginInfo plugin )
        {
            YodiiGraphVertex pluginVertex = new YodiiGraphVertex( plugin );
            this.AddVertex( pluginVertex );

            if( plugin.PluginInfo.Service != null )
            {
                YodiiGraphVertex serviceVertex = FindOrCreateServiceVertex( plugin.Service );

                YodiiGraphEdge serviceEdge = new YodiiGraphEdge( pluginVertex, serviceVertex, YodiiGraphEdgeType.Implementation );
                this.AddEdge( serviceEdge );
            }

            foreach( IServiceReferenceInfo reference in plugin.PluginInfo.ServiceReferences )
            {
                YodiiGraphVertex refVertex = FindOrCreateServiceVertex( reference.Reference );

                YodiiGraphEdge refEdge = new YodiiGraphEdge( pluginVertex, refVertex, reference.Requirement );
                this.AddEdge( refEdge );
            }

            plugin.PluginInfo.InternalServiceReferences.CollectionChanged += ObservableServiceReferences_CollectionChanged;

            return pluginVertex;
        }

        private YodiiGraphVertex FindOrCreateServiceVertex( IServiceInfo serviceInfo )
        {
            LiveServiceInfo info = _serviceInfos.Where( s => s.ServiceInfo == serviceInfo ).FirstOrDefault();
            Debug.Assert( info != null );

            return FindOrCreateServiceVertex( info );
        }

        private YodiiGraphVertex FindOrCreatePluginVertex( IPluginInfo pluginInfo )
        {
            LivePluginInfo info = _pluginInfos.Where( p => p.PluginInfo == pluginInfo ).FirstOrDefault();
            Debug.Assert( info != null );

            return FindOrCreatePluginVertex( info );
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

        private void RemovePluginVertex( LivePluginInfo plugin )
        {
            YodiiGraphVertex toRemove = Vertices.Where( v => v.LivePluginInfo == plugin ).FirstOrDefault();

            plugin.PluginInfo.InternalServiceReferences.CollectionChanged -= ObservableServiceReferences_CollectionChanged;

            if( toRemove != null )
            {
                RemoveVertex( toRemove );
            }
        }

        private void ClearPluginVertices()
        {
            this.RemoveVertexIf( v => v.IsPlugin );
        }

        private YodiiGraphVertex CreateServiceVertex( LiveServiceInfo service )
        {
            YodiiGraphVertex serviceVertex = new YodiiGraphVertex( service );
            this.AddVertex( serviceVertex );

            if( service.Generalization != null )
            {
                YodiiGraphVertex generalizationVertex = FindOrCreateServiceVertex( service.Generalization );
                
                YodiiGraphEdge edge = new YodiiGraphEdge( serviceVertex, generalizationVertex, YodiiGraphEdgeType.Specialization );

                this.AddEdge( edge );
            }

            return serviceVertex;
        }

        private void RemoveServiceVertex( LiveServiceInfo service )
        {
            YodiiGraphVertex toRemove = Vertices.Where( v => v.LiveServiceInfo == service ).FirstOrDefault();
            if( toRemove != null )
            {
                RemoveVertex( toRemove );
            }
        }

        private void ClearServiceVertices()
        {
            this.RemoveVertexIf( v => v.IsService );
        }

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
    }
}
