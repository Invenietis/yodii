using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface ITimerService : IYodiiService
    {
        void IncreaseSpeed();

        void DecreaseSpeed();

        void Stop();

        void Start();
    }
}
