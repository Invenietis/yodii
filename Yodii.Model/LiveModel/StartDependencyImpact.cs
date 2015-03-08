#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\LiveModel\StartDependencyImpact.cs) is part of CiviKey. 
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

namespace Yodii.Model
{
    /// <summary>
    /// Impact of a plugin or service start on its dependencies.
    /// </summary>
    /// <remarks>
    /// Defines whether the plugin starting will also attempt to start everything it has references to,
    /// or only the minimum required services.
    /// </remarks>
    [Flags]
    public enum StartDependencyImpact
    {
        /// <summary>
        /// Impact is not determined. Ultimately defaults to <see cref="Minimal"/>.
        /// </summary>
        Unknown = 0,
   
        /// <summary>
        /// Do not try to start nor stop dependencies that are not absolutely required (<see cref="DependencyRequirement.Running"/>).
        /// This is the default.
        /// </summary>
        Minimal = 1,

        /// <summary>
        /// Starts <see cref="DependencyRequirement.RunnableRecommended"/> dependencies.
        /// When this bit is set, it takes precedence over <see cref="IsTryStartRunnableRecommended"/>.
        /// </summary>
        IsStartRunnableRecommended = 2,

        /// <summary>
        /// Starts <see cref="DependencyRequirement.OptionalRecommended"/> dependencies.
        /// When this bit is set, it takes precedence over <see cref="IsTryStartOptionalRecommended"/>.
        /// </summary>
        IsStartOptionalRecommended = 4,

        /// <summary>
        /// Starts <see cref="DependencyRequirement.Runnable"/> dependencies.
        /// When this bit is set, it takes precedence over <see cref="IsTryStartRunnableOnly"/>.
        /// </summary>
        IsStartRunnableOnly = 8,

        /// <summary>
        /// Starts <see cref="DependencyRequirement.Optional"/> dependencies.
        /// When this bit is set, it takes precedence over <see cref="IsTryStartOptionalOnly"/>.
        /// </summary>
        IsStartOptionalOnly = 16,

        /// <summary>
        /// Attempts to start <see cref="DependencyRequirement.RunnableRecommended"/> dependencies.
        /// </summary>
        IsTryStartRunnableRecommended = 32,

        /// <summary>
        /// Attempts to start <see cref="DependencyRequirement.OptionalRecommended"/> dependencies.
        /// </summary>
        IsTryStartOptionalRecommended = 64,

        /// <summary>
        /// Attempts to start <see cref="DependencyRequirement.Runnable"/> dependencies.
        /// </summary>
        IsTryStartRunnableOnly = 128,

        /// <summary>
        /// Attempts to start <see cref="DependencyRequirement.Optional"/> dependencies.
        /// </summary>
        IsTryStartOptionalOnly = 256,

        /// <summary>
        /// Starts recommended dependencies: the ones that are <see cref="DependencyRequirement.OptionalRecommended"/>
        /// and <see cref="DependencyRequirement.RunnableRecommended"/>.
        /// </summary>
        StartRecommended = IsStartOptionalRecommended | IsStartRunnableRecommended,

        /// <summary>
        /// Attempts to start recommended dependencies: the ones that are <see cref="DependencyRequirement.OptionalRecommended"/>
        /// and <see cref="DependencyRequirement.RunnableRecommended"/>.
        /// </summary>
        TryStartRecommended = IsTryStartOptionalRecommended | IsTryStartRunnableRecommended,

        /// <summary>
        /// Starts all dependencies: <see cref="DependencyRequirement.OptionalRecommended"/>, <see cref="DependencyRequirement.RunnableRecommended"/>, 
        /// but also the "not recommended" ones (<see cref="DependencyRequirement.Optional"/> and <see cref="DependencyRequirement.Runnable"/>).
        /// </summary>
        FullStart = StartRecommended | IsStartRunnableOnly | IsStartOptionalOnly, 

        /// <summary>
        /// Attempts to start all dependencies: <see cref="DependencyRequirement.OptionalRecommended"/>, 
        /// <see cref="DependencyRequirement.RunnableRecommended"/>, but also the "not recommended" ones (<see cref="DependencyRequirement.Optional"/> 
        /// and <see cref="DependencyRequirement.Runnable"/>).
        /// </summary>
        TryFullStart = TryStartRecommended | IsTryStartRunnableOnly | IsTryStartOptionalOnly 
    }
}
