#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationManager\ConfigurationItemCollection.cs) is part of CiviKey. 
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
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
    class ConfigurationItemCollection : CKObservableSortedArrayKeyList<IConfigurationItem, string>, IConfigurationItemCollection
    {
        internal readonly ConfigurationLayer ParentLayer;

        internal ConfigurationItemCollection( ConfigurationLayer parent )
            : base( e => e.ServiceOrPluginFullName, StringComparer.Ordinal.Compare, allowDuplicates: false )
        {
            ParentLayer = parent;
        }

        IConfigurationLayer IConfigurationItemCollection.ParentLayer
        {
            get { return ParentLayer; }
        }

        public IConfigurationItem this[string name]
        {
            get { return this.GetByKey( name ); }
        }

        internal ConfigurationManager ConfigurationManager
        {
            get { return ParentLayer.Owner == null ? null : ParentLayer.Owner.ConfigurationManager; }
        }

        public IYodiiEngineResult Set( string serviceOrPluginFullName, ConfigurationStatus status, StartDependencyImpact impact, string description )
        {
            return DoSet( serviceOrPluginFullName, status, impact, description );
        }

        public IYodiiEngineResult Set( string serviceOrPluginFullName, ConfigurationStatus status, string description )
        {
            return DoSet( serviceOrPluginFullName, status, null, description );
        }

        public IYodiiEngineResult Set( string serviceOrPluginFullName, StartDependencyImpact impact, string description )
        {
            return DoSet( serviceOrPluginFullName, null, impact, description );
        }

        IYodiiEngineResult DoSet( string serviceOrPluginFullName, ConfigurationStatus? status, StartDependencyImpact? impact, string description )
        {
            Debug.Assert( status.HasValue || impact.HasValue );
            if( String.IsNullOrEmpty( serviceOrPluginFullName ) ) throw new ArgumentException( "serviceOrPluginFullName is null or empty" );

            IYodiiEngineResult result;
            IConfigurationItem existing = this.GetByKey( serviceOrPluginFullName );
            if( existing != null )
            {
                if( !status.HasValue ) result = existing.Set( impact.Value, description );
                else if( !impact.HasValue ) result = existing.Set( status.Value, description );
                else result = existing.Set( status.Value, impact.Value, description );
                return result;
            }
            if( !status.HasValue ) status = ConfigurationStatus.Optional;
            if( !impact.HasValue ) impact = StartDependencyImpact.Unknown;
            ConfigurationItem newItem = new ConfigurationItem( ParentLayer, serviceOrPluginFullName, status.Value, impact.Value, description ?? String.Empty );
            var c = ConfigurationManager;
            if( c == null )
            {
                Add( newItem );
                return SuccessYodiiEngineResult.NullEngineSuccessResult;
            }

            result = c.OnConfigurationItemAdding( newItem );
            if( result.Success )
            {
                Add( newItem );
                c.OnConfigurationChanged();
                return result;
            }
            newItem.OnRemoved();
            return result;
        }

        /// <summary>
        /// Removes a configuration for plugin or a service.
        /// </summary>
        /// <param name="serviceOrPluginFullName">The identifier.</param>
        /// <returns>Detailed result of the operation.</returns>
        public new IYodiiEngineResult Remove( string serviceOrPluginFullName )
        {
            if( String.IsNullOrEmpty( serviceOrPluginFullName ) ) throw new ArgumentException( "serviceOrPluginFullName is null or empty" );

            var c = ConfigurationManager;
            ConfigurationItem toRemove = (ConfigurationItem)this.GetByKey( serviceOrPluginFullName );
            if( toRemove == null )
            {
                return c != null ? c.Engine.SuccessResult : SuccessYodiiEngineResult.NullEngineSuccessResult;
            }
            if( c == null )
            {
                toRemove.OnRemoved();
                base.Remove( toRemove );
                return SuccessYodiiEngineResult.NullEngineSuccessResult;
            }
            IYodiiEngineResult result = c.OnConfigurationItemRemoving( toRemove );
            if( result.Success )
            {
                toRemove.OnRemoved();
                base.Remove( toRemove );
                c.OnConfigurationChanged();
            }
            return result;
        }

    }

}
