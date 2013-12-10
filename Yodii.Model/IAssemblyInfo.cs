using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Yodii.Model
{
    /// <summary>
    /// Assembly information for Yodii plugins.
    /// </summary>
    public interface IAssemblyInfo
    {
        /// <summary>
        /// Gets the assembly location.
        /// </summary>
        Uri AssemblyLocation { get; }
    }
}
