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
    //Todo : augmenter la précision de la résolution du layer system
    class ConfigurationManager : IConfigurationManager
    {
        readonly YodiiEngine _engine;
        readonly ConfigurationLayerCollection _configurationLayerCollection;
        FinalConfiguration _finalConfiguration;

        ConfigurationChangingEventArgs _currentEventArgs;

        public event EventHandler<ConfigurationChangingEventArgs> ConfigurationChanging;
        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        public IConfigurationLayerCollection Layers
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

        internal ConfigurationManager( YodiiEngine engine )
        {
            _engine = engine;
            _configurationLayerCollection = new ConfigurationLayerCollection( this );
        }

        //private FinalConfiguration ResolveBasicConfiguration(ConfigurationLayer firstLayer)
        //{
        //    Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();
        //    foreach( ConfigurationItem item in firstLayer.Items )
        //    {
        //        final.Add( item.ServiceOrPluginId, item.Status );
        //    }

        //    _currentEventArgs = new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.LayerAdded, firstLayer );
        //    RaiseConfigurationChanging( _currentEventArgs );

        //    if( _currentEventArgs.IsCanceled )
        //    {
        //        firstLayer.Items.Remove( firstLayer.Items.Last().ServiceOrPluginId );
        //        return ResolveBasicConfiguration( firstLayer );
        //    }
        //    else
        //    {
        //        return _currentEventArgs.FinalConfiguration;
        //    }
        //}

        internal void OnLayerNameChanged( ConfigurationLayer layer )
        {
            _configurationLayerCollection.CheckPosition( layer );
        }

        ConfigurationFailureResult FillFromConfiguration( Dictionary<string, ConfigurationStatus> final, Func<ConfigurationItem, bool> filter = null )
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
                            if( status == ConfigurationStatus.Optional || (status == ConfigurationStatus.Runnable && item.Status == ConfigurationStatus.Running) )
                            {
                                final[item.ServiceOrPluginId] = item.Status;
                            }
                            else if( status != item.Status )
                            {
                                return new ConfigurationFailureResult( String.Format( "Conflict for {0} between statuses {1} and {2}", item.ServiceOrPluginId, item.Status, status ) );
                            }
                        }
                        else
                        {
                            final.Add( item.ServiceOrPluginId, item.Status );
                        }
                    }
                }
            }
            return new ConfigurationFailureResult();
        }

        internal IYodiiEngineResult OnConfigurationItemChanging( ConfigurationItem item, ConfigurationStatus newStatus )
        {
            Debug.Assert( item != null && _finalConfiguration != null && _configurationLayerCollection.Count != 0 );
            if( _currentEventArgs != null ) throw new InvalidOperationException( "Another change is in progress" );

            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();
            final.Add( item.ServiceOrPluginId, newStatus );

            ConfigurationFailureResult internalResult = FillFromConfiguration( final, c => c != item );
            if( internalResult.Success )
            {
                FinalConfiguration finalConfiguration = new FinalConfiguration( final );
                IYodiiEngineResult result = _engine.StaticResolution( finalConfiguration );
                if( result.Success )
                {
                    _currentEventArgs = new ConfigurationChangingEventArgs( finalConfiguration, FinalConfigurationChange.StatusChanged, item );
                    RaiseConfigurationChanging( _currentEventArgs );

                    if( _currentEventArgs.IsCanceled )
                    {
                        return new YodiiEngineResult( new ConfigurationFailureResult( _currentEventArgs.FailureExternalReasons ) );
                    }
                    return _engine.DynamicResolution();
                }
                return result;
            }
            internalResult.addFailureReason( String.Format( "The status of {0} cannot be changed", item.ServiceOrPluginId ) );
            return new YodiiEngineResult( internalResult );
        }

        internal IYodiiEngineResult OnConfigurationItemAdding( ConfigurationItem newItem )
        {
            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();
            final.Add( newItem.ServiceOrPluginId, newItem.Status );

            ConfigurationFailureResult internalResult = FillFromConfiguration( final );
            if( internalResult.Success )
            {
                FinalConfiguration finalConfiguration = new FinalConfiguration( final );
                IYodiiEngineResult result = _engine.StaticResolution( finalConfiguration );
                if( result.Success )
                {
                    _currentEventArgs = new ConfigurationChangingEventArgs( finalConfiguration, FinalConfigurationChange.ItemAdded, newItem );
                    RaiseConfigurationChanging( _currentEventArgs );

                    if( _currentEventArgs.IsCanceled )
                    {
                        return new YodiiEngineResult( new ConfigurationFailureResult( _currentEventArgs.FailureExternalReasons ) );
                    }
                    return _engine.DynamicResolution();
                }
                return result;
            }
            internalResult.addFailureReason( String.Format( "{0} cannot be added", newItem.ServiceOrPluginId ) );
            return new YodiiEngineResult( internalResult );
        }

        internal IYodiiEngineResult OnConfigurationItemRemoving( ConfigurationItem item )
        {
            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();

            ConfigurationFailureResult internalResult = FillFromConfiguration( final, c => c != item );
            if( internalResult.Success )
            {
                FinalConfiguration finalConfiguration = new FinalConfiguration( final );
                IYodiiEngineResult result = _engine.StaticResolution( finalConfiguration );
                if( result.Success )
                {
                    _currentEventArgs = new ConfigurationChangingEventArgs( finalConfiguration, FinalConfigurationChange.ItemRemoved, item );
                    RaiseConfigurationChanging( _currentEventArgs );

                    if( _currentEventArgs.IsCanceled )
                    {
                        return new YodiiEngineResult( new ConfigurationFailureResult( _currentEventArgs.FailureExternalReasons ) );
                    }
                    return _engine.DynamicResolution();
                }
                return result;
            }
            Debug.Fail( "Removing a configuration item can not lead to an impossibility." );
            internalResult.addFailureReason( "Removing a configuration item can not lead to an impossibility." );
            return new YodiiEngineResult( internalResult );
        }

        internal IYodiiEngineResult OnConfigurationLayerAdding( ConfigurationLayer layer )
        {
            //if( _finalConfiguration == null )
            //{
            //    FinalConfiguration = ResolveBasicConfiguration( layer );
            //    return new ConfigurationResult();
            //}

            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();
            foreach( ConfigurationItem item in layer.Items )
            {
                final.Add( item.ServiceOrPluginId, item.Status );
            }

            ConfigurationFailureResult internalResult = FillFromConfiguration( final );

            if( internalResult.Success )
            {
                FinalConfiguration finalConfiguration = new FinalConfiguration( final );
                IYodiiEngineResult result = _engine.StaticResolution( finalConfiguration );
                if( result.Success )
                {
                    _currentEventArgs = new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.LayerAdded, layer );
                    RaiseConfigurationChanging( _currentEventArgs );

                    if( _currentEventArgs.IsCanceled )
                    {
                        return new YodiiEngineResult( new ConfigurationFailureResult( _currentEventArgs.FailureExternalReasons ) );
                    }
                    return _engine.DynamicResolution();
                }
                return result;
            }
            internalResult.addFailureReason( String.Format( "{0} cannot be added", layer.LayerName ) );
            return new YodiiEngineResult( internalResult );
        }

        internal IYodiiEngineResult OnConfigurationLayerRemoving( ConfigurationLayer layer )
        {
            Dictionary<string,ConfigurationStatus> final = new Dictionary<string, ConfigurationStatus>();

            ConfigurationFailureResult internalResult = FillFromConfiguration( final, c => c.Layer != layer );

            if( internalResult.Success )
            {
                FinalConfiguration finalConfiguration = new FinalConfiguration( final );
                IYodiiEngineResult result = _engine.StaticResolution( finalConfiguration );
                if( result.Success )
                {
                    _currentEventArgs = new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.LayerRemoved, layer );
                    RaiseConfigurationChanging( _currentEventArgs );


                    if( _currentEventArgs.IsCanceled )
                    {
                        return new YodiiEngineResult( new ConfigurationFailureResult( _currentEventArgs.FailureExternalReasons ) );
                    }
                    return _engine.DynamicResolution();
                }
                return new SuccessYodiiEngineResult();
            }
            Debug.Fail( "Removing a configuration layer can not lead to an impossibility." );
            internalResult.addFailureReason( "Removing a configuration layer can not lead to an impossibility." );
            return new YodiiEngineResult( internalResult );
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

        public class ConfigurationLayerCollection : IConfigurationLayerCollection
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

            public IYodiiEngineResult Add( ConfigurationLayer layer )
            {
                if( layer == null ) throw new ArgumentNullException( "layer" );

                if( layer.ConfigurationManager == _parent ) return new SuccessYodiiEngineResult();
                else if( layer.ConfigurationManager != null ) return new YodiiEngineResult( new ConfigurationFailureResult("A ConfigurationManager already contains this layer") );

                IYodiiEngineResult result = _parent.OnConfigurationLayerAdding( layer );

                if( result.Success )
                {
                    if( _layers.Add( layer ) )
                    {
                        layer.ConfigurationManager = _parent;
                        _parent.OnConfigurationChanged();
                        return result;
                    }
                    else
                    {
                        return new YodiiEngineResult( new ConfigurationFailureResult("A problem has been encountered while adding the ConfigurationLayer in the collection" ) );
                    }
                }
                return result;
            }

            public IYodiiEngineResult Remove( ConfigurationLayer layer )
            {
                if( layer == null ) throw new ArgumentNullException( "configurationLayer" );

                IYodiiEngineResult result = _parent.OnConfigurationLayerRemoving( layer );
                if( result.Success )
                {
                    if( _layers.Remove( layer ) )
                    {
                        layer.ConfigurationManager = null;
                        _parent.OnConfigurationChanged();
                        return result;
                    }
                    else
                    {
                        return new YodiiEngineResult( new ConfigurationFailureResult( "The layer could not be removed because it could not be found" ) );
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

}
