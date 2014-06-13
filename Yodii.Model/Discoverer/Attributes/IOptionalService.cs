using System;

namespace Yodii.Model
{
    public interface IOptionalService<T> : IService<T> where T : IYodiiService
    {
    }
}
