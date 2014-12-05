using System;

namespace Yodii.Model
{
    public interface IRunningService<T> : IService<T> where T : IYodiiService
    {
    }
}
