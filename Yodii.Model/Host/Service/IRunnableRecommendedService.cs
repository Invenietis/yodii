using System;

namespace Yodii.Model
{
    /// <summary>
    /// Declarative wrapper for a <see cref="DependencyRequirement.RunnableRecommended"/> dependency.
    /// </summary>
    /// <typeparam name="T">Actual type of the service.</typeparam>
    public interface IRunnableRecommendedService<T> : IService<T> where T : IYodiiService
    {
    }
}
