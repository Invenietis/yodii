using System;
using System.Collections.Generic;
using Yodii.Model;
using Yodii.Engine;
using Yodii.Discoverer;
using Yodii.Host;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.ObjectModel;

namespace Yodii.DemoApp
{
    public sealed class DemoManager
    {
        readonly StandardDiscoverer _discoverer;
        readonly IAssemblyInfo _assemblyInfo;
        readonly IDiscoveredInfo _discoveredInfo;
        readonly PluginHost _host;
        readonly YodiiEngine _engine;
        public ObservableCollection<string> PluginNameList { get; private set; }//va falloir créer des ptit nobjets
        public ObservableCollection<string> ServiceNameList { get; private set; }

        public IDiscoveredInfo DiscoveredInfo { get { return _discoveredInfo; } }

        public DemoManager()
        {
            PluginNameList = new ObservableCollection<string>();
            ServiceNameList = new ObservableCollection<string>();

            _discoverer = new StandardDiscoverer();
            _assemblyInfo = _discoverer.ReadAssembly( Path.GetFullPath( "Yodii.DemoApp.exe" ) );
            _discoveredInfo = _discoverer.GetDiscoveredInfo();

            _host = new PluginHost();
            _engine = new YodiiEngine( _host );
        }

        public void Initialize()
        {
            for( int i=0; i < _discoveredInfo.PluginInfos.Count; i++ )
            {
                PluginNameList.Add( _discoveredInfo.PluginInfos[i].PluginFullName );
            }
            for( int i=0; i < _discoveredInfo.ServiceInfos.Count; i++ )
            {
                ServiceNameList.Add( _discoveredInfo.ServiceInfos[i].ServiceFullName );
            }
        }



        public bool Start()
        {
            _host.PluginCreator = PluginCreator2;
            _engine.SetDiscoveredInfo( _discoveredInfo );
            IConfigurationLayer cl = _engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.DemoApp.Client1", ConfigurationStatus.Running );
            cl.Items.Add( "Yodii.DemoApp.Company1", ConfigurationStatus.Running );
            _engine.Start();
            //engine.LiveInfo.FindPlugin( "Yodii.DemoApp.Client1" ).Start();
            //engine.LiveInfo.FindPlugin( "Yodii.DemoApp.Company1" ).Start();
            return true;
        }
        public bool Stop()
        {
            _engine.Stop();
            return true;
        }


        public IYodiiPlugin PluginCreator2( IPluginInfo pluginInfo, object[] ctorParameters )
        {
            var tPlugin = Assembly.Load( pluginInfo.AssemblyInfo.AssemblyName ).GetType( pluginInfo.PluginFullName, true );
            var ctor = tPlugin.GetConstructors().OrderBy( c => c.GetParameters().Length ).Last();

            //compte le nombre de IYodiiEngine
            //si Existe, créer nouveau tableau de bonne taille
            //mettre les autres param dans le tablau tout en insérant IYodiiEngine au bon endroit
            int a= (from y in ctor.GetParameters() where (y.ParameterType == typeof( IYodiiEngine )) select y).Count();
            if( a > 0 )
            {
                object[] newCtorParameters = new object[ctorParameters.Count() + a];
                List<int> indexList= new List<int>();
                for( int i=0; i < ctor.GetParameters().Count(); i++ )
                {
                    if( ctor.GetParameters()[i].ParameterType == typeof( IYodiiEngine ) )
                    {
                        indexList.Add( i );
                    }
                }
                int j=0;
                for( int i=0; i < newCtorParameters.Count(); i++ )
                {
                    if( j < indexList.Count && i == indexList[j] )
                    {
                        newCtorParameters[i] = _engine;
                        j++;
                    }
                    else
                    {
                        newCtorParameters[i] = ctorParameters[i + j];
                    }
                }
                ctorParameters = newCtorParameters;
            }
            return (IYodiiPlugin)ctor.Invoke( ctorParameters );
        }


        private void Generate()
        {

        }
        public void StartPlugin( string pluginName )
        {
            if( _engine.LiveInfo.FindPlugin( pluginName ) != null )
            {
                if( _engine.LiveInfo.FindPlugin( pluginName ).Capability.CanStart == true )
                    _engine.LiveInfo.FindPlugin( pluginName ).Start( "DemoManager", StartDependencyImpact.Minimal );
            }
        }
        public void StopPlugin( string pluginName )
        {
            if( _engine.LiveInfo.FindPlugin( pluginName ) != null )
            {
                if( _engine.LiveInfo.FindPlugin( pluginName ).Capability.CanStop == true )
                    _engine.LiveInfo.FindPlugin( pluginName ).Stop( "DemoManager" );
            }
        }

        internal void StartService( string serviceName )
        {
            if( _engine.LiveInfo.FindService( serviceName ) != null )
            {
                if( _engine.LiveInfo.FindService( serviceName ).Capability.CanStart == true )
                    _engine.LiveInfo.FindService( serviceName ).Start( "DemoManager", StartDependencyImpact.Minimal );
            }
        }

        internal void StopService( string serviceName )
        {
            if( _engine.LiveInfo.FindService( serviceName ) != null )
            {
                if( _engine.LiveInfo.FindService( serviceName ).Capability.CanStop == true )
                    _engine.LiveInfo.FindService( serviceName ).Stop( "DemoManager" );
            }
        }
        public void MainWindowClosing()
        {
            _engine.Stop();
        }
    }
}
