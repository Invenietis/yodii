using System;

namespace Yodii.Model
{
    /// <summary>
    /// Declarative wrapper for a <see cref="DependencyRequirement.Runnable"/> dependency.
    /// </summary>
    /// <typeparam name="T">Actual type of the service.</typeparam>
    public interface IRunnableService<T> : IService<T> where T : IYodiiService
    {

    }
}
