using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using CK.Core;
using Yodii.Model.CoreModel;
using System.Diagnostics;
using System.Collections.Specialized;
using Yodii.Lab.Mocks;

namespace Yodii.Lab
{
    public class YodiiGraph : BidirectionalGraph<YodiiGraphVertex, YodiiGraphEdge>
    {
        readonly ICKObservableReadOnlyCollection<IServiceInfo> _serviceInfos;
        readonly ICKObservableReadOnlyCollection<IPluginInfo> _pluginInfos;

        internal YodiiGraph( ICKObservableReadOnlyCollection<IServiceInfo> serviceInfos, ICKObservableReadOnlyCollection<IPluginInfo> pluginInfos )
            : base()
        {
            Debug.Assert( serviceInfos != null );
            Debug.Assert( pluginInfos != null );

            _serviceInfos = serviceInfos;
            _pluginInfos = pluginInfos;

            _serviceInfos.CollectionChanged += _serviceInfos_CollectionChanged;
            _pluginInfos.CollectionChanged += _pluginInfos_CollectionChanged;
        }

        private YodiiGraphVertex FindOrCreateServiceVertex( IServiceInfo service )
        {
            YodiiGraphVertex serviceVertex = this.Vertices.FirstOrDefault( v => v.ServiceInfo == service );
            if( serviceVertex == null )
            {
                serviceVertex = CreateServiceVertex( service );
            }
            return serviceVertex;
        }

        private YodiiGraphVertex FindOrCreatePluginVertex( PluginInfo plugin )
        {
            YodiiGraphVertex pluginVertex = this.Vertices.FirstOrDefault( v => v.PluginInfo == plugin );
            if( plugin == null )
            {
                pluginVertex = CreatePluginVertex( plugin );
            }
            return pluginVertex;
        }

        private YodiiGraphVertex CreatePluginVertex( PluginInfo plugin )
        {
            YodiiGraphVertex pluginVertex = new YodiiGraphVertex( plugin );
            this.AddVertex( pluginVertex );

            if( plugin.Service != null )
            {
                YodiiGraphVertex serviceVertex = FindOrCreateServiceVertex( plugin.Service );

                YodiiGraphEdge serviceEdge = new YodiiGraphEdge( pluginVertex, serviceVertex, YodiiGraphEdgeType.Implementation );
                this.AddEdge( serviceEdge );
            }

            foreach( IServiceReferenceInfo reference in plugin.ServiceReferences )
            {
                YodiiGraphVertex refVertex = FindOrCreateServiceVertex( reference.Reference );

                YodiiGraphEdge refEdge = new YodiiGraphEdge( pluginVertex, refVertex, reference.Requirement );
                this.AddEdge( refEdge );
            }

            plugin.ObservableServiceReferences.CollectionChanged += ObservableServiceReferences_CollectionChanged;

            return pluginVertex;
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

                this.RemoveEdgeIf( e2 => e2.Source.PluginInfo == reference.Owner && e2.Target.ServiceInfo == reference.Reference && e2.ReferenceRequirement == reference.Requirement );
            }
        }

        private void RemovePluginVertex( PluginInfo plugin )
        {
            YodiiGraphVertex toRemove = Vertices.Where( v => v.PluginInfo == plugin ).FirstOrDefault();

            plugin.ObservableServiceReferences.CollectionChanged -= ObservableServiceReferences_CollectionChanged;

            if( toRemove != null )
            {
                RemoveVertex( toRemove );
            }
        }

        private void ClearPluginVertices()
        {
            this.RemoveVertexIf( v => v.IsPlugin );
        }

        private YodiiGraphVertex CreateServiceVertex( IServiceInfo service )
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

        private void RemoveServiceVertex( IServiceInfo service )
        {
            YodiiGraphVertex toRemove = Vertices.Where( v => v.ServiceInfo == service ).FirstOrDefault();
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
                CreatePluginVertex( (PluginInfo)e.NewItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                RemovePluginVertex( (PluginInfo)e.OldItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Replace )
            {
                RemovePluginVertex( (PluginInfo)e.OldItems[0] );
                CreatePluginVertex( (PluginInfo)e.NewItems[0] );
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
                CreateServiceVertex( (IServiceInfo)e.NewItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Remove )
            {
                RemoveServiceVertex( (IServiceInfo)e.OldItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Replace )
            {
                RemoveServiceVertex( (IServiceInfo)e.OldItems[0] );
                CreateServiceVertex( (IServiceInfo)e.NewItems[0] );
            }
            else if( e.Action == NotifyCollectionChangedAction.Reset )
            {
                ClearServiceVertices();
            }
        }
    }
}
