#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\Service\ServiceProxyBase.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Yodii.Model;
using System.Reflection;
using System.Linq;
using CK.Core;

namespace Yodii.Host
{
    internal struct MEntry
    {
        public MethodInfo Method;
        public ServiceLogMethodOptions LogOptions;
    }

    internal struct EEntry
    {
        public EventInfo Event;
        public ServiceLogEventOptions LogOptions;
    }

    internal abstract class ServiceProxyBase : IServiceUntyped, IYodiiService
	{
        readonly Type _typeInterface;
        readonly MEntry[] _mRefs;
        readonly EEntry[] _eRefs;
        PluginProxyBase _impl;
        object _unavailableImpl;
        ServiceHost _serviceHost;
        ServiceStatus _status;
        public bool IsExternalService;

		protected ServiceProxyBase( object unavailableImpl, Type typeInterface, IList<MethodInfo> mRefs, IList<EventInfo> eRefs )
		{
            Debug.Assert( mRefs.All( r => r != null ) && mRefs.Distinct().SequenceEqual( mRefs ) );
            _typeInterface = typeInterface;
            RawImpl = _unavailableImpl = unavailableImpl;
            _mRefs = new MEntry[mRefs.Count];
            for( int i = 0; i < mRefs.Count; i++ )
            {
                _mRefs[i].Method = mRefs[i];
            }
            _eRefs = new EEntry[eRefs.Count];
            for( int i = 0; i < eRefs.Count; i++ )
            {
                _eRefs[i].Event = eRefs[i];
            }
            _status = ServiceStatus.Stopped;
        }

        internal void Initialize( ServiceHost serviceHost, bool isExternalService )
        {
            _serviceHost = serviceHost;
            IsExternalService = isExternalService;
            if( isExternalService ) _status = ServiceStatus.Started;
        }

        internal MEntry[] MethodEntries
        {
            get { return _mRefs; }
        }
        
        internal EEntry[] EventEntries
        {
            get { return _eRefs; }
        }

        class Event : ServiceStatusChangedEventArgs
        {
            readonly Action<Action<IYodiiEngineExternal>> _postStart;
            readonly ServiceProxyBase _service;
            readonly PluginProxyBase _originalImpl;
            readonly PluginProxyBase _swappingPlugin;

            public Event( ServiceProxyBase s, PluginProxyBase swappingPlugin, Action<Action<IYodiiEngineExternal>> postStartActionsCollector )
            {
                Debug.Assert( s._status != ServiceStatus.StoppingSwapped || swappingPlugin != null, "Swapping ==> swappingPlugin != null" );
                _service = s;
                _originalImpl = s._impl;
                _swappingPlugin = swappingPlugin;
                _postStart = postStartActionsCollector;
            }

            public override bool IsEngineStopping
            {
                get { return _postStart == null; }
            }

            public override bool IsSwapping
            {
                get { return _service._status.IsSwapping(); }
            }

            public override void BindToSwappedPlugin()
            {
                if( !IsSwapping ) throw new InvalidOperationException( R.BindToSwappedPluginMustBeSwapping );
                _service._impl = _swappingPlugin;
            }

            internal void RestoreImplementation()
            {
                if( _swappingPlugin != null ) _service._impl = _originalImpl;
            }

            public override void TryStart<T>( IService<T> service, StartDependencyImpact impact, Action onSuccess, Action<IYodiiEngineResult> onError )
            {
                TryStart( service.Service.GetType().FullName, impact, onSuccess, onError );
            }

            public override void TryStart( string serviceOrPluginFullName, StartDependencyImpact impact, Action onSuccess, Action<IYodiiEngineResult> onError )
            {
                if( _postStart == null ) return;
                Action<IYodiiEngineExternal> a = e => 
                {
                    if( e.IsRunning )
                    {
                        ILiveYodiiItem item = e.LiveInfo.FindYodiiItem( serviceOrPluginFullName );
                        if( item != null && item.Capability.CanStartWith( impact ) )
                        {
                            var r = e.StartItem( item, impact );
                            if( r.Success )
                            {
                                if( onSuccess != null ) onSuccess();
                            }
                            else
                            {
                                if( onError != null ) onError( r );
                            }
                        }
                    }
                };
                _postStart( a );
            }
        }

        /// <summary>
        /// Explicit implementation here: the public Service Property that will be generated 
        /// will return the T for IService{T}, IOptionalService{T}, etc...
        /// </summary>
        IYodiiService IServiceUntyped.Service { get { return this; } }

		public event EventHandler<ServiceStatusChangedEventArgs> ServiceStatusChanged;

		public ServiceStatus Status
		{
            get { return _status; }
            internal set { _status = value; }
		}

        internal void RaiseStatusChanged( Action<Action<IYodiiEngineExternal>> postStartActionsCollector, PluginProxyBase swappingPlugin = null )
		{
            var h = ServiceStatusChanged;
            if( h != null )
			{
                Event ev = new Event( this, swappingPlugin, postStartActionsCollector );
                foreach( EventHandler<ServiceStatusChangedEventArgs> f in h.GetInvocationList() )
                {
                    f( this, ev );
                    ev.RestoreImplementation();
                }
			}
		}

