using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IConfigurationManagerFailureResult
    {
        IReadOnlyList<ConfigurationConflict> BlockingItems { get; }

        IReadOnlyList<string> FailureReasons { get; }
    }
}
