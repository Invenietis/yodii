using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface IOutSourcingService : IYodiiService
    {
        bool GetEmployees();
    }
}
