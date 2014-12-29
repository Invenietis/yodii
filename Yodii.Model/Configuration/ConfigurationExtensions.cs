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
        /// Combine this <see cref="StartDependencyImpact"/> with another one.
        /// </summary>
        /// <param name="@this">This <see cref="StartDependencyImpact"/>.</param>
        /// <param name="other">Other <see cref="StartDependencyImpact"/> to combine.</param>
        /// <param name="result">Resulting combination. Unknown if combination is not possible.</param>
        /// <returns>True on success, false if other is not combinable with this impact.</returns>
        public static bool Combine( this StartDependencyImpact @this, StartDependencyImpact other, out StartDependencyImpact result )
        {
            // First check for equality and Unknown and only after we can handle Minimal case: 
            // this ensures that Unknown and Minimal is combined to Minimal.
            if( @this == other || other == StartDependencyImpact.Unknown ) result = @this;
            else if( @this == StartDependencyImpact.Unknown ) result = other;
            else if( @this == StartDependencyImpact.Minimal ) result = other;
            else if( other == StartDependencyImpact.Minimal ) result = @this;
            else if( (@this & StartDependencyImpact.IsTryOnly) != 0 )
            {
                if( (other & StartDependencyImpact.IsTryOnly) == 0 ) result = other;
                else return DoCombineTryOnly( @this, other, out result );
            }
            else if( (other & StartDependencyImpact.IsTryOnly) != 0 )
            {
                if( (@this & StartDependencyImpact.IsTryOnly) == 0 ) result = @this;
                else return DoCombineTryOnly( @this, other, out result );
            }
            else
            {
                return DoCombine( @this, other, out result );
            }
            return true;
        }

        static bool DoCombineTryOnly( StartDependencyImpact @this, StartDependencyImpact other, out StartDependencyImpact result )
        {
            Debug.Assert( @this != other && (@this & StartDependencyImpact.IsTryOnly) != 0 && (other & StartDependencyImpact.IsTryOnly) != 0 );

            if( !DoCombine( (@this & ~StartDependencyImpact.IsTryOnly), (other & ~StartDependencyImpact.IsTryOnly), out result ) )
            {
                return false;
            }
            result |= StartDependencyImpact.IsTryOnly;
            return true;
        }

        static bool DoCombine( StartDependencyImpact @this, StartDependencyImpact other, out StartDependencyImpact result )
        {
            result = StartDependencyImpact.Unknown;
            Debug.Assert( @this != other
                            && (@this & StartDependencyImpact.IsTryOnly) == 0
                            && (other & StartDependencyImpact.IsTryOnly) == 0
                            && @this != StartDependencyImpact.Minimal
                            && @this != StartDependencyImpact.Unknown
                            && other != StartDependencyImpact.Minimal
                            && other != StartDependencyImpact.Unknown );

            switch( @this )
            {
                case StartDependencyImpact.FullStop:
                    /// Impacts causing conflict: FullStart, StartRecommended, StartRecommendedAndStopOptionalAndRunnable
                    if( other == StartDependencyImpact.StopOptionalAndRunnable ) result = @this;
                    else return false;
                    break;

                case StartDependencyImpact.StopOptionalAndRunnable:
                    /// Impacts causing conflict: FullStart
                    if( other == StartDependencyImpact.FullStop || other == StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable ) result = other;
                    else if( other == StartDependencyImpact.StartRecommended ) result = StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable;
                    else return false;
                    break;

                case StartDependencyImpact.StartRecommended:
                    /// Impacts causing conflicts: FullStop
                    if( other == StartDependencyImpact.FullStart || other == StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable ) result = other;
                    else if( other == StartDependencyImpact.StopOptionalAndRunnable ) result = StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable;
                    else return false;
                    break;

                case StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable:
                    // Impacts causing conflicts: FullStart, FullStop
                    if( other == StartDependencyImpact.StartRecommended
                        || other == StartDependencyImpact.StopOptionalAndRunnable ) result = @this;
                    else return false;
                    break;

                case StartDependencyImpact.FullStart:
                    /// Impacts causing conflict: FullStop, StopOptionalAndRunnable, StartRecommendedAndStopOptionalAndRunnable
                    if( other == StartDependencyImpact.StartRecommended ) result = @this;
                    else return false;
                    break;

                default: throw new ArgumentException();
            }
            return true;
        }

    }
}
