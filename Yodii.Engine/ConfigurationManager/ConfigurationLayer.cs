#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationManager\ConfigurationLayer.cs) is part of CiviKey. 
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class ConfigurationLayer : IConfigurationLayer
    {
        internal readonly ConfigurationItemCollection Items;
        ConfigurationLayerCollection _owner;
        string _layerName;

        internal ConfigurationLayer( ConfigurationLayerCollection owner, string layerName )
        {
            _owner = owner;
            _layerName = String.IsNullOrWhiteSpace( layerName ) ? String.Empty : layerName;
            Items = new ConfigurationItemCollection( this );
        }

        public string LayerName
        {
            get { return _layerName; }
            set
            {
                if( _layerName.Length == 0 ) throw new InvalidOperationException();
                if( String.IsNullOrWhiteSpace( value ) ) throw new ArgumentException();
                if( _layerName != value )
                {
                    if( _owner != null )
                    {
                        int i = _owner.IndexOf( value );
                        _layerName = value;
                        _owner.CheckPosition( i );
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        internal ConfigurationLayerCollection Owner 
        {
            get { return _owner; }
            set
            {
                Debug.Assert( _owner != value );
                _owner = value;
                NotifyPropertyChanged();
            }
        }

        IConfigurationManager IConfigurationLayer.ConfigurationManager
        {
            get { return _owner != null ? _owner.ConfigurationManager : null; }
        }

        IConfigurationItemCollection IConfigurationLayer.Items
        {
            get { return Items; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged( [CallerMemberName]string propertyName = "" )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

        internal void ClearDefaultLayer()
        {
            Debug.Assert( _layerName.Length == 0 );
            Items.Clear();
        }

    }
}
