using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CK.Core;

namespace Yodii.Engine
{
    interface INotifyRaisePropertyChanged
    {
        void RaisePropertyChanged( string propertyName );
    }


    class DelayedPropertyNotification
    {
        List<Tuple<INotifyRaisePropertyChanged, string>> _serviceNotify;
        bool _silent;

        public DelayedPropertyNotification()
        {
            _serviceNotify = new List<Tuple<INotifyRaisePropertyChanged, string>>();
        }

        public IDisposable SilentMode()
        { 
            _silent = true;
            return Util.CreateDisposableAction( () => _silent = false );
        }

        public void Update<T>( INotifyRaisePropertyChanged obj, ref T field, T newValue, Expression<Func<T>> property )
        {
            if( (field == null && newValue != null) || (field != null && !field.Equals( newValue )) )
            {
                field = newValue;
                Notify( obj, property );
            }
        }

        public void Notify<T>( INotifyRaisePropertyChanged obj, Expression<Func<T>> property )
        {
            if( !_silent ) _serviceNotify.Add( Tuple.Create( obj, ((MemberExpression)property.Body).Member.Name ) );
        }

        public void RaiseEvents()
        {
            foreach( var ev in _serviceNotify ) ev.Item1.RaisePropertyChanged( ev.Item2 );
            _serviceNotify.Clear();
        }
    }
}
