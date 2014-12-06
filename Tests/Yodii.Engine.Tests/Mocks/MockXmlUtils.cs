using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Xml;
using CK.Core;
using NUnit.Framework;
using Yodii.Engine.Tests.ConfigurationSolverTests;
using Yodii.Engine.Tests.Properties;
using Yodii.Model;

namespace Yodii.Engine.Tests.Mocks
{
    class MockXmlUtils
    {
        public static YodiiEngine CreateEngineFromXmlResource( string name )
        {
            using( StringReader sr = new StringReader(Resources.ResourceManager.GetString( name )))
            {
                using( XmlReader r = XmlReader.Create(sr ))
                {
                    return CreateEngineFromXml( r );
                }
            }
        }

        public static YodiiEngine CreateEngineFromXml( XmlReader r )
        {
            YodiiEngine e = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            DiscoveredInfo d = new DiscoveredInfo();

            // Used to index reference links between plugins and services.
            List<PendingGeneralization> pendingGeneralizations = new List<PendingGeneralization>();
            List<PendingPluginService> pendingPluginServices = new List<PendingPluginService>();
            List<PendingServiceReference> pendingServiceReferences = new List<PendingServiceReference>();

            CKSortedArrayKeyList<PluginInfo, string> loadedPlugins;
            CKSortedArrayKeyList<ServiceInfo, string> loadedServices;
            loadedServices = new CKSortedArrayKeyList<ServiceInfo, string>( s => s.ServiceFullName, false );
            loadedPlugins = new CKSortedArrayKeyList<PluginInfo, string>( p => p.PluginFullName, false );

            while( r.Read() )
            {
                // Load services
                if( r.IsStartElement() && r.Name == "Services" )
                {
                    ReadServices( r.ReadSubtree(), d, loadedServices, loadedPlugins, pendingGeneralizations, pendingPluginServices, pendingServiceReferences );
                }

                // Load plugins
                if( r.IsStartElement() && r.Name == "Plugins" )
                {
                    ReadPlugins( r.ReadSubtree(), d, loadedServices, loadedPlugins, pendingPluginServices, pendingServiceReferences );
                }

                // Read configuration manager
                if( r.IsStartElement() && r.Name == "Configuration" )
                {
                    ReadConfigurationManager( e.Configuration, r.ReadSubtree() );
                }
            }

            e.SetDiscoveredInfo( d );

            return e;
        }

        #region De-serialization

