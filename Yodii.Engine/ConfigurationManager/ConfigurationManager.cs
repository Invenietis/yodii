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
using Yodii.Model;

namespace Yodii.Engine
{
    internal class ConfigurationManager : IConfigurationManager
    {
        readonly ConfigurationLayerCollection _configurationLayerCollection;
        FinalConfiguration _finalConfiguration;

        ConfigurationChangingEventArgs _currentEventArgs;

        public event EventHandler<ConfigurationChangingEventArgs> ConfigurationChanging;
        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        IConfigurationLayerCollection IConfigurationManager.Layers
        {
            get { return _configurationLayerCollection; }
        }

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
                RaisePropertyChanged();
            }
        }

        internal ConfigurationManager( YodiiEngine engine )
        {
            Engine = engine;
            _configurationLayerCollection = new ConfigurationLayerCollection( this );
            _finalConfiguration = new FinalConfiguration();
        }

        public readonly YodiiEngine Engine;

        internal void OnLayerNameChanged( IConfigurationLayer layer )
        {
            _configurationLayerCollection.CheckPosition( layer );
        }

        ConfigurationFailureResult FillFromConfiguration( string currentOperation, Dictionary<string, FinalConfigurationItem> final, Func<ConfigurationItem, bool> filter = null )
        {
            foreach( ConfigurationLayer layer in _configurationLayerCollection )
            {
                FinalConfigurationItem data;
                foreach( ConfigurationItem item in layer.Items )
                {
                    if( filter == null || filter( item ) )
                    {
                        if( final.TryGetValue( item.ServiceOrPluginFullName, out data ) )
                        {
                            if( data.Status == ConfigurationStatus.Optional || ( data.Status == ConfigurationStatus.Runnable && item.Status == ConfigurationStatus.Running ) )
                            {
                                final.Remove(item.ServiceOrPluginFullName);
                                final.Add( item.ServiceOrPluginFullName, new FinalConfigurationItem( item.ServiceOrPluginFullName, item.Status, item.Impact ) );
                            }
                            else if( data.Status != item.Status )
                            {
                                return new ConfigurationFailureResult( String.Format( "{0}: conflict for '{1}' between statuses '{2}' and '{3}'.", currentOperation, item.ServiceOrPluginFullName, item.Status, data ) );
                            }
                        }
                        else
                        {
                            final.Add( item.ServiceOrPluginFullName, new FinalConfigurationItem( item.ServiceOrPluginFullName, item.Status, item.Impact ) );
                        }
                    }
                }
            }
            return new ConfigurationFailureResult();
        }

        internal IYodiiEngineResult OnConfigurationItemChanging( ConfigurationItem item, FinalConfigurationItem data )
        {
            Debug.Assert( item != null && _finalConfiguration != null && _configurationLayerCollection.Count != 0 );
            if( _currentEventArgs != null ) throw new InvalidOperationException( "Another change is in progress" );

            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();
            final.Add( item.ServiceOrPluginFullName, data );

            ConfigurationFailureResult internalResult = FillFromConfiguration( "Item changing", final, c => c != item );
            if( !internalResult.Success ) return new YodiiEngineResult( internalResult, Engine);

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, FinalConfigurationChange.StatusChanged, item ) );
        }

        internal IYodiiEngineResult OnConfigurationItemAdding( ConfigurationItem newItem )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();
            final.Add( newItem.ServiceOrPluginFullName, new FinalConfigurationItem(newItem.ServiceOrPluginFullName, newItem.Status, newItem.Impact ));
          
            ConfigurationFailureResult internalResult = FillFromConfiguration( "Adding configuration item", final );
            if( !internalResult.Success ) return new YodiiEngineResult( internalResult, Engine );

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, FinalConfigurationChange.ItemAdded, newItem ) );
        }

        internal IYodiiEngineResult OnConfigurationItemRemoving( ConfigurationItem item )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();

            ConfigurationFailureResult internalResult = FillFromConfiguration( null, final, c => c != item );
            Debug.Assert( internalResult.Success, "Removing a configuration item can not lead to an impossibility." );

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, FinalConfigurationChange.ItemRemoved, item ) );
        }

        internal IYodiiEngineResult OnConfigurationLayerRemoving( ConfigurationLayer layer )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();

            ConfigurationFailureResult internalResult = FillFromConfiguration( null, final, c => c.Layer != layer );
            Debug.Assert( internalResult.Success, "Removing a configuration layer can not lead to an impossibility." );

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, FinalConfigurationChange.LayerRemoved, layer ) );
        }

        IYodiiEngineResult OnConfigurationChanging( Dictionary<string, FinalConfigurationItem> final, Func<FinalConfiguration, ConfigurationChangingEventArgs> createChangingEvent )
        {
            FinalConfiguration finalConfiguration = new FinalConfiguration( final );
            if( Engine.IsRunning )
            {
                Tuple<IYodiiEngineStaticOnlyResult,ConfigurationSolver> t = Engine.StaticResolutionByConfigurationManager( finalConfiguration );
                if( t.Item1 != null )
                {
                    Debug.Assert( !t.Item1.Success );
                    Debug.Assert( t.Item1.Engine == Engine );
                    return t.Item1;
                }
                return OnConfigurationChangingForExternalWorld( createChangingEvent( finalConfiguration ) ) ?? Engine.OnConfigurationChanging( t.Item2 );
            }
            return OnConfigurationChangingForExternalWorld( createChangingEvent( finalConfiguration ) ) ?? Engine.SuccessResult;
        }

        IYodiiEngineResult OnConfigurationChangingForExternalWorld( ConfigurationChangingEventArgs eventChanging )
        {
            _currentEventArgs = eventChanging;
            RaiseConfigurationChanging( _currentEventArgs );
            if( _currentEventArgs.IsCanceled )
            {
                return new YodiiEngineResult( new ConfigurationFailureResult( _currentEventArgs.FailureExternalReasons ), Engine );
            }
            return null;
        }


        internal void OnConfigurationChanged()
        {
            Debug.Assert( _currentEventArgs != null );

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

        private void RaisePropertyChanged( [CallerMemberName] String propertyName = "" )
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

        public class ConfigurationLayerCollection : IConfigurationLayerCollection
        {
            private CKObservableSortedArrayKeyList<ConfigurationLayer,string> _layers;
            private ConfigurationManager _parent;

            internal ConfigurationLayerCollection( ConfigurationManager parent )
            {
                _parent = parent;
                _layers = new CKObservableSortedArrayKeyList<ConfigurationLayer, string>( e => e.LayerName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ), allowDuplicates: true );

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

            public IConfigurationLayer Create( string layerName = null )
            {
                var layer = new ConfigurationLayer( _parent, layerName );
                _layers.Add( layer );
                return layer;
            }

            public IYodiiEngineResult Remove( IConfigurationLayer layer )
            {
                if( layer == null ) throw new ArgumentNullException( "layer" );
                if( layer.ConfigurationManager != _parent ) return _parent.Engine.SuccessResult;
                // When called by a hacker.
                ConfigurationLayer l = layer as ConfigurationLayer;
                if ( l == null ) throw new ArgumentException( "Invalid layer.", "layer" );

                Debug.Assert( _layers.Contains( layer ), "Since layer.ConfigurationManager == _parent, then it necessarily belongs to us." );

                IYodiiEngineResult result = _parent.OnConfigurationLayerRemoving( l );
                if( result.Success )
                {
                    _layers.Remove( l );
                    l.SetConfigurationManager( null );
                    _parent.OnConfigurationChanged();
                }
                return result;
            }

            internal void CheckPosition( IConfigurationLayer layer )
            {
                _layers.CheckPosition( _layers.IndexOf( layer ) );
            }

            public IConfigurationLayer this[string key]
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

            public IEnumerator<IConfigurationLayer> GetEnumerator()
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

            public event NotifyCollectionChangedEventHandler CollectionChanged;

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

            public IConfigurationLayer this[int index]
            {
                get { return _layers[index]; }
            }

            #endregion           
        

            IReadOnlyCollection<IConfigurationLayer> IConfigurationLayerCollection.this[string layerName]
            {
                get { return _layers.GetAllByKey( String.IsNullOrWhiteSpace( layerName ) ? String.Empty : layerName ); }
            }
        }

    }

}
