using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Tests.TestYodiiObjects
{
    public interface ISubService : IMyYodiiService, IYodiiService
    {
        void HelloWorld2();
    }
}
