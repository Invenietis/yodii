using System;

namespace Yodii.Model
{
    public interface IOptionalRecommendedService<T> : IService<T> where T : IYodiiService
    {
    }
}
