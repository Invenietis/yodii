using System;
using System.Diagnostics;
using System.Reflection;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class ServiceViewModel : EmptyPropertyChangedHandler
    {
        [AllowNull]
        public ILiveServiceInfo Service { get; private set; }

        [AllowNull]
        public string DisplayName { get; private set; }

        [AllowNull]
        public string Description { get; private set; }

        public string FullName
        {
            get
            {
                if( Service != null ) return Service.FullName;
                else return null;
            }
        }

        public ServiceViewModel()
        {

        }

        public void LoadLiveService( ILiveServiceInfo service )
        {
            if( Service != null ) { throw new InvalidOperationException( "Cannot load a Service twice." ); }
            Service = service;

            LoadTypeData();
        }

        void LoadTypeData()
        {
            Assembly serviceAssembly = Assembly.Load( Service.ServiceInfo.AssemblyInfo.AssemblyName );
            Type serviceInterfaceType = serviceAssembly.GetType( Service.FullName, true );
            Debug.Assert( serviceInterfaceType != null, "Service Type must exist, since it sits in current AppDomain with loaded assembly" );

            Attribute a = Attribute.GetCustomAttribute( serviceInterfaceType, typeof( YodiiServiceAttribute ) );
            if( a != null )
            {
                YodiiServiceAttribute da = (YodiiServiceAttribute)a;

                if( !String.IsNullOrWhiteSpace( da.DisplayName ) )
                {
                    DisplayName = da.DisplayName;
                }
                else
                {
                    DisplayName = serviceInterfaceType.Name;
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
                DisplayName = serviceInterfaceType.Name;
                Description = String.Empty;
            }
        }
    }
}