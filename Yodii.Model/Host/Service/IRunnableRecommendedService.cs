using System;

namespace Yodii.Model
{
    public interface IRunnableRecommendedService<T> : IService<T> where T : IYodiiService
    {

    }
}
