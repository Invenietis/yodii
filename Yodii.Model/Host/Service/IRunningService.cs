using System;

namespace Yodii.Model
{
    /// <summary>
    /// Declarative wrapper for a <see cref="DependencyRequirement.Running"/> dependency.
    /// This is only defined for the sake of completeness since a dependecy to a <see cref="IYodiiService"/> is
    /// de facto considered a running dependency.
    /// </summary>
    /// <typeparam name="T">Actual type of the service.</typeparam>
    public interface IRunningService<T> : IService<T> where T : IYodiiService
    {
    }
}
