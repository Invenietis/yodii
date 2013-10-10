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

        internal void OnConfigurationManagerChanged( FinalConfigurationChange change, ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            FinalConfiguration = GenerateFinalConfiguration();
            RaiseConfigurationChanged( new ConfigurationChangedEventArgs( _finalConfiguration, change, configurationItem ) );
        }

        internal void OnConfigurationManagerChanged( FinalConfigurationChange change, ConfigurationLayer configurationLayer )
        {
            FinalConfiguration = GenerateFinalConfiguration();
            RaiseConfigurationChanged( new ConfigurationChangedEventArgs( _finalConfiguration, change, configurationLayer ) );
        }
        

        //WARNING : check if this isn't call without layer in ConfigurationManager
        internal ConfigurationResult OnConfigurationLayerChanging(FinalConfigurationChange change, ConfigurationItem configurationItem, ConfigurationStatus newStatus)
        {
            switch( change )
            {
                case FinalConfigurationChange.StatusChanged :
                    return OnConfigurationItemStatusChanging( configurationItem, newStatus );
                case FinalConfigurationChange.ItemAdded :
                    return OnConfigurationItemAdding( configurationItem, newStatus );
                case FinalConfigurationChange.ItemRemoved :
                    return OnConfigurationItemRemoving( configurationItem, newStatus );
                default :
                    return new ConfigurationResult( "" ); //ecriremessage d'erreur
            }
        }

        private ConfigurationResult OnConfigurationItemStatusChanging( ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            //on appelle CanChangeStatus sur le FinalConfiguration car il agrege l'ensemble des items
            //on ressout donc les conflits entre les differents layers
            if( _finalConfiguration.Items.GetByKey( configurationItem.ServiceOrPluginName ).CanChangeStatus( newStatus ) )
            {
                FinalConfiguration finalConfiguration = GenerateFinalConfiguration();
                finalConfiguration.ChangeStatusItem( configurationItem.ServiceOrPluginName, configurationItem.Status );

                ConfigurationChangingEventArgs e = new ConfigurationChangingEventArgs( finalConfiguration, FinalConfigurationChange.StatusChanged, configurationItem );
                RaiseConfigurationChanging( e );

                if( e.IsCanceled )
                {
                    return new ConfigurationResult( e.Causes );
                }
                else
                {
                    return new ConfigurationResult();
                }
            }
            else
            {
                return new ConfigurationResult( "A new ConfigurationItem, or a ConfigurationStatus change, conflicts with another item in the FinalConfiguration" );
            }
        }

        private ConfigurationResult OnConfigurationItemAdding( ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            if( _finalConfiguration.Items.Contains( configurationItem.ServiceOrPluginName ) )
            {
                return OnConfigurationItemStatusChanging( configurationItem, newStatus );
            }
            else
            {
                FinalConfiguration finalConfiguration = GenerateFinalConfiguration();
                finalConfiguration.Items.Add( new FinalConfigurationItem( configurationItem ) );

                ConfigurationChangingEventArgs e = new ConfigurationChangingEventArgs( finalConfiguration, FinalConfigurationChange.ItemAdded, configurationItem );
                RaiseConfigurationChanging( e );

                if( e.IsCanceled )
                {
                    return new ConfigurationResult( e.Causes );
                }
                else
                {
                    return new ConfigurationResult();
                }
            }
        }

        private ConfigurationResult OnConfigurationItemRemoving( ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            return new ConfigurationResult();
        }

        internal ConfigurationResult OnConfigurationManagerChanging( FinalConfigurationChange change, ConfigurationLayer configurationLayer )
        {
            if( change == FinalConfigurationChange.LayerAdded )
            {
                if( _finalConfiguration == null )
                {
                    FinalConfiguration = ResolveBasicConfiguration( configurationLayer );
                    return new ConfigurationResult();
                }
                else
                {
                    FinalConfiguration finalConfiguration = GenerateFinalConfiguration();
                    finalConfiguration.ConcatLayer( configurationLayer );

                    ConfigurationChangingEventArgs e = new ConfigurationChangingEventArgs( finalConfiguration, FinalConfigurationChange.LayerAdded, configurationLayer );
                    RaiseConfigurationChanging( e );

                    if( e.IsCanceled )
                    {
                        return new ConfigurationResult( e.Causes );
                    }
                    else
                    {
                        return new ConfigurationResult();
                    }
                }
            }
            else
            {
                return new ConfigurationResult();
            }
        }

        private FinalConfiguration GenerateFinalConfiguration()
        {
            FinalConfiguration finalConfiguration = new FinalConfiguration();
            foreach( ConfigurationLayer layer in _configurationLayerCollection )
            {
                finalConfiguration.ConcatLayer( layer );
            }
            return finalConfiguration;
        }

        private FinalConfiguration ResolveBasicConfiguration(ConfigurationLayer firstLayer)
        {
            FinalConfiguration finalConfiguration = new FinalConfiguration();
            finalConfiguration.ConcatLayer( firstLayer );

            ConfigurationChangingEventArgs e = new ConfigurationChangingEventArgs( finalConfiguration, FinalConfigurationChange.LayerAdded, firstLayer );
            RaiseConfigurationChanging( e );

            if( e.IsCanceled )
            {
                firstLayer.Items.Remove( firstLayer.Items[firstLayer.Items.Count].ServiceOrPluginName );
                return ResolveBasicConfiguration( firstLayer );
            }
            else
            {
                return finalConfiguration;
            }
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

        private void RaiseConfigurationChanged( ConfigurationChangedEventArgs e )
        {
            if( ConfigurationChanged != null )
            {
                ConfigurationChanged( this, e );
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

            public ConfigurationResult Add( ConfigurationLayer layer )
            {
                if( layer == null ) throw new ArgumentNullException( "layer" );

                ConfigurationResult result = CanAddLayer( layer );

                if( result )
                {
                    result = _parent.OnConfigurationManagerChanging( FinalConfigurationChange.LayerAdded, layer );
                    if( result )
                    {
                        if( _layers.Add( layer ) )
                        {
                            _parent.OnConfigurationManagerChanged( FinalConfigurationChange.LayerAdded, layer );
                            return result;
                        }
                        else 
                        {
                            return new ConfigurationResult( "A problem has been encountered while adding the ConfigurationLayer in the collection" );
                        }
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    return result;
                }
            }

            private ConfigurationResult CanAddLayer( ConfigurationLayer layer )
            {
                if( _layers.Count == 0 ) return new ConfigurationResult();
                bool exist;
                FinalConfigurationItem finalItem;
                foreach( ConfigurationItem item in layer.Items )
                {
                    finalItem = _parent._finalConfiguration.Items.GetByKey( item.ServiceOrPluginName, out exist );
                    if( exist )
                    {
                        if( !finalItem.CanChangeStatus( item.Status ) )
                        {
                            return new ConfigurationResult(
                                string.Format( "{0}({3}) est en conflit avec {1}({4}) contenu dans {2}",
                                        item.ServiceOrPluginName, finalItem.ServiceOrPluginName, layer.ConfigurationName, item.Status, finalItem.Status ) );
                        }
                    }
                }
                return new ConfigurationResult();
            }

            public ConfigurationResult Remove( ConfigurationLayer layer )
            {
                if( layer == null ) throw new ArgumentNullException( "configurationLayer" );

                if( _layers.Remove( layer ) )
                {
                    _parent.OnConfigurationManagerChanged( FinalConfigurationChange.LayerRemoved, layer );
                    return new ConfigurationResult();
                }
                else
                {
                    return new ConfigurationResult("The layer could not be removed because it could not be found");
                }
            }

            #region IEnumerable Members

            public IEnumerator GetEnumerator()
            {
                return _layers.GetEnumerator();
            }

            #endregion
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

        public bool IsCanceled
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
