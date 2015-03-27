using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class YodiiItemViewModelBase : EmptyPropertyChangedHandler
    {
        [AllowNull]
        public string DisplayName { get; private set; }

        [AllowNull]
        public string Description { get; private set; }

        [AllowNull]
        public ILiveYodiiItem LiveItem { get; private set; }

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

        }

        public void LoadLiveItem( ILiveYodiiItem item )
        {
            if( LiveItem != null ) { throw new InvalidOperationException( "Cannot load an item twice." ); }
            LiveItem = item;
            LoadTypeData();
        }

        protected abstract Assembly GetItemAssembly();

        void LoadTypeData()
        {
            Assembly pluginAssembly = GetItemAssembly();

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