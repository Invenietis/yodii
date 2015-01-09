#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\IYodiiEngineResult.cs) is part of CiviKey. 
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
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace Yodii.Model
{
    /// <summary>
    /// Yodii engine result, containing various data regarding the engine operation.
    /// </summary>
    public interface IYodiiEngineResult
    {
        /// <summary>
        /// Gets the engine that generated this result.
        /// </summary>
        IYodiiEngine Engine { get; }

        /// <summary>
        /// Gets whether the operation is a success: <see cref="ConfigurationFailureResult"/>, <see cref="StaticFailureResult"/> and <see cref="HostFailureResult"/> are null.
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// Details of errors encountered during resolution of configuration.
        /// When not null, <see cref="StaticFailureResult"/> and <see cref="HostFailureResult"/> are null.
        /// </summary>
        IConfigurationFailureResult ConfigurationFailureResult { get; }

        /// <summary>
        /// Details of errors encountered during static resolution.
        /// When not null, <see cref="ConfigurationFailureResult"/> and <see cref="HostFailureResult"/> are null.
        /// </summary>
        IStaticFailureResult StaticFailureResult { get; }

        /// <summary>
        /// Details of errors encountered during host startup and plugin startup/shutdown.
        /// When not null, <see cref="ConfigurationFailureResult"/> and <see cref="StaticFailureResult"/> are null.
        /// </summary>
        IDynamicFailureResult HostFailureResult { get; }

        /// <summary>
        /// List of plugins causing the failure, whatever the failure is (from  <see cref="StaticFailureResult"/> or <see cref="HostFailureResult"/>).
        /// Never null (but can be empty).
        /// </summary>
        IReadOnlyList<IPluginInfo> PluginCulprits { get; }
        
        /// <summary>
        /// List of services causing the failure: in case of <see cref="StaticFailureResult"/>, it is the service of <see cref="IStaticFailureResult.BlockingServices"/>
        /// and when <see cref="HostFailureResult"/> is not null, this is the services implemented by the <see cref="IDynamicFailureResult.ErrorPlugins"/> that failed.
        /// Never null (but can be empty).
        /// </summary>
        IReadOnlyList<IServiceInfo> ServiceCulprits { get; }
    }
}
