using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace Yodii.Model
{
    public class FinalConfiguration
    {
        private CKSortedArrayKeyList<FinalConfigurationItem, string> _items;

        public CKSortedArrayKeyList<FinalConfigurationItem, string> Items
        {
            get { return _items; }
        }

        internal FinalConfiguration()
        {
            _items = new CKObservableSortedArrayKeyList<FinalConfigurationItem, string>( e => e.ServiceOrPluginName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), true );
        }
    }
}
