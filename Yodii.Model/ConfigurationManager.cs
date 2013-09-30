using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    class ConfigurationManager
    {
        private List<ConfigurationLayer> _configurationLayerCollection;
        private ConfigurationLayer _finalConfigurationLayer;

        public ConfigurationManager(ConfigurationLayer system)
        {
            _configurationLayerCollection = new List<ConfigurationLayer>();
            _configurationLayerCollection.Add(ResolveSystemConfiguration(system));
        }

        public bool SetConfiguration(Guid pluginGuid, ConfigurationStatus pluginStatus)
        {
            throw new NotImplementedException();
        }

        public bool SetConfiguration(string serviceFullName, ConfigurationStatus serviceStatus)
        {
            throw new NotImplementedException();
        }

        public bool AddLayer(ConfigurationLayer configurationLayer)
        {
            throw new NotImplementedException();
        }

        //demander si on doit remove par une reference d'un objet present dans la collection directement ou plutot par le name
        //du layer ( plus pertinent je pense) de plus si il faut bien renvoyer un bool quand le remove a fonctionné. comme pour le remove d'un dictionnaire par exemple
        public bool RemoveLayer(ConfigurationLayer configurationLayer)
        {
            throw new NotImplementedException();
        }

        internal bool OnConfigurationLayerChanged( ConfigurationItem configurationItem, ConfigurationStatus newStatus)
        {
            throw new NotImplementedException();
        }

        private ConfigurationLayer ResolveSystemConfiguration(ConfigurationLayer system)
        {
            throw new NotImplementedException();
        }
    }
}
