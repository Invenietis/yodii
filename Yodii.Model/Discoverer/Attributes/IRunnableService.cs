using System;

namespace Yodii.Model
{
    public interface IRunnableService<T> : IService<T> where T : IYodiiService
    {

    }
}
