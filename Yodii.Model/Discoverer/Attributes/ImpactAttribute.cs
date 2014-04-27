using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    [AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
    public sealed class ImpactAttribute : Attribute
    {
        public StartDependencyImpact Impact { get; set; }
    }
}
