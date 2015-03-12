using System;

/// <summary>
/// These marker attributes are usually present in PropertyChanged.Fody (PropertyChanged.dll).
/// Having them locally stored in our project allows us to remove the .csproj reference to PropertyChanged.dll.
/// Note that, even if we DO have a reference to PropertyChanged.dll, they are removed when weaving; the resulting assembly no longer references PropertyChanged.dll.
/// </summary>
namespace PropertyChanged
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false )]
    public class AlsoNotifyForAttribute : Attribute
    {
        public AlsoNotifyForAttribute( string property ) { }
        public AlsoNotifyForAttribute( string property, params string[] otherProperties ) { }
    }

    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false )]
    public class DoNotNotifyAttribute : Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false )]
    public class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute( string dependency ) { }
        public DependsOnAttribute( string dependency, params string[] otherDependencies ) { }
    }

    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
    public class DoNotCheckEqualityAttribute : Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Class, Inherited = false )]
    public class ImplementPropertyChangedAttribute : Attribute
    {
    }
}