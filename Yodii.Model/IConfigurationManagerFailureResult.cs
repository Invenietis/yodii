using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IConfigurationManagerFailureResult
    {
        IReadOnlyList<string> FailureReasons { get; }
    }
}
