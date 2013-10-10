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

        internal bool ChangeStatusItem( string ServiceOrPluginName, ConfigurationStatus status )
        {
            bool exist;
            _items.GetByKey( ServiceOrPluginName, out exist ).Status = status;
            return exist;
        }

        internal bool ConcatLayer( ConfigurationLayer layer )
        {
            bool exist;
            FinalConfigurationItem finalItem;
            foreach( ConfigurationItem item in layer.Items )
            {
                finalItem = _items.GetByKey( item.ServiceOrPluginName, out exist );
                if( exist )
                {
                    if( item.CanChangeStatus( item.Status ) )
                    {
                        finalItem.Status = item.Status;
                    }
                }
                else
                {
                    if( !_items.Add( new FinalConfigurationItem( item ) ) )
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
