using System;

namespace Yodii.Model
{
    /// <summary>
    /// Declarative wrapper for a <see cref="DependencyRequirement.Optional"/> dependency.
    /// </summary>
    /// <typeparam name="T">Actual type of the service.</typeparam>
    public interface IOptionalService<T> : IService<T> where T : IYodiiService
    {
    }
}
