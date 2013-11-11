using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Yodii.Model;

namespace Yodii.Model
{
    public static class ConfigurationManagerXmlSerializer
    {
        public static void SerializeConfigurationManager( ConfigurationManager m, XmlWriter w )
        {
            w.WriteStartElement( "YodiiConfiguration" );
            foreach( var layer in m.Layers )
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
            w.WriteEndElement();
        }

        public static ConfigurationManager DeserializeConfigurationManager( XmlReader r )
        {
            ConfigurationManager m = new ConfigurationManager();

            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "ConfigurationLayer" )
                {
                    m.Layers.Add( DeserializeConfigurationLayer( r.ReadSubtree(), m ) );
                }
            }

            return m;
        }

        private static ConfigurationLayer DeserializeConfigurationLayer( XmlReader r, ConfigurationManager m )
        {
            r.Read();
            string layerName = r.GetAttribute( "Name" );

            ConfigurationLayer l = new ConfigurationLayer( layerName );

            while( r.Read() )
            {
                if( r.IsStartElement() && r.Name == "ConfigurationItem" )
                {
                    string itemId = r.GetAttribute( "ServiceOrPluginId" );
                    ConfigurationStatus status = (ConfigurationStatus)Enum.Parse( typeof( ConfigurationStatus ), r.GetAttribute( "Status" ) );
                    string reason = r.GetAttribute( "Reason" );

                    l.Items.Add( itemId, status, reason );
                }
            }

            return l;
        }

    }

}
