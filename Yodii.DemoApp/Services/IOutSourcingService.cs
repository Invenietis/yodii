using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface IOutSourcingService : IYodiiService
    {
        void GetEmployees();
    }
}
