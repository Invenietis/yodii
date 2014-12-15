using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public interface IAnotherService : IYodiiService
    {
        void DoSomethingElse();
    }

    public class AnotherPlugin : YodiiPluginBase, IAnotherService
    {
        public void DoSomethingElse()
        {
        }
    }
}
