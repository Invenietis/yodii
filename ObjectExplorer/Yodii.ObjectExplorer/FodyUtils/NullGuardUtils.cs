
using System;
namespace NullGuard
{
    /// <summary>
    /// Prevents the injection of null checking.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Property )]
    public class AllowNullAttribute : Attribute
    {
    }

    /// <summary>
    /// Allow specific categories of members to be targeted for injection. <seealso cref="ValidationFlags"/>
    /// </summary>
    [AttributeUsage( AttributeTargets.Assembly | AttributeTargets.Class )]
    public class NullGuardAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullGuardAttribute"/> with a <see cref="ValidationFlags"/>.
        /// </summary>
        /// <param name="flags">The <see cref="ValidationFlags"/> to use for the target this attribute is being applied to.</param>
        public NullGuardAttribute( ValidationFlags flags )
        {
        }
    }
    /// <summary>
    /// Used by <see cref="NullGuardAttribute"/> to target specific categories of members.
    /// </summary>
    [Flags]
    public enum ValidationFlags
    {
        /// <summary>
        /// Don't process anything.
        /// </summary>
        None = 0,

        /// <summary>
        /// Process properties.
        /// </summary>
        Properties = 1,

        /// <summary>
        /// Process arguments of methods.
        /// </summary>
        Arguments = 2,

        /// <summary>
        /// Process out arguments of methods.
        /// </summary>
        OutValues = 4,

        /// <summary>
        /// Process return values of members.
        /// </summary>
        ReturnValues = 8,

        /// <summary>
        /// Process non-public members.
        /// </summary>
        NonPublic = 16,

        /// <summary>
        /// Process arguments, out arguments and return values of a method.
        /// </summary>
        Methods = Arguments | OutValues | ReturnValues,

        /// <summary>
        /// Process properties and arguments of public methods.
        /// </summary>
        AllPublicArguments = Properties | Arguments,

        /// <summary>
        /// Process public properties, and arguments, out arguments and return values of public methods.
        /// </summary>
        AllPublic = Properties | Methods,

        /// <summary>
        /// Process all properties, and arguments, out arguments and return values of all methods.
        /// </summary>
        All = AllPublic | NonPublic
    }
}