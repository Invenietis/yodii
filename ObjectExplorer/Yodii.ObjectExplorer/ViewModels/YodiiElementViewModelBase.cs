using System;
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

        IYodiiEngineProxy _parentEngine;

        public ICommand StartItemCommand { get; private set; }
        public ICommand StopItemCommand { get; private set; }

        public string FullName
        {
            get
            {
                if( LiveItem != null ) return LiveItem.FullName;
                else return null;
            }
        }

        public YodiiItemViewModelBase()
        {
            StartItemCommand = new RelayCommand( () =>
            {
                if( _parentEngine != null )
                {
                    _parentEngine.StartItem( LiveItem );
                }
            }, () =>
            {
                return LiveItem != null && LiveItem.Capability.CanStart;
            } );

            StopItemCommand = new RelayCommand( () =>
            {
                if( _parentEngine != null )
                {
                    _parentEngine.StopItem( LiveItem );
                }
            }, () =>
            {
                return LiveItem != null && LiveItem.Capability.CanStop;
            } );
        }

        public void LoadLiveItem( IYodiiEngineProxy parentEngine, ILiveYodiiItem item )
        {
            if( parentEngine == null ) throw new ArgumentNullException( "parentEngine" );
            if( item == null ) throw new ArgumentNullException( "item" );

            if( LiveItem != null ) { throw new InvalidOperationException( "Cannot load an item twice." ); }

            _parentEngine = parentEngine;
            LiveItem = item;

            IsPlugin = item is ILivePluginInfo;
            IsService = item is ILiveServiceInfo;

            LoadTypeData();
        }

        public abstract IAssemblyInfo AssemblyInfo { get; }

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