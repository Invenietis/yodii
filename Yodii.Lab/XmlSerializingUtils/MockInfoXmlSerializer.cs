using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using Yodii.Lab.Mocks;
using CK.Core;
using Yodii.Model;

namespace Yodii.Lab.Mocks
{
    internal static class MockInfoXmlSerializer
    {
        public static void SerializeLabStateToXmlWriter( MainWindowViewModel vm, XmlWriter w )
        {
            w.WriteStartElement( "YodiiLabState" );

            w.WriteStartElement( "Services" );
            foreach( ServiceInfo si in vm.ServiceInfos )
            {
                SerializeServiceInfoToXmlWriter( si, w );
            }
            w.WriteEndElement();

            w.WriteStartElement( "Plugins" );
            foreach( PluginInfo pi in vm.PluginInfos )
            {
                SerializePluginInfoToXmlWriter( pi, w );
            }
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private static void SerializeServiceInfoToXmlWriter( ServiceInfo si, XmlWriter w )
        {
            w.WriteStartElement( "Service" );

            w.WriteStartAttribute( "FullName" );
            w.WriteValue( si.ServiceFullName );
            w.WriteEndAttribute();

            w.WriteStartElement( "Generalization" );
            if( si.Generalization != null ) w.WriteValue( si.Generalization.ServiceFullName );
            w.WriteEndElement();

            // That's pretty much all we need. Implementations will be filled by the plugins themselves.
            // HasError, AssemblyInfo and others aren't supported at this time.

            w.WriteEndElement();
        }

        private static void SerializePluginInfoToXmlWriter( PluginInfo pi, XmlWriter w )
        {
            w.WriteStartElement( "Plugin" );

            w.WriteStartAttribute( "Guid" );
            w.WriteValue( pi.PluginId.ToString() );
            w.WriteEndAttribute();

            w.WriteStartElement( "FullName" );
            if( pi.PluginFullName != null ) w.WriteValue( pi.PluginFullName );
            w.WriteEndElement();

            w.WriteStartElement( "Service" );
            if( pi.Service != null ) w.WriteValue( pi.Service.ServiceFullName );
            w.WriteEndElement();

            w.WriteStartElement( "ServiceReferences" );
            foreach( var serviceRef in pi.ServiceReferences )
            {
                Debug.Assert( serviceRef.Owner == pi );

                w.WriteStartElement( "ServiceReference" );

                w.WriteStartAttribute( "Service" );
                w.WriteValue( serviceRef.Reference.ServiceFullName );
                w.WriteEndAttribute();

                w.WriteStartAttribute( "Requirement" );
                w.WriteValue( serviceRef.Requirement.ToString() );
                w.WriteEndAttribute();

                w.WriteEndElement();
            }
            w.WriteEndElement();

            w.WriteEndElement();
        }

        public static PluginServiceInfoState DeserializeLabStateFromXmlReader( XmlReader r )
        {
            List<PendingGeneralization> pendingGeneralizations = new List<PendingGeneralization>();
            List<PendingPluginService> pendingPluginServices = new List<PendingPluginService>();
            List<PendingServiceReference> pendingServiceReferences = new List<PendingServiceReference>();
            
            CKSortedArrayKeyList<PluginInfo, Guid> loadedPlugins;
            CKSortedArrayKeyList<ServiceInfo, string> loadedServices;

            loadedServices = new CKSortedArrayKeyList<ServiceInfo, string>( s => s.ServiceFullName, false );
            loadedPlugins = new CKSortedArrayKeyList<PluginInfo, Guid>( p => p.PluginId, false );

            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "Services" )
                {
                    ReadServices( r.ReadSubtree(), loadedServices, loadedPlugins, pendingGeneralizations, pendingPluginServices, pendingServiceReferences );
                } else if( r.IsStartElement() && r.Name == "Plugins" )
                {
                    ReadPlugins( r.ReadSubtree(), loadedServices, loadedPlugins, pendingPluginServices, pendingServiceReferences );
                }
            }

            Debug.Assert( pendingGeneralizations.Count == 0, "No read service is missing a generalization" );
            Debug.Assert( pendingPluginServices.Count == 0, "No read plugin is missing a service" );
            Debug.Assert( pendingServiceReferences.Count == 0, "No read plugin is missing a reference" );

