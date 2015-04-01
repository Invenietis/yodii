using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Tests.TestYodiiObjects
{
    [Description( "A service with a display attribute." )]
    public interface IServiceWithDisplayAttribute : IYodiiService
    {
        void HelloWorld();
    }
}
