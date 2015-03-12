using System;
using System.Collections.Specialized;
using CK.Core;
using NullGuard;
using PropertyChanged;
using Yodii.Model;
using System.Linq;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class EngineViewModel : EmptyPropertyChangedHandler
    {
        [AllowNull]
        public IYodiiEngineBase Engine { get; private set; }

        public IObservableReadOnlyCollection<ServiceViewModel> Services { get { return _services; } }
        public IObservableReadOnlyCollection<PluginViewModel> Plugins { get { return _plugins; } }

        readonly CKObservableSortedArrayKeyList<ServiceViewModel, string> _services;
        readonly CKObservableSortedArrayKeyList<PluginViewModel, string> _plugins;

        public EngineViewModel()
        {
            _services = new CKObservableSortedArrayKeyList<ServiceViewModel, string>( x => x.Service.FullName, false );
            _plugins = new CKObservableSortedArrayKeyList<PluginViewModel, string>( x => x.Plugin.FullName, false );
        }

        public void LoadEngine( IYodiiEngineBase engine )
        {
            if( Engine != null ) { throw new InvalidOperationException( "Cannot load an Engine twice." ); }
            Engine = engine;

            _services.Clear();
            _plugins.Clear();

            BindCollectionChanges();
            ResetPluginsFromEngine();
            ResetServicesFromEngine();
        }

        void BindCollectionChanges()
        {
            Engine.LiveInfo.Services.CollectionChanged += Services_CollectionChanged;
            Engine.LiveInfo.Plugins.CollectionChanged += Plugins_CollectionChanged;
        }

        void Plugins_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case NotifyCollectionChangedAction.Add:
                    foreach( ILivePluginInfo p in e.NewItems.Cast<ILivePluginInfo>() )
                    {
                        CreateAndAddPluginViewModel( p );
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach( ILivePluginInfo p in e.OldItems.Cast<ILivePluginInfo>() )
                    {
                        _plugins.Remove( p.FullName );
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _plugins.Clear();
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }

        void Services_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case NotifyCollectionChangedAction.Add:
                    foreach( ILiveServiceInfo s in e.NewItems.Cast<ILiveServiceInfo>())
                    {
                        CreateAndAddServiceViewModel( s );
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach( ILiveServiceInfo s in e.OldItems.Cast<ILiveServiceInfo>() )
                    {
                        _services.Remove( s.FullName );
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _services.Clear();
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }

        void ResetPluginsFromEngine()
        {
            _plugins.Clear();

            if( Engine != null )
            {
                foreach( ILivePluginInfo livePlugin in Engine.LiveInfo.Plugins )
                {
                    CreateAndAddPluginViewModel( livePlugin );
                }
            }
        }

        void ResetServicesFromEngine()
        {
            _services.Clear();

            if( Engine != null )
            {
                foreach( ILiveServiceInfo liveService in Engine.LiveInfo.Services )
                {
                    CreateAndAddServiceViewModel( liveService );
                }
            }
        }

        void CreateAndAddPluginViewModel( ILivePluginInfo livePlugin )
        {
            PluginViewModel vm = new PluginViewModel();
            vm.LoadLivePlugin( livePlugin );
            _plugins.Add( vm );
        }

        void CreateAndAddServiceViewModel( ILiveServiceInfo liveService )
        {
            ServiceViewModel vm = new ServiceViewModel();
            vm.LoadLiveService( liveService );
            _services.Add( vm );
        }
    }
}