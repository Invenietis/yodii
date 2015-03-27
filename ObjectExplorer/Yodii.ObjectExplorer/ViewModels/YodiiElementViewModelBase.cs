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
    public abstract class YodiiItemViewModelBase<T> : EmptyPropertyChangedHandler
        where T : ILiveYodiiItem
    {
        [AllowNull]
        public string DisplayName { get; private set; }

        [AllowNull]
        public string Description { get; private set; }

        [AllowNull]
        public T YodiiItem { get; private set; }

        public string FullName
        {
            get
            {
                if( YodiiItem != null ) return YodiiItem.FullName;
                else return null;
            }
        }

        public YodiiItemViewModelBase()
        {

        }

        public void LoadLiveItem( T item )
        {
            if( YodiiItem != null ) { throw new InvalidOperationException( "Cannot load an item twice." ); }
            YodiiItem = item;
            LoadTypeData();
        }

        protected abstract Assembly GetItemAssembly();

        void LoadTypeData()
        {
            Assembly pluginAssembly = GetItemAssembly();

            Type pluginType = pluginAssembly.GetType( YodiiItem.FullName, true );

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