		protected abstract object RawImpl { get; set; }

        internal PluginProxyBase Implementation { get { return _impl; } }

        /// <summary>
        /// Currently, injection of external services must be totally independent of
        /// any Dynamic services: a Service is either a dynamic one, implemented by one (or more) plugin, 
        /// or an external one that is considered to be persistent and always available and started.
        /// </summary>
        /// <param name="implementation">Plugin implementation.</param>
        public void SetExternalImplementation( object implementation )
        {
            if( !IsExternalService ) throw new CKException( R.ServiceIsPluginBased, _typeInterface );
            if( implementation == null ) implementation = _unavailableImpl;
            if( implementation != RawImpl )
            {
                RawImpl = implementation;
            }
        }

        internal void SetPluginImplementation( PluginProxyBase implementation )
        {
            if( IsExternalService ) throw new CKException( R.ServiceIsAlreadyExternal, _typeInterface, implementation.GetType().AssemblyQualifiedName );
            _impl = implementation;
            if( _impl == null )
            {
                RawImpl = _unavailableImpl;
            }
            else RawImpl = _impl.RealPluginObject;
        }

        #region Protected methods called by concrete concrete dynamic classes (event relaying).

        /// <summary>
        /// This method is called whenever a method not marked with <see cref="IgnoreServiceStoppedAttribute"/>
        /// is called. It throws a <see cref="ServiceStoppedException"/> if the service is stopped or a <see cref="ServiceNotAvailableException"/> for a disabled one.
        /// It also checks that the call to any service is allowed  (ServiceHost.CallServiceBlocker).
        /// Otherwise it returns the appropriate log configuration.
        /// </summary>
        /// <returns>The log configuration that must be used.</returns>
        [DebuggerNonUserCodeAttribute]
        protected ServiceLogMethodOptions GetLoggerForRunningCall( int iMethodMRef, out LogMethodEntry logger )
        {
            var blocker = _serviceHost.CallServiceBlocker;
            if( blocker != null ) throw blocker( _typeInterface );
            if( _impl == null || _impl.Status == PluginStatus.Null )
            {
                throw new ServiceNotAvailableException( _typeInterface );
            }
            if( _impl.Status == PluginStatus.Stopped )
            {
                throw new ServiceStoppedException( _typeInterface );
            }
            MEntry me = _mRefs[iMethodMRef];
            ServiceLogMethodOptions o = me.LogOptions;
            o &= ServiceLogMethodOptions.CreateEntryMask;
            logger = o == ServiceLogMethodOptions.None ? null : _serviceHost.LogMethodEnter( me.Method, o );
            return o;
        }

        /// <summary>
        /// Returns the appropriate log configuration after having checked that the dynamic service is not disabled
        /// (but it can be stopped). Checks that the call to any service is allowed  (ServiceHost.CallServiceBlocker).
        /// </summary>
        /// <returns>The log configuration that must be used.</returns>
        [DebuggerNonUserCodeAttribute]
        protected ServiceLogMethodOptions GetLoggerForNotDisabledCall( int iMethodMRef, out LogMethodEntry logger )
        {
            var blocker = _serviceHost.CallServiceBlocker;
            if( blocker != null ) throw blocker( _typeInterface );
            if( _impl == null || _impl.Status == PluginStatus.Null )
            {
                throw new ServiceNotAvailableException( _typeInterface );
            }
            MEntry me = _mRefs[iMethodMRef];
            ServiceLogMethodOptions o = me.LogOptions;
            o &= ServiceLogMethodOptions.CreateEntryMask;
            logger = o == ServiceLogMethodOptions.None ? null : _serviceHost.LogMethodEnter( me.Method, o );
            return o;
        }

        /// <summary>
        /// Returns the appropriate log configuration without any runtime status checks except that the call 
        /// to any service must be allowed (ServiceHost.CallServiceBlocker).
        /// </summary>
        /// <returns>The log configuration that must be used.</returns>
        [DebuggerNonUserCodeAttribute]
        protected ServiceLogMethodOptions GetLoggerForAnyCall( int iMethodMRef, out LogMethodEntry logger )
        {
            var blocker = _serviceHost.CallServiceBlocker;
            if( blocker != null ) throw blocker( _typeInterface );
            MEntry me = _mRefs[iMethodMRef];
            ServiceLogMethodOptions o = me.LogOptions;
            logger = o == ServiceLogMethodOptions.None ? null : _serviceHost.LogMethodEnter( me.Method, o );
            return o;
        }

        [DebuggerNonUserCodeAttribute]
        protected void LogEndCall( LogMethodEntry e )
        {
            Debug.Assert( e != null );
            _serviceHost.LogMethodSuccess( e );
        }

