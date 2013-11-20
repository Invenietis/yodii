using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace Yodii.Model
{
    //Todo : augmenter la précision de la résolution du layer system
    public class ConfigurationManager : INotifyPropertyChanged
    {
        readonly ConfigurationLayerCollection _configurationLayerCollection;
        FinalConfiguration _finalConfiguration;

        ConfigurationChangingEventArgs _currentEventArgs;

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

        private FinalConfiguration ResolveBasicConfiguration(ConfigurationLayer firstLayer)
        {
            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();
            foreach( ConfigurationItem item in firstLayer.Items )
            {
                final.Add( item.ServiceOrPluginId, item.Status );
            }

            _currentEventArgs = new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.LayerAdded, firstLayer );
            RaiseConfigurationChanging( _currentEventArgs );

            if( _currentEventArgs.IsCanceled )
            {
                firstLayer.Items.Remove( firstLayer.Items.Last().ServiceOrPluginId );
                return ResolveBasicConfiguration( firstLayer );
            }
            else
            {
                return _currentEventArgs.FinalConfiguration;
            }
        }

        internal void OnLayerNameChanged( ConfigurationLayer layer)
        {
            _configurationLayerCollection.CheckPosition( layer );
        }

        ConfigurationResult FillFromConfiguration( Dictionary<string, ConfigurationStatus> final, Func<ConfigurationItem, bool> filter = null )
        {
            foreach( ConfigurationLayer layer in _configurationLayerCollection )
            {
                ConfigurationStatus status;
                foreach( ConfigurationItem item in layer.Items )
                {
                    if( filter == null || filter( item ) )
                    {
                        if( final.TryGetValue( item.ServiceOrPluginId, out status ) )
                        {
                            if( status == ConfigurationStatus.Optional || ( status == ConfigurationStatus.Runnable && item.Status == ConfigurationStatus.Running ) )
                            {
                                final[item.ServiceOrPluginId] = item.Status;
                            }
                            else if( status != item.Status )
                            {
                                return new ConfigurationResult( String.Format( "Conflict for {0} between statuses {1} and {2}", item.ServiceOrPluginId, item.Status, status ) );
                            }
                        }
                        else
                        {
                            final.Add( item.ServiceOrPluginId, item.Status );
                        }
                    }
                }
            }
            return new ConfigurationResult();
        }

        internal ConfigurationResult OnConfigurationItemChanging( ConfigurationItem item, ConfigurationStatus newStatus )
        {
            Debug.Assert( item != null && _finalConfiguration != null && _configurationLayerCollection.Count != 0 );
            //if( _currentEventArgs != null ) throw new InvalidOperationException( "Another change is in progress" );

            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();
            final.Add( item.ServiceOrPluginId, newStatus );

            ConfigurationResult result = FillFromConfiguration( final, c => c != item );
            if( result )
            {
                _currentEventArgs = new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.StatusChanged, item );
                RaiseConfigurationChanging( _currentEventArgs );

                if( _currentEventArgs.IsCanceled )
                {
                    foreach( string s in _currentEventArgs.Causes ) result.AddFailureCause( s );
                    return result;
                }

                return result;
            }
            result.AddFailureCause( String.Format("The status of {0} cannot be changed", item.ServiceOrPluginId) );
            return result ;
        }

        internal ConfigurationResult OnConfigurationItemAdding( ConfigurationItem newItem )
        {
            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();
            final.Add( newItem.ServiceOrPluginId, newItem.Status );

            ConfigurationResult result = FillFromConfiguration( final );
            if( result )
            {
                _currentEventArgs = new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.StatusChanged, newItem );
                RaiseConfigurationChanging( _currentEventArgs );

                if( _currentEventArgs.IsCanceled )
                {
                    foreach( string s in _currentEventArgs.Causes ) result.AddFailureCause( s );
                    return result;
                }
                return result;
            }
            result.AddFailureCause( String.Format( "{0} cannot be added", newItem.ServiceOrPluginId ) );
            return result;
        }

        internal ConfigurationResult OnConfigurationItemRemoving( ConfigurationItem item )
        {
            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();

            if( FillFromConfiguration( final, c => c != item ) )
            {
                _currentEventArgs = new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.StatusChanged, item );
                RaiseConfigurationChanging( _currentEventArgs );

                if( _currentEventArgs.IsCanceled )
                {
                    return new ConfigurationResult( _currentEventArgs.Causes );
                }
                return new ConfigurationResult();
            }
            Debug.Fail( "Removing a configuration item can not lead to an impossibility." );
            return new ConfigurationResult("Removing a configuration item can not lead to an impossibility.");
        }

        internal ConfigurationResult OnConfigurationLayerAdding( ConfigurationLayer layer )
        {
            if( _finalConfiguration == null )
            {
                FinalConfiguration = ResolveBasicConfiguration( layer );
                return new ConfigurationResult();
            }

            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();
            foreach( ConfigurationItem item in layer.Items )
            {
                final.Add( item.ServiceOrPluginId, item.Status );
            }

            ConfigurationResult result = FillFromConfiguration( final );

            if( result )
            {
                _currentEventArgs = new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.StatusChanged, layer );
                RaiseConfigurationChanging( _currentEventArgs );

                if( _currentEventArgs.IsCanceled )
                {
                    foreach( string s in _currentEventArgs.Causes ) result.AddFailureCause( s );
                    return result;
                }
                return result;
            }
            result.AddFailureCause( String.Format( "{0} cannot be added", layer.LayerName ) );
            return result;
        }

        internal ConfigurationResult OnConfigurationLayerRemoving( ConfigurationLayer layer )
        {
            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();

            if( FillFromConfiguration( final, c => c.Layer != layer ) )
            {
                _currentEventArgs = new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.StatusChanged, layer );
                RaiseConfigurationChanging( _currentEventArgs );

                if( _currentEventArgs.IsCanceled )
                {
                    return new ConfigurationResult( _currentEventArgs.Causes );
                }
                return new ConfigurationResult();
            }
            Debug.Fail( "Removing a configuration layer can not lead to an impossibility." );
            return new ConfigurationResult( "Removing a configuration item can not lead to an impossibility." );
        }

        internal void OnConfigurationChanged()
        {
            Debug.Assert( _currentEventArgs != null, "Another change is in progress." ); // Reeantrancy warning. If you break here, check that your event handlers are not doing another change at the same time.

            FinalConfiguration = _currentEventArgs.FinalConfiguration;
            if( _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.StatusChanged
                || _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.ItemAdded
                || _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.ItemRemoved )
            {
                RaiseConfigurationChanged( new ConfigurationChangedEventArgs( FinalConfiguration, _currentEventArgs.FinalConfigurationChange, _currentEventArgs.ConfigurationItemChanged ) );
            }
            else
            {
                RaiseConfigurationChanged( new ConfigurationChangedEventArgs( FinalConfiguration, _currentEventArgs.FinalConfigurationChange, _currentEventArgs.ConfigurationLayerChanged ) );
            }
            _currentEventArgs = null;
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
            var h = ConfigurationChanging;
            if( h != null ) h( this, e ); 
        }

        private void RaiseConfigurationChanged( ConfigurationChangedEventArgs e )
        {
            var h = ConfigurationChanged;
            if( h != null ) h( this, e );
        }

        public class ConfigurationLayerCollection : ICKObservableReadOnlyList<ConfigurationLayer>
        {
            private CKObservableSortedArrayKeyList<ConfigurationLayer,string> _layers;
            private ConfigurationManager _parent;

            internal ConfigurationLayerCollection( ConfigurationManager parent )
            {
                _parent = parent;
                _layers = new CKObservableSortedArrayKeyList<ConfigurationLayer, string>( e => e.LayerName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), true );

                _layers.PropertyChanged += RetrievePropertyEvent;
                _layers.CollectionChanged += RetrieveCollectionEvent;            
            }

            private void RetrieveCollectionEvent( object sender, NotifyCollectionChangedEventArgs e )
            {
                FireCollectionChanged( e );
            }

            private void RetrievePropertyEvent( object sender, PropertyChangedEventArgs e )
            {
                FirePropertyChanged( e );
            }

            public ConfigurationResult Add( ConfigurationLayer layer )
            {
                if( layer == null ) throw new ArgumentNullException( "layer" );

                if( layer.ConfigurationManager == _parent ) return new ConfigurationResult();
                else if( layer.ConfigurationManager != null ) return new ConfigurationResult( "A ConfigurationManager already contains this layer" );

                ConfigurationResult result = _parent.OnConfigurationLayerAdding( layer );

                if( result )
                {
                    if( _layers.Add( layer ) )
                    {
                        layer.ConfigurationManager = _parent;
                        _parent.OnConfigurationChanged();
                        return result;
                    }
                    else 
                    {
                        result.AddFailureCause( "A problem has been encountered while adding the ConfigurationLayer in the collection" );
                        return result;
                    }
                }
                else
                {
                    return result;
                }
            }

            public ConfigurationResult Remove( ConfigurationLayer layer )
            {
                if( layer == null ) throw new ArgumentNullException( "configurationLayer" );

                ConfigurationResult result = _parent.OnConfigurationLayerRemoving( layer );
                if( result )
                {
                    if( _layers.Remove( layer ) )
                    {
                        layer.ConfigurationManager = null;
                        _parent.OnConfigurationChanged();
                        return result;
                    }
                    else
                    {
                        result.AddFailureCause( "The layer could not be removed because it could not be found" );
                        return result;
                    }
                }
                return result;
            }

            internal void CheckPosition( ConfigurationLayer layer )
            {
                _layers.CheckPosition( _layers.IndexOf( layer ) );
            }

            public ConfigurationLayer this[string key]
            {
                get { return this._layers.GetByKey( key ); }
            }

            private void FirePropertyChanged( PropertyChangedEventArgs e )
            {
                var h = PropertyChanged;
                if( h != null ) h( this, e );
            }

            private void FireCollectionChanged( NotifyCollectionChangedEventArgs e )
            {
                var h = CollectionChanged;
                if( h != null ) h( this, e );
            }

            #region ICKReadOnlyCollection<ConfigurationLayer> Members

            public bool Contains( object item )
            {
                return _layers.Contains( item );
            }

            #endregion

            #region IReadOnlyCollection<ConfigurationLayer> Members

            public int Count
            {
                get { return _layers.Count; } 
            }

            #endregion

            #region IEnumerable<ConfigurationLayer> Members

            public IEnumerator<ConfigurationLayer> GetEnumerator()
            {
                return _layers.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _layers.GetEnumerator();
            }

            #endregion

            #region INotifyCollectionChanged Members

            public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

            #endregion

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            #region ICKReadOnlyList<ConfigurationLayer> Members

            public int IndexOf( object item )
            {
                return _layers.IndexOf( item );
            }

            #endregion

            #region IReadOnlyList<ConfigurationLayer> Members

            public ConfigurationLayer this[int index]
            {
                get { return _layers[index]; }
            }

            #endregion
        }
        
    }

    public class ConfigurationChangingEventArgs : EventArgs
    {
        private bool _isCanceled;
        private List<string> _causes;
        private FinalConfiguration _finalConfiguration;
        private FinalConfigurationChange _finalConfigurationChange;
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

        public FinalConfigurationChange FinalConfigurationChange
        {
            get { return _finalConfigurationChange; }
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
            _finalConfigurationChange = finalConfigurationChanged;
            _configurationItemChanged = configurationItem;
        }

        internal ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, ConfigurationLayer configurationLayer )
        {
            _isCanceled = false;
            _causes = new List<string>();
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChange = finalConfigurationChanged;
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
