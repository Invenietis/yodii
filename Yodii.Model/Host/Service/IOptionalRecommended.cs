using System;

namespace Yodii.Model
{
    public interface IOptionalRecommended<T> : IService<T> where T : IYodiiService
    {
    }
}
