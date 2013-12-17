using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using CK.Core;
using Yodii.Engine;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Host of most Yodii.Lab core collections.
    /// Handles lifecycle of mock services and plugins, and exposes methods to create and delete them.
    /// Handles item bindings.
    /// </summary>
    public class LabStateManager : IDiscoveredInfo, IYodiiEngineHost
    {
        #region Fields
        /// <summary>
        /// Collection of mock IServiceInfos.
        /// </summary>
        readonly CKObservableSortedArrayKeyList<ServiceInfo, string> _serviceInfos;

        /// <summary>
        /// Collection of mock IPluginInfos.
        /// </summary>
        readonly CKObservableSortedArrayKeyList<PluginInfo, string> _pluginInfos;

        /// <summary>
        /// Collection of lab service wrappers.
        /// </summary>
        readonly CKObservableSortedArrayKeyList<LabServiceInfo, ServiceInfo> _labServiceInfos;

        /// <summary>
        /// Collection of lab plugin wrappers.
        /// </summary>
        readonly CKObservableSortedArrayKeyList<LabPluginInfo, PluginInfo> _labPluginInfos;

        /// <summary>
        /// Collection of plugins running in this fake host.
        /// </summary>
        readonly ObservableCollection<IPluginInfo> _runningPlugins;

        /// <summary>
        /// Yodii engine to use. Created in constructor
        /// </summary>
        readonly IYodiiEngine _engine;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of this LabStateManager, as well as a new IYodiiEngine.
        /// </summary>
        internal LabStateManager()
        {
            YodiiEngine engine = new YodiiEngine( this );
            _engine = engine;
            engine.SetDiscoveredInfo( this );

            Debug.Assert( _engine.LiveInfo != null );
            _engine.PropertyChanged += _engine_PropertyChanged;
            _engine.LiveInfo.Plugins.CollectionChanged += Plugins_CollectionChanged;
            _engine.LiveInfo.Services.CollectionChanged += Services_CollectionChanged;

            Debug.Assert( _engine.IsRunning == false );

            _serviceInfos = new CKObservableSortedArrayKeyList<ServiceInfo, string>( s => s.ServiceFullName, false );
            _pluginInfos = new CKObservableSortedArrayKeyList<PluginInfo, string>( p => p.PluginFullName, false );

            _labServiceInfos = new CKObservableSortedArrayKeyList<LabServiceInfo, ServiceInfo>( s => s.ServiceInfo, ( x, y ) => String.CompareOrdinal( x.ServiceFullName, y.ServiceFullName ), false );
            _labPluginInfos = new CKObservableSortedArrayKeyList<LabPluginInfo, PluginInfo>( p => p.PluginInfo, ( x, y ) => String.CompareOrdinal( x.PluginFullName, y.PluginFullName ), false );

            _runningPlugins = new ObservableCollection<IPluginInfo>();
        }

        #endregion

        #region Event handlers
        void Plugins_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach( var i in e.NewItems )
                    {
                        ILivePluginInfo p = (ILivePluginInfo)i;
                        LookupAndBindLivePluginInfoToPlugin( p );
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach( var i in e.OldItems )
                    {
                        ILivePluginInfo p = (ILivePluginInfo)i;
                        LookupAndUnbindLivePluginInfoFromPlugin( p );
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    ClearLivePluginInfos();
                    break;
            }
        }

        void Services_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach( var i in e.NewItems )
                    {
                        ILiveServiceInfo s = (ILiveServiceInfo)i;
                        LookupAndBindLiveServiceInfoToService( s );
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach( var i in e.OldItems )
                    {
                        ILiveServiceInfo s = (ILiveServiceInfo)i;
                        LookupAndUnbindLiveServiceInfoFromService( s );
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    ClearLiveServiceInfos();
                    break;
            }
        }

        void _engine_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsRunning" && !_engine.IsRunning )
            {
                _runningPlugins.Clear();
            }
        }

        #endregion

        #region Properties
        /// <summary>
        /// Active IYodiiEngine.
        /// </summary>
        public IYodiiEngine Engine
        {
            get { return _engine; }
        }

        /// <summary>
        /// Mock IServiceInfos created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<ServiceInfo> ServiceInfos
        {
            get { return _serviceInfos; }
        }

        /// <summary>
        /// Mock IPluginInfos created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<PluginInfo> PluginInfos
        {
            get { return _pluginInfos; }
        }

        /// <summary>
        /// Wrappers of services created in this Lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<LabServiceInfo> LabServiceInfos
        {
            get { return _labServiceInfos; }
        }

        /// <summary>
        /// Wrappers of plugins reated in this lab.
        /// </summary>
        public ICKObservableReadOnlyCollection<LabPluginInfo> LabPluginInfos
        {
            get { return _labPluginInfos; }
        }

        /// <summary>
        /// Currently running plugins, in this fake host.
        /// </summary>
        public ObservableCollection<IPluginInfo> RunningPlugins
        {
            // TODO: Make it read-only
            get
            {
                return _runningPlugins;
            }
        }

        #region IDiscoveredInfo implementation
        IReadOnlyList<IServiceInfo> IDiscoveredInfo.ServiceInfos
        {
            get { return _serviceInfos; }
        }
        IReadOnlyList<IPluginInfo> IDiscoveredInfo.PluginInfos
        {
            get { return _pluginInfos; }
        }
        #endregion

        #endregion Properties

        #region Public methods
        /// <summary>
        /// Gets a descriptive string of given mock service or plugin info.
        /// </summary>
        /// <param name="serviceOrPluginInfo">ServiceInfo or PluginInfo. Will throw an ArgumentException on anything else.</param>
        /// <returns>Descriptive string of given service or plugin info.</returns>
        public static string GetDescriptionOfServiceOrPluginInfo( object serviceOrPluginInfo )
        {
            if( serviceOrPluginInfo == null ) return "No service or plugin given.";
            if( serviceOrPluginInfo is ServiceInfo )
            {
                ServiceInfo service = serviceOrPluginInfo as ServiceInfo;
                return String.Format( "Service: {0}", service.ServiceFullName );
            }
            else if( serviceOrPluginInfo is PluginInfo )
            {
                PluginInfo plugin = serviceOrPluginInfo as PluginInfo;
                return String.Format( "Plugin: {0}", plugin.PluginFullName );

            }
            else
            {
                throw new ArgumentException( "Parameter must be a ServiceInfo instance, a PluginInfo instance, or null.", "serviceOrPluginInfo" );
            }
        }

        /// <summary>
        /// Returns a descriptive of given service or plugin ID, provided it exists in our collections.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Plugin or service name.</param>
        /// <returns>Descriptive of given service or plugin ID</returns>
        public string GetDescriptionOfServiceOrPluginFullName( string serviceOrPluginFullName )
        {
            bool isPlugin = false;
            bool isService = false;

            var matchingPlugin = _pluginInfos.GetByKey( serviceOrPluginFullName, out isPlugin );
            var matchingService = _serviceInfos.GetByKey( serviceOrPluginFullName, out isService );

            if( isPlugin )
            {
                return String.Format( "Plugin: {0}", matchingPlugin.PluginFullName );
            }
            else if( isService )
            {
                return String.Format( "Service: {0}", matchingService.ServiceFullName );
            }

            return "Unknown plugin or service.";
        }

        #region IYodiiEngineHost Members

        IEnumerable<Tuple<IPluginInfo, Exception>> IYodiiEngineHost.Apply( IEnumerable<IPluginInfo> toDisable, IEnumerable<IPluginInfo> toStop, IEnumerable<IPluginInfo> toStart )
        {
            List<Tuple<IPluginInfo, Exception>> exceptionList = new List<Tuple<IPluginInfo, Exception>>();

            // TODO
            Console.WriteLine( "Disabling:" );
            foreach( var plugin in toDisable )
            {
                Console.WriteLine( String.Format( "- {0}", plugin.PluginFullName ) );
                if( _runningPlugins.Contains( plugin ) )
                {
                    _runningPlugins.Remove( plugin );
                }
            }

            Console.WriteLine( "Stopping:" );
            foreach( var plugin in toStop )
            {
                Console.WriteLine( String.Format( "- {0}", plugin.PluginFullName ) );
                if( _runningPlugins.Contains( plugin ) )
                {
                    _runningPlugins.Remove( plugin );
                }
            }

            Console.WriteLine( "Starting:" );
            foreach( var plugin in toStart )
            {
                Console.WriteLine( String.Format( "- {0}", plugin.PluginFullName ) );
                if( !_runningPlugins.Contains( plugin ) )
                {
                    _runningPlugins.Add( plugin );
                }
            }

            return exceptionList;
        }

        #endregion

        #endregion

        #region Internal methods
        /// <summary>
        /// Stops and clears everything in this lab, including configuration manager entries.
        /// </summary>
        internal void ClearState()
        {
            if( _engine.IsRunning )
            {
                _engine.Stop();
            }

            _labPluginInfos.Clear();
            _labServiceInfos.Clear();
            _pluginInfos.Clear();
            _serviceInfos.Clear();

            // Clear configuration manager
            foreach( var l in Engine.Configuration.Layers.ToList() )
            {
                var result = Engine.Configuration.Layers.Remove( l );
                Debug.Assert( result.Success );
            }
        }

        internal bool IsService( string serviceFullName )
        {
            return _serviceInfos.Contains( serviceFullName );
        }

        internal bool IsPlugin( string pluginFullName )
        {
            return _pluginInfos.Contains( pluginFullName );
        }

        /// <summary>
        /// Creates a new named service, which specializes another service.
        /// </summary>
        /// <param name="serviceName">Name of the new service</param>
        /// <param name="generalization">Specialized service</param>
        /// <returns>New service</returns>
        /// <remarks>Cannot be used when engine is running.</remarks>
        internal ServiceInfo CreateNewService( string serviceName, ServiceInfo generalization = null )
        {
            if( _engine.IsRunning )
            {
                throw new InvalidOperationException( "Cannot create Service while Engine is running." );
            }
            Debug.Assert( serviceName != null );
            Debug.Assert( _serviceInfos.Any( x => x.ServiceFullName == serviceName ) == false, "Service does not exist and can be added" );

            if( generalization != null ) Debug.Assert( ServiceInfos.Contains( generalization ) );

            ServiceInfo newService = new ServiceInfo( serviceName, AssemblyInfoHelper.ExecutingAssemblyInfo, generalization );

            _serviceInfos.Add( newService ); // Throws on duplicate

            CreateLabService( newService );

            return newService;
        }

        /// <summary>
        /// Creates a new named plugin, which implements an existing service.
        /// </summary>
        /// <param name="pluginName">Name of the new plugin</param>
        /// <param name="service">Implemented service</param>
        /// <returns>New plugin</returns>
        internal PluginInfo CreateNewPlugin( string pluginName, ServiceInfo service = null )
        {
            Debug.Assert( !String.IsNullOrWhiteSpace( pluginName ) );

            if( _engine.IsRunning )
            {
                throw new InvalidOperationException( "Cannot create Plugin while Engine is running." );
            }

            if( service != null ) Debug.Assert( ServiceInfos.Contains( service ) );

            PluginInfo plugin = new PluginInfo( pluginName, AssemblyInfoHelper.ExecutingAssemblyInfo, service );

            _pluginInfos.Add( plugin ); // Throws on duplicate

            CreateLabPlugin( plugin );

            return plugin;
        }

        /// <summary>
        /// Set an existing plugin's dependency to an existing service.
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="service">Service the plugin depends on</param>
        /// <param name="runningRequirement">How the plugin depends on the service</param>
        internal void SetPluginDependency( PluginInfo plugin, ServiceInfo service, DependencyRequirement runningRequirement )
        {
            if( _engine.IsRunning )
            {
                throw new InvalidOperationException( "Cannot create reference while Engine is running." );
            }
            Debug.Assert( plugin != null );
            Debug.Assert( service != null );
            Debug.Assert( ServiceInfos.Contains( service ) );
            Debug.Assert( PluginInfos.Contains( plugin ) );

            MockServiceReferenceInfo reference = new MockServiceReferenceInfo( plugin, service, DependencyRequirement.Running );
            plugin.InternalServiceReferences.Add( reference );

        }

        /// <summary>
        /// Removes a mock service info from our collections.
        /// </summary>
        /// <param name="serviceInfo">Mock service info to remove</param>
        internal void RemoveService( ServiceInfo serviceInfo )
        {
            if( _engine.IsRunning )
            {
                throw new InvalidOperationException( "Cannot remove Service while Engine is running." );
            }
            // If we delete a service : Unbind linked plugins and services.

            // Unbind generalized services
            foreach( ServiceInfo s in ServiceInfos.Where( si => si.Generalization == serviceInfo ).ToList() )
            {
                s.Generalization = null;
            }

            // Unbind implementations
            foreach( PluginInfo p in PluginInfos.Where( pi => pi.Service == serviceInfo ).ToList() )
            {
                p.Service = null;
            }

            // Delete all existing service references

            foreach( PluginInfo p in PluginInfos )
            {
                foreach( MockServiceReferenceInfo reference in p.InternalServiceReferences.Where( r => r.Reference == serviceInfo ).ToList() )
                {
                    p.InternalServiceReferences.Remove( reference );
                }
            }

            _labServiceInfos.Remove( serviceInfo );
            _serviceInfos.Remove( serviceInfo );
        }

        /// <summary>
        /// Removes a mock plugin info from our collections.
        /// </summary>
        /// <param name="pluginInfo">Mock plugin info to remove</param>
        internal void RemovePlugin( PluginInfo pluginInfo )
        {
            if( _engine.IsRunning )
            {
                throw new InvalidOperationException( "Cannot remove Plugin while Engine is running." );
            }

            if( pluginInfo.Service != null )
            {
                pluginInfo.InternalService.InternalImplementations.Remove( pluginInfo );
            }

            LabPluginInfo livePlugin = _labPluginInfos.GetByKey( pluginInfo );
            _labPluginInfos.Remove( livePlugin );
            _pluginInfos.Remove( pluginInfo );
        }

        /// <summary>
        /// Attempts to change one of our services' ServiceFullName.
        /// </summary>
        /// <param name="serviceInfo">ServiceInfo to rename</param>
        /// <param name="newName">New name</param>
        /// <returns>DetailedOperationResult on renaming attempt.</returns>
        /// <remarks>
        /// Can fail: Two services cannot coexist with the same name.
        /// </remarks>
        internal Utils.DetailedOperationResult RenameService( ServiceInfo serviceInfo, string newName )
        {
            if( _engine.IsRunning )
            {
                throw new InvalidOperationException( "Cannot rename Service while Engine is running." );
            }

            bool exists;
            ServiceInfo existingInfo = _serviceInfos.GetByKey( newName, out exists );

            if( exists ) return new Utils.DetailedOperationResult( false, "A service with this name already exists." );

            serviceInfo.ServiceFullName = newName;

            return new Utils.DetailedOperationResult( true );
        }

        #endregion Internal methods

        #region Private methods
        /// <summary>
        /// Creates a lab wrapper item around an existing mock service, and adds it to our collection.
        /// </summary>
        /// <param name="s">Existing mock service</param>
        private void CreateLabService( ServiceInfo s )
        {
            LabServiceInfo newServiceInfo;
            if( s.Generalization != null )
            {
                LabServiceInfo generalizationLiveInfo = _labServiceInfos.GetByKey( (ServiceInfo)s.Generalization );
                newServiceInfo = new LabServiceInfo( s ); // TODO: Running requirement
            }
            else
            {
                newServiceInfo = new LabServiceInfo( s );
            }
            _labServiceInfos.Add( newServiceInfo );
        }

        /// <summary>
        /// Creates a lab wrapper item around an existing mock plugin, and adds it to our collection.
        /// </summary>
        /// <param name="p">Existing mock plugin</param>
        private void CreateLabPlugin( PluginInfo p )
        {
            LabPluginInfo lp;

            if( p.Service != null )
            {
                p.Service.InternalImplementations.Add( p );
                LabServiceInfo liveService = _labServiceInfos.GetByKey( p.Service );
                lp = new LabPluginInfo( p );
            }
            else
            {
                lp = new LabPluginInfo( p );
            }

            _labPluginInfos.Add( lp );
        }

        /// <summary>
        /// Loads a foreign mock service info and all it depends on into our collections.
        /// </summary>
        /// <param name="serviceInfo">Foreign mock service info</param>
        internal void LoadServiceInfo( ServiceInfo serviceInfo )
        {
            if( _serviceInfos.Contains( serviceInfo ) ) return; // Already loaded
            _serviceInfos.Add( serviceInfo );

            if( serviceInfo.Generalization != null )
            {
                LoadServiceInfo( (ServiceInfo)serviceInfo.Generalization );
            }

            CreateLabService( serviceInfo );
        }

        /// <summary>
        /// Loads a foreign mock plugin info and al it depends on into our collections.
        /// </summary>
        /// <param name="pluginInfo">Foreign mock plugin info</param>
        internal void LoadPluginInfo( PluginInfo pluginInfo )
        {
            if( _pluginInfos.Contains( pluginInfo ) ) return; // Already loaded
            _pluginInfos.Add( pluginInfo );

            if( pluginInfo.Service != null )
            {
                LoadServiceInfo( pluginInfo.Service );
            }

            foreach( var serviceRef in pluginInfo.ServiceReferences )
            {
                Debug.Assert( serviceRef.Owner == pluginInfo );
                LoadServiceInfo( (ServiceInfo)serviceRef.Reference );
            }

            CreateLabPlugin( pluginInfo );
        }

        private void ClearLiveServiceInfos()
        {
            foreach( LabServiceInfo labService in _labServiceInfos )
            {
                labService.LiveServiceInfo = null;
            }
        }

        private void ClearLivePluginInfos()
        {
            foreach( LabPluginInfo labPlugin in _labPluginInfos )
            {
                labPlugin.LivePluginInfo = null;
            }
        }

        /// <summary>
        /// Binds all live infos from the engine in our services and plugins.
        /// </summary>
        private void LoadLiveInfoFromEngine()
        {
            foreach( ILiveServiceInfo liveService in _engine.LiveInfo.Services )
            {
                LookupAndBindLiveServiceInfoToService( liveService );
            }
            foreach( ILivePluginInfo livePlugin in _engine.LiveInfo.Plugins )
            {
                LookupAndBindLivePluginInfoToPlugin( livePlugin );
            }
        }

        private void LookupAndBindLiveServiceInfoToService( ILiveServiceInfo info )
        {
            Debug.Assert( info.ServiceInfo is ServiceInfo );

            LabServiceInfo labService = _labServiceInfos.GetByKey( (ServiceInfo)info.ServiceInfo );
            labService.LiveServiceInfo = info;
        }

        private void LookupAndUnbindLiveServiceInfoFromService( ILiveServiceInfo info )
        {
            Debug.Assert( info.ServiceInfo is ServiceInfo );

            LabServiceInfo labService = _labServiceInfos.GetByKey( (ServiceInfo)info.ServiceInfo );
            labService.LiveServiceInfo = null;
        }

        private void LookupAndBindLivePluginInfoToPlugin( ILivePluginInfo info )
        {
            Debug.Assert( info.PluginInfo is PluginInfo );

            LabPluginInfo labPlugin = _labPluginInfos.GetByKey( (PluginInfo)info.PluginInfo );
            labPlugin.LivePluginInfo = info;
        }

        private void LookupAndUnbindLivePluginInfoFromPlugin( ILivePluginInfo info )
        {
            Debug.Assert( info.PluginInfo is PluginInfo );

            LabPluginInfo labPlugin = _labPluginInfos.GetByKey( (PluginInfo)info.PluginInfo );
            labPlugin.LivePluginInfo = null;
        }
        #endregion

    }
}
