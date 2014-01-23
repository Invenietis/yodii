using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace Yodii.Model
{
    /// <summary>
    /// Resolved, read-only configuration.
    /// </summary>
    public class FinalConfiguration
    {
        readonly CKSortedArrayKeyList<FinalConfigurationItem, string> _items;

        /// <summary>
        /// Items of this final configuration.
        /// </summary>
        public IReadOnlyList<FinalConfigurationItem> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Gets the final configuration status for a given service or plugin ID.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin ID to check.</param>
        /// <returns>The status of the item, or Optional if the item does not exist.</returns>
        public ConfigurationStatus GetStatus( string serviceOrPluginFullName )
        {
            Debug.Assert( ConfigurationStatus.Optional == 0 );
            return _items.GetByKey( serviceOrPluginFullName ).Status;
        }

        /// <summary>
        /// Gets the final configuration impact for a given service or plugin ID.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin ID to check.</param>
        /// <returns>The impact of the item.</returns>
        public StartDependencyImpact GetImpact( string serviceOrPluginFullName )
        {
            return _items.GetByKey( serviceOrPluginFullName ).Impact;
        }

        /// <summary>
        /// Creates a new instance of FinalConfiguration, using given statuses.
        /// </summary>
        /// <param name="finalStatusAndImpact">Statuses to set.</param>
        public FinalConfiguration( Dictionary<string, FinalConfigurationItem> finalStatusAndImpact )
            : this()
        {
            foreach( var item in finalStatusAndImpact ) _items.Add( new FinalConfigurationItem(item.Key, item.Value.Status, item.Value.Impact) );
        }

        /// <summary>
        /// Creates a new, empty FinalConfiguration.
        /// </summary>
        public FinalConfiguration()
        {
            _items = new CKSortedArrayKeyList<FinalConfigurationItem, string>( e => e.ServiceOrPluginFullName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ) );
        }
    }
}