            return new PluginServiceInfoState(loadedServices, loadedPlugins);
        }

        private static void ReadPlugins( XmlReader r,
            CKSortedArrayKeyList<ServiceInfo, string> loadedServices,
            CKSortedArrayKeyList<PluginInfo, Guid> loadedPlugins,
            List<PendingPluginService> pendingPluginServices,
            List<PendingServiceReference> pendingServiceReferences )
        {
            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "Plugin" )
                {
                    ReadPlugin( r.ReadSubtree(), loadedServices, loadedPlugins, pendingPluginServices, pendingServiceReferences );
                }
            }
        }

        private static void ReadServices( XmlReader r,
            CKSortedArrayKeyList<ServiceInfo, string> loadedServices,
            CKSortedArrayKeyList<PluginInfo, Guid> loadedPlugins,
            List<PendingGeneralization> pendingGeneralizations,
            List<PendingPluginService> pendingPluginServices,
            List<PendingServiceReference> pendingServiceReferences
            )
        {
            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "Service" )
                {
                    ReadService( r.ReadSubtree(), loadedServices, loadedPlugins, pendingGeneralizations, pendingPluginServices, pendingServiceReferences );
                }
            }
        }

        private static void ReadPlugin( XmlReader r,
            CKSortedArrayKeyList<ServiceInfo, string> loadedServices,
            CKSortedArrayKeyList<PluginInfo, Guid> loadedPlugins,
            List<PendingPluginService> pendingPluginServices,
            List<PendingServiceReference> pendingServiceReferences
            )
        {
            r.Read();

            string guidString = r.GetAttribute( "Guid" );
            Guid guid = Guid.Parse( guidString );

            PluginInfo p = new PluginInfo(guid, null, AssemblyInfoHelper.ExecutingAssemblyInfo);
            loadedPlugins.Add( p );

            while( r.Read() )
            {
                if( r.IsStartElement() && !r.IsEmptyElement )
                {
                    switch( r.Name )
                    {
                        case "FullName":
                            if( r.Read() )
                            {
                                string fullName = r.Value;
                                p.PluginFullName = fullName;
                            }
                            break;
                        case "Service":
                            if( r.Read() )
                            {
                                string serviceFullName = r.Value;
                                if( !String.IsNullOrEmpty( serviceFullName ) )
                                {
                                    if( loadedServices.Contains(serviceFullName ) )
                                    {
                                        var service = loadedServices.GetByKey(serviceFullName);
                                        p.Service = service;
                                        service.InternalImplementations.Add( p );
                                    }
                                    else
                                    {
                                        pendingPluginServices.Add( new PendingPluginService( p, serviceFullName ) );
                                    }
                                }
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

                                        if( loadedServices.Contains( serviceFullName2 ) )
                                        {
                                            MockServiceReferenceInfo i = new MockServiceReferenceInfo( p, loadedServices.GetByKey(serviceFullName2), requirement );
                                            p.InternalServiceReferences.Add( i );
                                        }
                                        else
                                        {
                                            pendingServiceReferences.Add( new PendingServiceReference( p, serviceFullName2, requirement ) );
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        private static void ReadService( XmlReader r,
            CKSortedArrayKeyList<ServiceInfo, string> loadedServices,
            CKSortedArrayKeyList<PluginInfo, Guid> loadedPlugins,
            List<PendingGeneralization> pendingGeneralizations,
            List<PendingPluginService> pendingPluginServices,
            List<PendingServiceReference> pendingServiceReferences
            )
        {
            r.Read();
            string serviceFullName = r.GetAttribute( "FullName" );
            Debug.Assert( serviceFullName != null, "FullName attribute was found in Service XML element." );

            ServiceInfo s = new ServiceInfo( serviceFullName, AssemblyInfoHelper.ExecutingAssemblyInfo );
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
                s.InternalImplementations.Add( pps.Plugin );
                pendingPluginServices.Remove( pps );
            }

            foreach( var psr in pendingServiceReferences.Where( x => x.PendingServiceFullName == serviceFullName ).ToList() )
            {
                var reference = new MockServiceReferenceInfo( psr.Plugin, s, psr.Requirement );
                psr.Plugin.InternalServiceReferences.Add( reference );
                pendingServiceReferences.Remove( psr );
            }
        }

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
    }
}
