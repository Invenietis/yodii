using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace Yodii.Model
{
    //remove le dernier layer ? remove le dernier item du dernier layer ?
    public class ConfigurationManager : INotifyPropertyChanged
    {
        private ConfigurationLayerCollection _configurationLayerCollection;
        private FinalConfiguration _finalConfiguration;

        public event EventHandler<ConfigurationChangingEventArgs> ConfigurationChanging;
        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        public ConfigurationLayerCollection Layers
        {
            get { return _configurationLayerCollection; }
        }

        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
            private set
            {
                _finalConfiguration = value;

                NotifyPropertyChanged();
            }
        }

        public ConfigurationManager()
        {
            _configurationLayerCollection = new ConfigurationLayerCollection(this);

        }

        //public ConfigurationManager( ConfigurationLayer system )
        //{
        //    _configurationLayerCollection = new Dictionary<string, ConfigurationLayer>();
        //    _configurationLayerCollection.Add(system.ConfigurationName, ResolveSystemConfiguration(system));
        //    FinalConfigurationLayer = CreateFinalConfigurationLayer();
        //}

        public bool AddLayer(ConfigurationLayer configurationLayer)
        {
            throw new NotImplementedException();
        }

        public bool RemoveLayer(ConfigurationLayer configurationLayer)
        {
            if( configurationLayer == null ) throw new ArgumentNullException( "configurationLayer" );

            if( _configurationLayerCollection.Remove( configurationLayer ) )
            {
                FinalConfiguration = GenerateFinalConfiguration();
                return true;
            }
            else
            {
                return false;
            }
        }

        //WARNING : check if this isn't call without layer in ConfigurationManager
        internal bool OnConfigurationLayerChanging(FinalConfigurationChange change, ConfigurationItem configurationItem, ConfigurationStatus newStatus)
        {
            switch( change )
            {
                case FinalConfigurationChange.StatusChanged :
                    return OnConfigurationItemStatusChanging( configurationItem, newStatus );
                case FinalConfigurationChange.ItemAdded :
                    return OnConfigurationItemAdding( configurationItem, newStatus );
                case FinalConfigurationChange.ItemRemoved :
                    return OnConfigurationItemRemoving( configurationItem, newStatus );
            }
            
            
            throw new NotImplementedException();
        }

        private ConfigurationResult OnConfigurationItemStatusChanging( ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            //on appelle CanChangeStatus sur le FinalConfiguration car il agrege l'ensemble des items
            //on ressout donc les conflits entre les differents layers
            if( _finalConfiguration.Items.GetByKey( configurationItem.ServiceOrPluginName ).CanChangeStatus( newStatus ) )
            {
                FinalConfiguration finalConfiguration = GenerateFinalConfiguration();
                finalConfiguration.Items.GetByKey( configurationItem.ServiceOrPluginName ).Status = newStatus;
                ConfigurationChangingEventArgs e = new ConfigurationChangingEventArgs( finalConfiguration, FinalConfigurationChange.StatusChanged, configurationItem );
                RaiseConfigurationChanging( e );
                //generate configurationresult
            }
            else
            {
                //generate configurationresult
                return false;
            }
        }
        private ConfigurationResult OnConfigurationItemAdding( ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            FinalConfiguration finalConfiguration = GenerateFinalConfiguration();
            finalConfiguration.Items.Add( new FinalConfigurationItem( configurationItem ) );
            ConfigurationChangingEventArgs e = new ConfigurationChangingEventArgs( finalConfiguration, FinalConfigurationChange.ItemAdded, configurationItem );
            RaiseConfigurationChanging( e );
            //generate configurationresult
        }

        private FinalConfiguration GenerateFinalConfiguration()
        {
            FinalConfiguration finalConfiguration = new FinalConfiguration();
            foreach( ConfigurationLayer layer in _configurationLayerCollection )
            {
                foreach( ConfigurationItem item in layer.Items )
                {
                    finalConfiguration.Items.Add( new FinalConfigurationItem( item ) );
                }
            }
            return finalConfiguration;
        }

        private ConfigurationLayer ResolveSystemConfiguration(ConfigurationLayer system)
        {
            throw new NotImplementedException();
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

        private void RaiseConfigurationChanging( ConfigurationChangingEventArgs e )
        {
            if( ConfigurationChanging != null )
            {
                ConfigurationChanging( this, e );
            }
        }
        public class ConfigurationLayerCollection : IEnumerable
        {
            private CKObservableSortedArrayKeyList<ConfigurationLayer,string> _layers;
            private ConfigurationManager _parent;

            internal ConfigurationLayerCollection( ConfigurationManager parent )
            {
                _parent = parent;
                _layers = new CKObservableSortedArrayKeyList<ConfigurationLayer, string>( e => e.ConfigurationName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), true );
            }
        }
    }

    public class ConfigurationChangingEventArgs : EventArgs
    {
        private bool _isCanceled;
        private List<string> _causes;
        private FinalConfiguration _finalConfiguration;
        private FinalConfigurationChange _finalConfigurationChanged;
        private ConfigurationItem _configurationItemChanged;
        private ConfigurationLayer _configurationLayerChanged;

        public bool IsCancel
        {
            get { return _isCanceled; }
        }

        public IReadOnlyList<string> Causes
        {
            get { return _causes.ToReadOnlyList(); }
        }

        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
        }

        public FinalConfigurationChange FinalConfigurationChanged
        {
            get { return _finalConfigurationChanged; }
        }

        public ConfigurationItem ConfigurationItemChanged
        {
            get { return _configurationItemChanged; }
        }

        public ConfigurationLayer ConfigurationLayerChanged
        {
            get { return _configurationLayerChanged; }
        }

        internal ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, ConfigurationItem configurationItem )
        {
            _isCanceled = false;
            _causes = new List<string>();
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChanged = finalConfigurationChanged;
            _configurationItemChanged = configurationItem;
        }

        internal ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, ConfigurationLayer configurationLayer )
        {
            _isCanceled = false;
            _causes = new List<string>();
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChanged = finalConfigurationChanged;
            _configurationLayerChanged = configurationLayer;
        }

        public void Cancel(string cause)
        {
            _isCanceled = true;
            _causes.Add( cause );
        }
    }

    public class ConfigurationChangedEventArgs : EventArgs
    {
        private FinalConfiguration _finalConfiguration;

        private FinalConfigurationChange _finalConfigurationChanged;
        private ConfigurationItem _configurationItemChanged;
        private ConfigurationLayer _configurationLayerChanged;

        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
        }

        public FinalConfigurationChange FinalConfigurationChanged
        {
            get { return _finalConfigurationChanged; }
        }

        public ConfigurationItem ConfigurationItemChanged
        {
            get { return _configurationItemChanged; }
        }

        public ConfigurationLayer ConfigurationLayerChanged
        {
            get { return _configurationLayerChanged; }
        }

        internal ConfigurationChangedEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, ConfigurationItem configurationItem )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChanged = finalConfigurationChanged;
            _configurationItemChanged = configurationItem;
        }

        internal ConfigurationChangedEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, ConfigurationLayer configurationLayer )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChanged = finalConfigurationChanged;
            _configurationLayerChanged = configurationLayer;
        }
    }
}
