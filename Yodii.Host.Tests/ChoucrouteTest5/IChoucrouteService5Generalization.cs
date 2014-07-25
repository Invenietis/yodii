using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public interface IChoucrouteService5Generalization : IYodiiService
    {
        void DoSomethingElse();
        List<string> CalledMethods { get; }
    }
}
