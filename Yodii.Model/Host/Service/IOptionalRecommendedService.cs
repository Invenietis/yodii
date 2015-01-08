using System;

namespace Yodii.Model
{
    /// <summary>
    /// Declarative wrapper for a <see cref="DependencyRequirement.OptionalRecommended"/> dependency.
    /// </summary>
    /// <typeparam name="T">Actual type of the service.</typeparam>
    public interface IOptionalRecommendedService<T> : IService<T> where T : IYodiiService
    {
    }
}
