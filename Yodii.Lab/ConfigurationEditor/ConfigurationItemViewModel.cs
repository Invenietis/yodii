using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab.ConfigurationEditor
{
    public class ConfigurationItemViewModel
    {
        readonly ConfigurationItem _item;
        readonly ServiceInfoManager _serviceInfoManager;

        internal ConfigurationItemViewModel( ConfigurationItem item, ServiceInfoManager manager )
        {
            Debug.Assert( item != null );
            Debug.Assert( manager != null );

            _serviceInfoManager = manager;
            _item = item;

            DeleteItemCommand = new RelayCommand( ExecuteDeleteItem );
        }

        private void ExecuteDeleteItem( object obj )
        {
            _item.Layer.Items.Remove( _item.ServiceOrPluginId );
        }

        public ICommand DeleteItemCommand { get; private set; }

        public ConfigurationItem Item
        {
            get { return _item; }
        }

        public string Description
        {
            get { return _serviceInfoManager.GetDescriptionOfServiceOrPluginId( _item.ServiceOrPluginId ); }
        }
    }
}
