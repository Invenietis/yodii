using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using CK.Core;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab
{
    static class LabXmlSerialization
    {
        public static void SerializeToXml( LabStateManager state, XmlWriter w )
        {
            PersistedLabState stateToSerialize = CreatePersistentObject( state );

            w.WriteStartElement( "Yodii.Lab" );

            w.WriteStartElement( "Services" );
            foreach( ServiceInfo si in state.ServiceInfos )
            {
                w.WriteStartElement( "Service" );
                SerializeServiceInfoToXmlWriter( si, w );
                w.WriteEndElement();
            }
            w.WriteEndElement();

            w.WriteStartElement( "Plugins" );
            foreach( PluginInfo pi in state.PluginInfos )
            {
                w.WriteStartElement( "Plugin" );
                SerializePluginInfoToXmlWriter( pi, w );
                w.WriteEndElement();
            }
            w.WriteEndElement();

            w.WriteStartElement( "Configuration" );
            SerializeConfigurationManager( stateToSerialize, w );
            w.WriteEndElement();

            w.WriteEndElement();
        }

        public static void DeserializeAndResetStateFromXml( this LabStateManager state, XmlReader r )
        {
            LabXmlDeserializer helper = new LabXmlDeserializer( state, r );
            helper.Deserialize();
        }

        #region Conversion & application of persisted classes
        private static PersistedLabState CreatePersistentObject( LabStateManager runningState )
        {
            PersistedLabState persistedState = new PersistedLabState();

            // Persist layer
            foreach( IConfigurationLayer l in runningState.Engine.Configuration.Layers )
            {
                PersistedConfigurationLayer persistedLayer = new PersistedConfigurationLayer();
                persistedState.ConfigurationLayers.Add( persistedLayer );

                persistedLayer.LayerName = l.LayerName;
                foreach( IConfigurationItem i in l.Items )
                {
                    PersistedConfigurationItem persistedItem = new PersistedConfigurationItem();
                    persistedItem.ServiceOrPluginId = i.ServiceOrPluginFullName;
                    persistedItem.Status = i.Status;
                    persistedItem.StatusReason = i.StatusReason;

                    persistedLayer.Items.Add( persistedItem );
                }
            }

            // Persist service and plugins -- We already have the mocks, so we can use them.
            foreach( ServiceInfo s in runningState.ServiceInfos )
            {
                persistedState.Services.Add( s );
            }
            foreach( PluginInfo p in runningState.PluginInfos )
            {
                persistedState.Plugins.Add( p );
            }

            return persistedState;
        }
        #endregion

        #region Serialization
        private static void SerializeConfigurationManager( PersistedLabState s, XmlWriter w )
        {
            foreach( var layer in s.ConfigurationLayers )
            {
                w.WriteStartElement( "ConfigurationLayer" );

                w.WriteStartAttribute( "Name" );
                w.WriteValue( layer.LayerName );
                w.WriteEndAttribute();

                foreach( var item in layer.Items )
                {
                    w.WriteStartElement( "ConfigurationItem" );

                    w.WriteStartAttribute( "ServiceOrPluginId" );
                    w.WriteValue( item.ServiceOrPluginId );
                    w.WriteEndAttribute();

                    w.WriteStartAttribute( "Status" );
                    w.WriteValue( item.Status.ToString() );
                    w.WriteEndAttribute();

                    w.WriteStartAttribute( "Reason" );
                    w.WriteValue( item.StatusReason );
                    w.WriteEndAttribute();

                    w.WriteEndElement();
                }

                w.WriteEndElement();
            }
        }

        private static void SerializeServiceInfoToXmlWriter( ServiceInfo si, XmlWriter w )
        {
            w.WriteStartAttribute( "FullName" );
            w.WriteValue( si.ServiceFullName );
            w.WriteEndAttribute();

            w.WriteStartElement( "Generalization" );
            if( si.Generalization != null ) w.WriteValue( si.Generalization.ServiceFullName );
            w.WriteEndElement();

            // That's pretty much all we need. Implementations will be guessed from the plugins themselves.
            // HasError, AssemblyInfo and others aren't supported at this time, but can be added here later on.
        }

        private static void SerializePluginInfoToXmlWriter( PluginInfo pi, XmlWriter w )
        {
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
        }
        #endregion

    }

    class LabXmlDeserializer
    {
        CKSortedArrayKeyList<ServiceInfo, string> loadedServices;
        CKSortedArrayKeyList<PluginInfo, string> loadedPlugins;
        List<PendingGeneralization> pendingGeneralizations;
        List<PendingPluginService> pendingPluginServices;
        List<PendingServiceReference> pendingServiceReferences;
        LabStateManager state;
        PersistedLabState deserializedState;
        XmlReader r;


        internal LabXmlDeserializer( LabStateManager state, XmlReader r )
        {
            this.state = state;
            this.r = r;
            deserializedState = new PersistedLabState();
            // Used to index reference links between plugins and services.
            pendingGeneralizations = new List<PendingGeneralization>();
            pendingPluginServices = new List<PendingPluginService>();
            pendingServiceReferences = new List<PendingServiceReference>();

            loadedServices = new CKSortedArrayKeyList<ServiceInfo, string>( s => s.ServiceFullName, false );
            loadedPlugins = new CKSortedArrayKeyList<PluginInfo, string>( p => p.PluginFullName, false );
        }

        internal void Deserialize()
        {
            if( state.Engine.IsRunning )
            {
                state.Engine.Stop();
            }

            while( r.Read() )
            {
                // Load services
                if( r.IsStartElement() && r.Name == "Services" )
                {
                    ReadServices( r.ReadSubtree() );
                }

                // Load plugins
                if( r.IsStartElement() && r.Name == "Plugins" )
                {
                    ReadPlugins( r.ReadSubtree() );
                }

                // Read configuration manager
                if( r.IsStartElement() && r.Name == "Configuration" )
                {
                    ReadConfigurationManager( r.ReadSubtree() );
                }
            }

            foreach( var s in loadedServices )
            {
                deserializedState.Services.Add( s );
            }
            foreach( var p in loadedPlugins )
            {
                deserializedState.Plugins.Add( p );
            }

            ApplyPersistedStateToLab();
        }

        private void ReadConfigurationManager( XmlReader r )
        {
            // We're already inside a Configuration Element.

            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "ConfigurationLayer" )
                {
                    var newLayer = DeserializeConfigurationLayer( r.ReadSubtree() );
                    deserializedState.ConfigurationLayers.Add( newLayer );
                }
            }
        }

        private PersistedConfigurationLayer DeserializeConfigurationLayer( XmlReader r )
        {
            PersistedConfigurationLayer newLayer = new PersistedConfigurationLayer();

            // We're already inside a ConfigurationLayer element.
            r.Read();
            newLayer.LayerName = r.GetAttribute( "Name" );

            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "ConfigurationItem" )
                {
                    PersistedConfigurationItem item = new PersistedConfigurationItem();
                    item.ServiceOrPluginId = r.GetAttribute( "ServiceOrPluginId" );
                    item.Status = (ConfigurationStatus)Enum.Parse( typeof( ConfigurationStatus ), r.GetAttribute( "Status" ) );
                    item.StatusReason = r.GetAttribute( "Reason" );

                    newLayer.Items.Add( item );
                }
            }

            return newLayer;
        }

        private void ReadServices( XmlReader r )
        {
            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "Service" )
                {
                    ReadService( r.ReadSubtree() );
                }
            }
        }

        private void ReadService( XmlReader r )
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

        private void ReadPlugins( XmlReader r )
        {
            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "Plugin" )
                {
                    ReadPlugin( r.ReadSubtree() );
                }
            }
        }

        private void ReadPlugin( XmlReader r )
        {
            r.Read();

            PluginInfo p = new PluginInfo( null, AssemblyInfoHelper.ExecutingAssemblyInfo );
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
                                    if( loadedServices.Contains( serviceFullName ) )
                                    {
                                        var service = loadedServices.GetByKey( serviceFullName );
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
                                            MockServiceReferenceInfo i = new MockServiceReferenceInfo( p, loadedServices.GetByKey( serviceFullName2 ), requirement );
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

        private void ApplyPersistedStateToLab()
        {
            // Stop running engine
            if( state.Engine.IsRunning )
            {
                state.Engine.Stop();
            }

            // Clear configuration manager
            foreach( var l in state.Engine.Configuration.Layers.ToList() )
            {
                var result = state.Engine.Configuration.Layers.Remove( l );
                Debug.Assert( result.Success );
            }

            // Clear services and plugins
            state.ClearState();

            // Add services and plugins
            foreach( ServiceInfo s in deserializedState.Services )
            {
                state.LoadServiceInfo( s );
            }
            foreach( PluginInfo p in deserializedState.Plugins )
            {
                state.LoadPluginInfo( p );
            }

            // Load configuration manager data
            foreach( PersistedConfigurationLayer l in deserializedState.ConfigurationLayers )
            {
                IConfigurationLayer newLayer = state.Engine.Configuration.Layers.Create( l.LayerName );

                foreach( PersistedConfigurationItem item in l.Items )
                {
                    var result = newLayer.Items.Add( item.ServiceOrPluginId, item.Status, item.StatusReason );
                    Debug.Assert( result.Success );
                }
            }
        }

    }

    #region Persistent state classes
    class PersistedLabState
    {
        public List<PersistedConfigurationLayer> ConfigurationLayers { get; private set; }
        public List<ServiceInfo> Services { get; private set; }
        public List<PluginInfo> Plugins { get; private set; }

        public PersistedLabState()
        {
            ConfigurationLayers = new List<PersistedConfigurationLayer>();
            Services = new List<ServiceInfo>();
            Plugins = new List<PluginInfo>();
        }
    }

    class PersistedConfigurationLayer
    {
        public string LayerName { get; set; }
        public List<PersistedConfigurationItem> Items { get; private set; }

        internal PersistedConfigurationLayer()
        {
            Items = new List<PersistedConfigurationItem>();
        }
    }

    class PersistedConfigurationItem
    {
        public string ServiceOrPluginId { get; set; }
        public ConfigurationStatus Status { get; set; }
        public string StatusReason { get; set; }
    }
    #endregion

    #region Serialization utility classes
    class PendingServiceReference
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

    class PendingGeneralization
    {
        public readonly ServiceInfo Service;
        public readonly string PendingServiceFullName;

        internal PendingGeneralization( ServiceInfo service, string pendingServiceFullName )
        {
            Service = service;
            PendingServiceFullName = pendingServiceFullName;
        }
    }

    class PendingPluginService
    {
        public readonly PluginInfo Plugin;
        public readonly string PendingServiceFullName;

        internal PendingPluginService( PluginInfo plugin, string pendingServiceFullName )
        {
            Plugin = plugin;
            PendingServiceFullName = pendingServiceFullName;
        }
    }

    class PluginServiceInfoState
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
