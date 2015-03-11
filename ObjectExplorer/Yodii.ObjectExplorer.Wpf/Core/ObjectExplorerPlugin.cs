#region LGPL License
/*----------------------------------------------------------------------------
* This file (ObjectExplorer\Yodii.ObjectExplorer.Wpf\Core\ObjectExplorerPlugin.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Yodii.Model;
using Yodii.Wpf;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// WPF Object Explorer plugin.
    /// </summary>
    public class ObjectExplorerPlugin : WindowPluginBase
    {
        IYodiiEngineBase _activeEngine;

        public ObjectExplorerPlugin( IYodiiEngineBase e )
            : base( e )
        {
            if( e == null ) throw new ArgumentNullException( "e" );

            ShowClosingFailedMessageBox = true;
            StopPluginWhenWindowCloses = true;
            AutomaticallyDisableCloseButton = true;

            _activeEngine = e;
        }


        protected override Window CreateWindow()
        {
            return new ObjectExplorerWindow( _activeEngine );
        }
    }
}
