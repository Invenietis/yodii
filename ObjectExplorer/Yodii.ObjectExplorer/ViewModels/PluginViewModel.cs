using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class PluginViewModel : EmptyPropertyChangedHandler
    {
        [AllowNull]
        public ILivePluginInfo Plugin { get; private set; }

        [AllowNull]
        public string DisplayName { get; private set; }

        [AllowNull]
        public string Description { get; private set; }

        public string FullName
        {
            get
            {
                if( Plugin != null ) return Plugin.FullName;
                else return null;
            }
        }

        public PluginViewModel()
        {

        }

        public void LoadLivePlugin( ILivePluginInfo plugin )
        {
            if( Plugin != null ) { throw new InvalidOperationException( "Cannot load a Plugin twice." ); }
            Plugin = plugin;
            LoadTypeData();
        }

        void LoadTypeData()
        {
            Assembly pluginAssembly = Assembly.Load( Plugin.PluginInfo.AssemblyInfo.AssemblyName );
            Type pluginType = pluginAssembly.GetType( Plugin.FullName, true );
            Debug.Assert( pluginType != null, "Plugin Type must exist, since it sits in current AppDomain with loaded assembly" );

            Attribute a = Attribute.GetCustomAttribute( pluginType, typeof( YodiiPluginAttribute ) );
            if( a != null )
            {
                YodiiPluginAttribute da = (YodiiPluginAttribute)a;

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