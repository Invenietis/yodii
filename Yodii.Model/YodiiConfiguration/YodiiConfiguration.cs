using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Simple serializable POCO that describes a Yodii configuration.
    /// </summary>
    [Serializable]
    public sealed class YodiiConfiguration : ISerializable
    {
        readonly List<YodiiConfigurationLayer> _layers;
        IDiscoveredInfo _discoveredInfo;

        /// <summary>
        /// Initializes a new <see cref="YodiiConfiguration"/>.
        /// </summary>
        public YodiiConfiguration()
        {
            _layers = new List<YodiiConfigurationLayer>();
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Info object.</param>
        /// <param name="context">Serialization context (unused).</param>
        YodiiConfiguration( SerializationInfo info, StreamingContext context )
        {
            _layers = (List<YodiiConfigurationLayer>)info.GetValue( "_layers", typeof( List<YodiiConfigurationLayer> ) );
            if( info.GetBoolean( "hasInfo" ) )
            {
                _discoveredInfo = (IDiscoveredInfo)info.GetValue( "_discoveredInfo", typeof( IDiscoveredInfo ) );
            }
        }

        /// <summary>
        /// Gets or sets the discovered info.
        /// Must be not null for this <see cref="YodiiConfiguration"/> to be valid.
        /// If the implementation is serializable, it will be serialized (and restored) otherwise 
        /// it will be restored as null.
        /// </summary>
        public IDiscoveredInfo DiscoveredInfo 
        {
            get { return _discoveredInfo; }
            set { _discoveredInfo = value; }
        }

        /// <summary>
        /// Gets an editable list of <see cref="YodiiConfigurationLayer"/>.
        /// </summary>
        public IList<YodiiConfigurationLayer> Layers
        {
            get { return _layers; }
        }

        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "_layers", _layers );
            if( _discoveredInfo != null && _discoveredInfo.GetType().IsSerializable )
            {
                info.AddValue( "hasInfo", true );
                info.AddValue( "_discoveredInfo", _discoveredInfo );
            }
            else info.AddValue( "hasInfo", false );
        }
    }
}
