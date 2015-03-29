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
                    foreach( ILiveServiceInfo s in e.NewItems.Cast<ILiveServiceInfo>() )
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
            vm.LoadLiveItem( this, livePlugin );
            _plugins.Add( vm );
        }

        void CreateAndAddServiceViewModel( ILiveServiceInfo liveService )
        {
            ServiceViewModel vm = new ServiceViewModel();
            vm.LoadLiveItem( this, liveService );
            _services.Add( vm );
        }

        public ServiceViewModel FindService( string serviceFullName )
        {
            return Services.SingleOrDefault( x => x.FullName == serviceFullName );
        }

        public PluginViewModel FindPlugin( string pluginFullName )
        {
            return Plugins.SingleOrDefault( x => x.FullName == pluginFullName );
        }
    }
}