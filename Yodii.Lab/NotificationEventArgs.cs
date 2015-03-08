#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\NotificationEventArgs.cs) is part of CiviKey. 
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

namespace Yodii.Lab
{
    /// <summary>
    /// Event args for a new Notification.
    /// </summary>
    internal class NotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Notification to display.
        /// </summary>
        public readonly Notification Notification;

        /// <summary>
        /// Creates a new instance of NotificationEventArgs.
        /// </summary>
        /// <param name="n">Notification to display.</param>
        public NotificationEventArgs( Notification n )
        {
            Notification = n;
        }
    }
}