        [DebuggerNonUserCodeAttribute]
        protected void LogEndCallWithValue( LogMethodEntry e, object retValue )
        {
            Debug.Assert( e != null );
            e._returnValue = retValue;
            _serviceHost.LogMethodSuccess( e );
        }

        [DebuggerNonUserCodeAttribute]
        protected void OnCallException( int iMethodMRef, Exception ex, LogMethodEntry e )
        {
            if( e != null )
            {
                _serviceHost.LogMethodError( e, ex );
            }
            else
            {
                MEntry me = _mRefs[iMethodMRef];
                _serviceHost.LogMethodError( me.Method, ex );
            }
        }

        /// <summary>
        /// This method is called whenever an event not marked with <see cref="IgnoreServiceStoppedAttribute"/>
        /// is raised. If the service is actually running, it does nothing and returns true.
        /// If the service is stopped or disabled it throws a <see cref="ServiceStoppedException"/> or returns false
        /// if <see cref="ServiceLogEventOptions.SilentEventRunningStatusError"/> is set: the event will not be raised and no exceptions will be
        /// thrown back to the buggy service.
        /// </summary>
        [DebuggerNonUserCodeAttribute]
        protected bool GetLoggerEventForRunningCall( int iEventMRef, out LogEventEntry entry, out ServiceLogEventOptions logOptions )
        {
            EEntry e = _eRefs[iEventMRef];
            logOptions = e.LogOptions;
            bool isDisabled = _impl == null || _impl.Status == PluginStatus.Null;
            if( isDisabled || _impl.Status == PluginStatus.Stopped )
            {
                if( (logOptions & ServiceLogEventOptions.SilentEventRunningStatusError) != 0 )
                {
                    entry = null;
                    if( (logOptions & ServiceLogEventOptions.LogSilentEventRunningStatusError) != 0 )
                        _serviceHost.LogEventNotRunningError( e.Event, isDisabled );
                    return false;
                }
                if( isDisabled ) throw new ServiceNotAvailableException( _typeInterface );
                else throw new ServiceStoppedException( _typeInterface );
            }
            logOptions &= ServiceLogEventOptions.CreateEntryMask;
            entry = logOptions != 0 ? _serviceHost.LogEventEnter( e.Event, logOptions ) : null;   
            return true;
        }

        [DebuggerNonUserCodeAttribute]
        protected bool GetLoggerEventForNotDisabledCall( int iEventMRef, out LogEventEntry entry, out ServiceLogEventOptions logOptions )
        {
            EEntry e = _eRefs[iEventMRef];
            logOptions = e.LogOptions & ServiceLogEventOptions.CreateEntryMask;
            if( _impl == null || _impl.Status == PluginStatus.Null )
            {
                if( (logOptions & ServiceLogEventOptions.SilentEventRunningStatusError) != 0 )
                {
                    entry = null;
                    if( (logOptions & ServiceLogEventOptions.LogSilentEventRunningStatusError) != 0 )
                        _serviceHost.LogEventNotRunningError( e.Event, true );
                    return false;
                }
                throw new ServiceNotAvailableException( _typeInterface );
            }
            entry = logOptions != 0 ? _serviceHost.LogEventEnter( e.Event, logOptions ) : null;
            return true;
        }

        [DebuggerNonUserCodeAttribute]
        protected bool GetLoggerEventForAnyCall( int iEventMRef, out LogEventEntry entry, out ServiceLogEventOptions logOptions )
        {
            EEntry e = _eRefs[iEventMRef];
            logOptions = e.LogOptions & ServiceLogEventOptions.CreateEntryMask;
            entry = logOptions != 0 ? _serviceHost.LogEventEnter( e.Event, logOptions ) : null;
            return true;
        }

        [DebuggerNonUserCodeAttribute]
        protected void LogEndRaise( LogEventEntry e )
        {
            Debug.Assert( e != null );
            _serviceHost.LogEventEnd( e );
        }

        /// <summary>
        /// This method is called when an event subscriber raises an exception while receiving the notification.
        /// By returning true, this methods silently swallow the exception. By returning false, the event dispatching
        /// is stoppped (remaining subscribers will not receive the event) and the plugin receives the exception (this
        /// corresponds to the standard behavior).
        /// </summary>
        /// <param name="iEventMRef">The index of the event info.</param>
        /// <param name="target">The called method that raised the exception.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="ee">The log entry if it has been created. Will be created if needed.</param>
        /// <returns>True to silently swallow the exception.</returns>
        [DebuggerNonUserCodeAttribute]
        protected bool OnEventHandlingException( int iEventMRef, MethodInfo target, Exception ex, ref LogEventEntry ee )
		{
            EEntry e = _eRefs[iEventMRef];
            if( (e.LogOptions & ServiceLogEventOptions.LogErrors) != 0 )
            {
                if( ee != null )
                {
                    _serviceHost.LogEventError( ee, target, ex );
                }
                else
                {
                    ee = _serviceHost.LogEventError( e.Event, target, ex );
                }
            }
            return (e.LogOptions&ServiceLogEventOptions.SilentEventError) != 0;
        }

        #endregion
    }

}
