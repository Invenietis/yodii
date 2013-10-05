using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public class ConfigurationManager : INotifyPropertyChanged
    {
        private static readonly string FINAL_LAYER_NAME = "Final Layer";


        private Dictionary<string,ConfigurationLayer> _configurationLayerCollection;
        private ConfigurationLayer _finalConfigurationLayer;

        public ConfigurationLayer FinalConfigurationLayer
        {
            get { return _finalConfigurationLayer; }
            private set
            {
                _finalConfigurationLayer = value;
                NotifyPropertyChanged();
            }
        }

        public ConfigurationManager()
        {
            _configurationLayerCollection = new Dictionary<string, ConfigurationLayer>();
        }

        public ConfigurationManager( ConfigurationLayer system )
        {
            _configurationLayerCollection = new Dictionary<string, ConfigurationLayer>();
            _configurationLayerCollection.Add(system.ConfigurationName, ResolveSystemConfiguration(system));
            FinalConfigurationLayer = CreateFinalConfigurationLayer();
        }

        public bool AddLayer(ConfigurationLayer configurationLayer)
        {
            throw new NotImplementedException();
        }

        //demander si on doit remove par une reference d'un objet present dans la collection directement ou plutot par le name
        //du layer ( plus pertinent je pense) de plus si il faut bien renvoyer un bool quand le remove a fonctionné. comme pour le remove d'un dictionnaire par exemple
        public bool RemoveLayer(string configurationLayerName)
        {
            if( string.IsNullOrEmpty( configurationLayerName ) ) throw new ArgumentNullException( "configurationLayerName is null" );

            if( _configurationLayerCollection.Remove( configurationLayerName ) )
            {
                FinalConfigurationLayer = CreateFinalConfigurationLayer();
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool OnConfigurationLayerChanged( ConfigurationItem configurationItem, ConfigurationStatus newStatus)
        {
            throw new NotImplementedException();
        }

        private ConfigurationLayer ResolveSystemConfiguration(ConfigurationLayer system)
        {
            throw new NotImplementedException();
        }

        //need performance test
        private ConfigurationLayer CreateFinalConfigurationLayer()
        {
            Dictionary<string, ConfigurationItem> temp = new Dictionary<string, ConfigurationItem>();
            foreach( ConfigurationLayer layer in _configurationLayerCollection.Values )
            {
                foreach( ConfigurationItem item in layer.ConfigurationItemCollection )
                {
                    if( temp.ContainsKey( item.ServiceOrPluginName ) )
                    {
                        if( temp[item.ServiceOrPluginName].CanChangeStatus( item.Status ) )
                        {
                            temp[item.ServiceOrPluginName] = item;
                        }
                    }
                    else
                    {
                        temp.Add( item.ServiceOrPluginName, item );
                    }
                }
            }
            //use a internal constructor because...
            return new ConfigurationLayer(FINAL_LAYER_NAME, temp.Values.ToList(), this);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged( [CallerMemberName] String propertyName = "" )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        #endregion INotifyPropertyChanged
    }
}
