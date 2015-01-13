#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\IYodiiEngineExternal.cs) is part of CiviKey. 
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
using System.ComponentModel;

namespace Yodii.Model
{
    /// <summary>
    /// Yodii engine is the primary object of Yodii.
    /// It is in charge of maintaining coherency among available plugins and services, their configuration and the evolution at runtime.
    /// </summary>
    public interface IYodiiEngineExternal : IYodiiEngine, INotifyPropertyChanged
    {
        /// <summary>
        /// Whether this engine is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Whether this engine is currently stopping.
        /// </summary>
        bool IsStopping { get; }

        /// <summary>
        /// Starts the engine (that must be stopped), performs all possible resolutions,
        /// and begins monitoring configuration for changes.
        /// </summary>
        /// <param name="persistedCommands">Optional list of commands that will be initialized.</param>
        /// <returns>Engine start result.</returns>
        /// <exception cref="InvalidOperationException">This engine must not be running (<see cref="IsRunning"/> must be false).</exception>
        IYodiiEngineResult StartEngine( IEnumerable<YodiiCommand> persistedCommands = null );

        /// <summary>
        /// Stops the engine: stops all plugins and stops monitoring configuration.
        /// </summary>
        void StopEngine();

        /// <summary>
        /// Triggers the static resolution of the graph (with the current <see cref="IYodiiEngine.Configuration">Configuration</see>).
        /// This has no impact on the engine and can be called when <see cref="IsRunning"/> is false.
        /// </summary>
        /// <returns>
        /// <para>
        /// The result with a potential non null <see cref="IYodiiEngineResult.StaticFailureResult"/> but always an 
        /// available <see cref="IYodiiEngineStaticOnlyResult.StaticSolvedConfiguration"/>.
        /// </para>
        /// <para>
        /// This method is useful only for advanced scenarios (for instance before starting the engine).
        /// </para>
        /// </returns>
        IYodiiEngineStaticOnlyResult StaticResolutionOnly();

    }
}
