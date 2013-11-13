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
            get
            {
                Guid pluginGuid;
                bool isPlugin = Guid.TryParse(_item.ServiceOrPluginId, out pluginGuid);

                if( isPlugin )
                {
                    PluginInfo p = _serviceInfoManager.PluginInfos.Where( x => x.PluginId == pluginGuid ).FirstOrDefault();
                    if( p != null)
                    {
                        if( String.IsNullOrWhiteSpace(p.PluginFullName))
                        {
                            return String.Format( "Plugin: Unnamed plugin ({0})", pluginGuid.ToString() );
                        }
                        else
                        {
                            return String.Format( "Plugin: {0}", p.PluginFullName );
                        }
                    }
                    else
                    {
                        return String.Format( "Plugin: Unknown ({0})", pluginGuid.ToString() );
                    }
                }
                else
                {
                    ServiceInfo s = _serviceInfoManager.ServiceInfos.Where( x => x.ServiceFullName == _item.ServiceOrPluginId ).FirstOrDefault();

                    if( s != null )
                    {
                        return String.Format( "Service: {0}", _item.ServiceOrPluginId );
                    }
                    else
                    {
                        return String.Format( "Service: Unknown ({0})", _item.ServiceOrPluginId );
                    }
                }
            }
        }
    }
}
