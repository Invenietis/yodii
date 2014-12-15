using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Host
{
    /// <summary>
    /// Defines how Service proxies handle exceptions on method, property or event of <see cref="IYodiiService"/> interfaces. 
    /// This drives code generation and defaults to <see cref="HonorIgnoreExceptionAttribute"/>.
    /// </summary>
    public enum CatchExceptionGeneration
    {
        /// <summary>
        /// Never generate code that intercepts exceptions.
        /// </summary>
        Never,

        /// <summary>
        /// Always generate code that intercepts exceptions.
        /// </summary>
        Always,

        /// <summary>
        /// Generate code that intercepts exceptions except if the method, property or event of 
        /// the <see cref="IYodiiService"/> interface is marked with <see cref="IgnoreExceptionAttribute"/>.
        /// </summary>
        HonorIgnoreExceptionAttribute
    }
}
