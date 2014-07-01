using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// Relationship between two elements in the Yodii Lab graph.
    /// </summary>
    public enum YodiiGraphEdgeType
    {
        /// <summary>
        /// Plugin implementing target service
        /// </summary>
        Implementation = 0,
        /// <summary>
        /// Service specializing target service
        /// </summary>
        Specialization = 1,
        /// <summary>
        /// Plugin referencing (through requirements) a service
        /// </summary>
        ServiceReference = 2
    }
}