        private static void ReadConfigurationManager( IConfigurationManager manager, XmlReader r )
        {
            // We're already inside a Configuration Element.

            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "ConfigurationLayer" )
                {
                    var newLayer = DeserializeConfigurationLayer( manager, r.ReadSubtree() );
                }
            }
        }

        private static IConfigurationLayer DeserializeConfigurationLayer( IConfigurationManager manager, XmlReader r )
        {
            // We're already inside a ConfigurationLayer element.
            r.Read();

            var layerName = r.GetAttribute( "Name" );

            var newLayer = manager.Layers.Create( layerName );

            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "ConfigurationItem" )
                {
                    var serviceOrPluginId = r.GetAttribute( "ServiceOrPluginId" );
                    var status = (ConfigurationStatus)Enum.Parse( typeof( ConfigurationStatus ), r.GetAttribute( "Status" ) );
                    var statusReason = r.GetAttribute( "Reason" );

                    newLayer.Items.Add( serviceOrPluginId, status, statusReason );
                }
            }

            return newLayer;
        }

        private static void ReadServices( XmlReader r, DiscoveredInfo d,
            CKSortedArrayKeyList<ServiceInfo, string> loadedServices,
            CKSortedArrayKeyList<PluginInfo, string> loadedPlugins,
            List<PendingGeneralization> pendingGeneralizations,
            List<PendingPluginService> pendingPluginServices,
            List<PendingServiceReference> pendingServiceReferences
            )
        {
            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "Service" )
                {
                    ReadService( r.ReadSubtree(), d, loadedServices, loadedPlugins, pendingGeneralizations, pendingPluginServices, pendingServiceReferences );
                }
            }
        }

        private static void ReadService( XmlReader r, DiscoveredInfo d,
            CKSortedArrayKeyList<ServiceInfo, string> loadedServices,
            CKSortedArrayKeyList<PluginInfo, string> loadedPlugins,
            List<PendingGeneralization> pendingGeneralizations,
            List<PendingPluginService> pendingPluginServices,
            List<PendingServiceReference> pendingServiceReferences
            )
        {
            r.Read();
            string serviceFullName = r.GetAttribute( "FullName" );
            Debug.Assert( serviceFullName != null, "FullName attribute was found in Service XML element." );

            var s = new ServiceInfo( serviceFullName, d.DefaultAssembly );
            d.ServiceInfos.Add( s );
            loadedServices.Add( s );

            ServiceInfo generalization = null;

            while( r.Read() )
            {
                if( r.IsStartElement() && !r.IsEmptyElement )
                {
                    if( r.Name == "Generalization" )
                    {
                        if( r.Read() )
                        {
                            string generalizationName = r.Value;
                            if( !String.IsNullOrEmpty( generalizationName ) )
                            {
                                if( loadedServices.Contains( generalizationName ) )
                                {
                                    generalization = loadedServices.GetByKey( generalizationName );
                                    s.Generalization = generalization;
                                }
                                else
                                {
                                    pendingGeneralizations.Add( new PendingGeneralization( s, generalizationName ) );
                                }
                            }
                        }
                    }
                }
            }

            // Fix pending references of this service
            foreach( var pg in pendingGeneralizations.Where( x => x.PendingServiceFullName == serviceFullName ).ToList() )
            {
                pg.Service.Generalization = s;
                pendingGeneralizations.Remove( pg );
            }

            foreach( var pps in pendingPluginServices.Where( x => x.PendingServiceFullName == serviceFullName ).ToList() )
            {
                pps.Plugin.Service = s;
                pendingPluginServices.Remove( pps );
            }

            foreach( var psr in pendingServiceReferences.Where( x => x.PendingServiceFullName == serviceFullName ).ToList() )
            {
                psr.Plugin.AddServiceReference( s, psr.Requirement );
                pendingServiceReferences.Remove( psr );
            }
        }

        private static void ReadPlugins( XmlReader r, DiscoveredInfo d,
            CKSortedArrayKeyList<ServiceInfo, string> loadedServices,
            CKSortedArrayKeyList<PluginInfo, string> loadedPlugins,
            List<PendingPluginService> pendingPluginServices,
            List<PendingServiceReference> pendingServiceReferences )
        {
            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "Plugin" )
                {
                    ReadPlugin( r.ReadSubtree(), d, loadedServices, loadedPlugins, pendingPluginServices, pendingServiceReferences );
                }
            }
        }



        private static void ReadPlugin( XmlReader r, DiscoveredInfo d,
            CKSortedArrayKeyList<ServiceInfo, string> loadedServices,
            CKSortedArrayKeyList<PluginInfo, string> loadedPlugins,
            List<PendingPluginService> pendingPluginServices,
            List<PendingServiceReference> pendingServiceReferences
            )
        {
            r.Read();

            string pluginFullName = String.Empty;
            string serviceFullName = null;

            List<Tuple<string,DependencyRequirement>> references = new List<Tuple<string, DependencyRequirement>>();

            while( r.Read() )
            {
                if( r.IsStartElement() && !r.IsEmptyElement )
                {
                    switch( r.Name )
                    {
                        case "FullName":
                            if( r.Read() )
                            {
                                pluginFullName = r.Value;
                            }
                            break;
                        case "Service":
                            if( r.Read() )
                            {
                                serviceFullName = r.Value;
                            }
                            break;
                        case "ServiceReferences":
                            while( r.Read() )
                            {
                                if( r.IsStartElement() && r.Name == "ServiceReference" )
                                {
                                    string serviceFullName2 = r.GetAttribute( "Service" );
                                    if( !String.IsNullOrEmpty( serviceFullName2 ) )
                                    {
                                        DependencyRequirement requirement = (DependencyRequirement)Enum.Parse( typeof( DependencyRequirement ), r.GetAttribute( "Requirement" ) );

                                        references.Add( Tuple.Create( serviceFullName2, requirement ) );

                                    }
                                }
                            }
                            break;
                    }
                }
            }


            PluginInfo p = new PluginInfo( pluginFullName, d.DefaultAssembly );
            d.PluginInfos.Add( p );
            loadedPlugins.Add( p );

            if( !String.IsNullOrEmpty( serviceFullName ) )
            {
                if( loadedServices.Contains( serviceFullName ) )
                {
                    var service = loadedServices.GetByKey( serviceFullName );
                    p.Service = service;
                }
                else
                {
                    pendingPluginServices.Add( new PendingPluginService( p, serviceFullName ) );
                }
            }

            foreach( var t in references )
            {

                if( loadedServices.Contains( t.Item1 ) )
                {
                    p.AddServiceReference( loadedServices.GetByKey( t.Item1 ), t.Item2 );
                }
                else
                {
                    pendingServiceReferences.Add( new PendingServiceReference( p, t.Item1, t.Item2 ) );
                }
            }
        }

        #endregion

        #region Serialization utility classes
        private class PendingServiceReference
        {
            public readonly PluginInfo Plugin;
            public readonly string PendingServiceFullName;
            public readonly DependencyRequirement Requirement;

            internal PendingServiceReference( PluginInfo plugin, string pendingServiceFullName, DependencyRequirement requirement )
            {
                Plugin = plugin;
                PendingServiceFullName = pendingServiceFullName;
                Requirement = requirement;
            }
        }

        private class PendingGeneralization
        {
            public readonly ServiceInfo Service;
            public readonly string PendingServiceFullName;

            internal PendingGeneralization( ServiceInfo service, string pendingServiceFullName )
            {
                Service = service;
                PendingServiceFullName = pendingServiceFullName;
            }
        }

        private class PendingPluginService
        {
            public readonly PluginInfo Plugin;
            public readonly string PendingServiceFullName;

            internal PendingPluginService( PluginInfo plugin, string pendingServiceFullName )
            {
                Plugin = plugin;
                PendingServiceFullName = pendingServiceFullName;
            }
        }

        public class PluginServiceInfoState
        {
            public readonly IEnumerable<ServiceInfo> ServiceInfos;
            public readonly IEnumerable<PluginInfo> PluginInfos;

            internal PluginServiceInfoState( IEnumerable<ServiceInfo> services, IEnumerable<PluginInfo> plugins )
            {
                ServiceInfos = services;
                PluginInfos = plugins;
            }
        }
        #endregion
    }


}
