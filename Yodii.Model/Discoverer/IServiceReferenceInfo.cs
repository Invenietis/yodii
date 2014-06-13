using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Service reference information.
    /// </summary>
    public interface IServiceReferenceInfo
    {
        /// <summary>
        /// Gets the <see cref="IPluginInfo"/> that defines this reference.
        /// </summary>
        IPluginInfo Owner { get; }

        /// <summary>
        /// Gets a reference to the actual service.
        /// </summary>
        IServiceInfo Reference { get; }

        /// <summary>
        /// Gets the requirement for the referenced service.
        /// </summary>
        DependencyRequirement Requirement { get; }
        
        /// <summary>
        /// Gets the name of the constructor parameter that references the service.
        /// </summary>
        string ConstructorParameterName { get; }

        /// <summary>
        /// Gets the index of the parameter in the constructor.
        /// This is used by the dependency injection engine (IServiceHost and IPluginHost).
        /// </summary>
        int ConstructorParameterIndex { get; }

        /// <summary>
        /// Gets whether the <see cref="Reference"/> is directly expressed without <see cref="IRunningService{T}"/> wrapper.
        /// </summary>
        bool IsNakedRunningService { get; }
    }
}
