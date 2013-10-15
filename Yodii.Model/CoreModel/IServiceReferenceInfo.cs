using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model.Interfaces
{
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
        RunningRequirement Requirement { get; }
        
        /// <summary>
        /// Gets the name of the property or constructor parameter that references the service.
        /// This is used by the dependency injection engine (IServiceHost and IPluginHost).
        /// </summary>
        string ConstructorParmeterOrPropertyName { get; }

        /// <summary>
        /// Gets the index of the parameter in the constructor if the reference appears as a parameter constructor.
        /// -1 otherwise.
        /// This is used by the dependency injection engine (IServiceHost and IPluginHost).
        /// </summary>
        int ConstructorParmeterIndex { get; }

        /// <summary>
        /// Gets whether the <see cref="Reference"/> is wrapped in <see cref="IService{T}"/>.
        /// This is used by the dependency injection engine (IServiceHost and IPluginHost).
        /// </summary>
        bool IsIServiceWrapped { get; }
    }
}
