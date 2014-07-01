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

        void SubscribeToTimerEvent( EventHandler handler );
        void UnsubscribeToTimerEvent( EventHandler handler );
    }
}
