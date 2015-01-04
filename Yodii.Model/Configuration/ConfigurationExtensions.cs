using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets whether this <see cref="StartDependencyImpact"/> has at least one IsTryXXX bit sets.
        /// </summary>
        /// <param name="@this">This <see cref="StartDependencyImpact"/>.</param>
        /// <returns>True if a IsTryXXX bit is set.</returns>
        public static bool HasTryBit( this StartDependencyImpact @this )
        {
            return (@this & StartDependencyImpact.TryFullStart) != 0;
        }

        /// <summary>
        /// Returns a <see cref="StartDependencyImpact"/> without any IsTryXXX bit in it.
        /// Note that <see cref="StartDependencyImpact.Minimal"/> is preserved.
        /// </summary>
        /// <param name="@this">This <see cref="StartDependencyImpact"/>.</param>
        /// <returns>Impact without IsTryXXX bits.</returns>
        public static StartDependencyImpact ClearAllTryBits( this StartDependencyImpact @this )
        {
            return @this & (StartDependencyImpact.FullStart|StartDependencyImpact.Minimal);
        }

        /// <summary>
        /// Returns a <see cref="StartDependencyImpact"/> with only IsTryXXX bit in it.
        /// Note that <see cref="StartDependencyImpact.Minimal"/> is always set.
        /// </summary>
        /// <param name="@this">This <see cref="StartDependencyImpact"/>.</param>
        /// <returns>Impact with only IsTryXXX bits.</returns>
        public static StartDependencyImpact ToTryBits( this StartDependencyImpact @this )
        {
            int moved = (int)(@this & StartDependencyImpact.FullStart) << 4;
            int cleared = (int)(@this & StartDependencyImpact.TryFullStart);
            return (StartDependencyImpact)(cleared | moved) | StartDependencyImpact.Minimal;
        }

        /// <summary>
        /// Returns a <see cref="StartDependencyImpact"/> without any IsTryXXX bit that have a XXX bit set.
        /// </summary>
        /// <param name="@this">This <see cref="StartDependencyImpact"/>.</param>
        /// <returns>Impact without superfluous IsTryXXX bits.</returns>
        public static StartDependencyImpact ClearUselessTryBits( this StartDependencyImpact @this )
        {
            var noTry = @this & StartDependencyImpact.FullStart;
            var tryMask = (StartDependencyImpact)~((int)noTry << 4);
            return (noTry & tryMask) | (@this & StartDependencyImpact.Minimal);
        }


    }
}
