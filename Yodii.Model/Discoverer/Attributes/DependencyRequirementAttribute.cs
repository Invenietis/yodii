using System;

namespace Yodii.Model
{
    [AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
    public sealed class DependencyRequirementAttribute : Attribute
    {
        public DependencyRequirement DependencyRequirement { get; set; }
    }
}
