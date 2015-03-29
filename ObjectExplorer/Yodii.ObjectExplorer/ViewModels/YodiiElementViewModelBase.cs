using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class YodiiItemViewModelBase : ViewModelBase
    {
        [AllowNull]
        public string DisplayName { get; private set; }

        [AllowNull]
        public string Description { get; private set; }

        [AllowNull]
        public ILiveYodiiItem LiveItem { get; private set; }

        public bool IsPlugin { get; private set; }

        public bool IsService { get; private set; }

        public bool IsSelected
        {
            get
            {
                if( _parentEngine != null )
                {
                    return _parentEngine.SelectedItem == this;
                }
                return false;
            }
            set
            {
                if( _parentEngine != null )
                {
                    if( value == true )
                    {
                        _parentEngine.SelectedItem = this;
                    }
                    else if( _parentEngine.SelectedItem == this )
                    {
                        _parentEngine.SelectedItem = null;
                    }
                }
            }
        }

        EngineViewModel _parentEngine;

        protected EngineViewModel EngineViewModel { get { return _parentEngine; } }

        public ICommand StartItemCommand { get; private set; }
        public ICommand StopItemCommand { get; private set; }
        public ICommand SelectItemCommand { get; private set; }

        public string FullName
        {
            get
            {
                if( LiveItem != null ) return LiveItem.FullName;
                else return null;
            }
        }

        [AllowNull]
        public ServiceViewModel AsService
        {
            get
            {
                return this as ServiceViewModel;
            }
        }

        [AllowNull]
        public PluginViewModel AsPlugin
        {
            get
            {
                return this as PluginViewModel;
            }
        }

        public YodiiItemViewModelBase()
        {
            StartItemCommand = new RelayCommand( () =>
            {
                if( _parentEngine != null )
                {
                    _parentEngine.Engine.StartItem( LiveItem );
                }
            }, () =>
            {
                return LiveItem != null && LiveItem.Capability.CanStart;
            } );

            StopItemCommand = new RelayCommand( () =>
            {
                if( _parentEngine != null )
                {
                    _parentEngine.Engine.StopItem( LiveItem );
                }
            }, () =>
            {
                return LiveItem != null && LiveItem.Capability.CanStop;
            } );
        }

        public void LoadLiveItem( EngineViewModel parentEngine, ILiveYodiiItem item )
        {
            if( parentEngine == null ) throw new ArgumentNullException( "parentEngine" );
            if( item == null ) throw new ArgumentNullException( "item" );

            if( LiveItem != null ) { throw new InvalidOperationException( "Cannot load an item twice." ); }

            _parentEngine = parentEngine;
            LiveItem = item;

            IsPlugin = item is ILivePluginInfo;
            IsService = item is ILiveServiceInfo;

            LoadTypeData();

            _parentEngine.PropertyChanged += _parentEngine_PropertyChanged;

            SelectItemCommand = _parentEngine.SelectItemCommand;

            OnLoaded();
        }

        void _parentEngine_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "SelectedItem" )
            {
                RaisePropertyChanged( "IsSelected" );
            }
        }

        public abstract IAssemblyInfo AssemblyInfo { get; }

        public abstract YodiiItemViewModelBase Parent { get; }

        public abstract ObservableCollection<YodiiItemViewModelBase> Children { get; }

        protected virtual void OnLoaded()
        {
        }

        void LoadTypeData()
        {
            Assembly pluginAssembly = Assembly.Load( AssemblyInfo.AssemblyName );

            Type pluginType = pluginAssembly.GetType( LiveItem.FullName, true );

            Debug.Assert( pluginType != null, "Item Type must exist, since it sits in current AppDomain with loaded assembly" );

            Attribute a = Attribute.GetCustomAttributes( pluginType )
                .Where( attr => attr is YodiiItemBaseAttribute )
                .SingleOrDefault();

            if( a != null )
            {
                YodiiItemBaseAttribute da = (YodiiItemBaseAttribute)a;

                if( !String.IsNullOrWhiteSpace( da.DisplayName ) )
                {
                    DisplayName = da.DisplayName;
                }
                else
                {
                    DisplayName = pluginType.Name;
                }

                if( !String.IsNullOrWhiteSpace( da.Description ) )
                {
                    Description = da.Description;
                }
                else
                {
                    Description = String.Empty;
                }
            }
            else
            {
                DisplayName = pluginType.Name;
                Description = String.Empty;
            }
        }
    }
}