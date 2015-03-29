using System;
using System.Collections.Specialized;
using CK.Core;
using NullGuard;
using PropertyChanged;
using Yodii.Model;
using System.Linq;
using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class EngineViewModel : ViewModelBase
    {
        [AllowNull]
        public IYodiiEngineProxy Engine { get; private set; }

        public IObservableReadOnlyCollection<ServiceViewModel> Services { get { return _services; } }
        public IObservableReadOnlyCollection<PluginViewModel> Plugins { get { return _plugins; } }

        readonly CKObservableSortedArrayKeyList<ServiceViewModel, string> _services;
        readonly CKObservableSortedArrayKeyList<PluginViewModel, string> _plugins;

        [AllowNull]
        public PluginViewModel SelectedPlugin { get; set; }

        [AllowNull]
        public ServiceViewModel SelectedService { get; set; }

        [AllowNull]
        public YodiiItemViewModelBase SelectedItem { get; set; }

        bool _changingSelect;
        public void OnSelectedPluginChanged()
        {
            if( !_changingSelect )
            {
                _changingSelect = true;

                SelectedItem = SelectedPlugin;
                SelectedService = null;

                _changingSelect = false;
            }
        }
        public void OnSelectedServiceChanged()
        {
            if( !_changingSelect )
            {
                _changingSelect = true;

                SelectedItem = SelectedService;
                SelectedPlugin = null;

                _changingSelect = false;
            }
        }
        public void OnSelectedItemChanged()
        {
            if( !_changingSelect )
            {
                _changingSelect = true;

                if( SelectedItem is ServiceViewModel )
                {
                    SelectedService = (ServiceViewModel)SelectedItem;
                    SelectedPlugin = null;
                }
                else if( SelectedItem is PluginViewModel )
                {
                    SelectedPlugin = (PluginViewModel)SelectedItem;
                    SelectedService = null;
                }
                else
                {
                    SelectedPlugin = null;
                    SelectedService = null;
                }

                _changingSelect = false;
            }
        }

        public EngineViewModel()
        {
            _services = new CKObservableSortedArrayKeyList<ServiceViewModel, string>( x => x.Service.FullName, false );
            _plugins = new CKObservableSortedArrayKeyList<PluginViewModel, string>( x => x.Plugin.FullName, false );

        }

        public void LoadEngine( IYodiiEngineProxy engine )
        {
            if( Engine != null ) { throw new InvalidOperationException( "Cannot load an Engine twice." ); }
            Engine = engine;

            _services.Clear();
            _plugins.Clear();

            BindCollectionChanges();
            CreatePluginsFromEngine();
            CreateServicesFromEngine();
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
                        FindOrCreatePlugin( p );
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach( ILivePluginInfo p in e.OldItems.Cast<ILivePluginInfo>() )
                    {
                        Remove( p );
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
                    foreach( ILiveServiceInfo s in e.NewItems.Cast<ILiveServiceInfo>() )
                    {
                        FindOrCreateService( s );
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach( ILiveServiceInfo s in e.OldItems.Cast<ILiveServiceInfo>() )
                    {
                        Remove( s );
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

        void CreatePluginsFromEngine()
        {
            if( Engine != null )
            {
                foreach( ILivePluginInfo livePlugin in Engine.LiveInfo.Plugins )
                {
                    FindOrCreatePlugin( livePlugin );
                }
            }
        }

        void CreateServicesFromEngine()
        {
            if( Engine != null )
            {
                foreach( ILiveServiceInfo liveService in Engine.LiveInfo.Services )
                {
                    FindOrCreateService( liveService );
                }
            }
        }

        PluginViewModel CreateAndAddPluginViewModel( ILivePluginInfo livePlugin )
        {
            ServiceViewModel parentService = FindOrCreateService( livePlugin.Service );

            PluginViewModel vm = new PluginViewModel();
            vm.LoadLiveItem( this, livePlugin );

            vm.Service = parentService;
            if( parentService != null ) parentService.Children.Add( vm );

            _plugins.Add( vm );
            return vm;
        }

        ServiceViewModel CreateAndAddServiceViewModel( ILiveServiceInfo liveService )
        {
            ServiceViewModel parentGeneralization = FindOrCreateService( liveService.Generalization );

            ServiceViewModel vm = new ServiceViewModel();
            vm.LoadLiveItem( this, liveService );

            vm.Generalization = parentGeneralization;
            if( parentGeneralization != null ) parentGeneralization.Children.Add( vm );

            _services.Add( vm );
            return vm;
        }

        void Remove( ILiveServiceInfo s )
        {
            var vm = FindService( s.FullName );
            if( vm != null )
            {
                if( vm.Generalization != null )
                {
                    vm.Generalization.Children.Remove( vm );
                }

                _services.Remove( s.FullName );
            }
        }

        void Remove( ILivePluginInfo p )
        {
            var vm = FindPlugin( p.FullName );
            if( vm != null )
            {
                if( vm.Service != null )
                {
                    vm.Service.Children.Remove( vm );
                }

                _plugins.Remove( p.FullName );
            }
        }

        [return: AllowNull]
        public ServiceViewModel FindService( string serviceFullName )
        {
            return Services.SingleOrDefault( x => x.FullName == serviceFullName );
        }

        ServiceViewModel FindOrCreateService( ILiveServiceInfo serviceInfo )
        {
            ServiceViewModel s = null;
            if( serviceInfo != null )
            {
                s = FindService( serviceInfo.FullName );

                if( s == null ) s = CreateAndAddServiceViewModel( serviceInfo );
            }
            return s;
        }

        [return: AllowNull]
        public PluginViewModel FindPlugin( string pluginFullName )
        {
            return Plugins.SingleOrDefault( x => x.FullName == pluginFullName );
        }

        PluginViewModel FindOrCreatePlugin( ILivePluginInfo pluginInfo )
        {
            PluginViewModel p = null;
            if( pluginInfo != null )
            {
                p = FindPlugin( pluginInfo.FullName );

                if( p == null ) p = CreateAndAddPluginViewModel( pluginInfo );
            }
            return p;
        }


    }
}