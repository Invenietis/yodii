#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\Live\DelayedPropertyNotification.cs) is part of CiviKey. 
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
