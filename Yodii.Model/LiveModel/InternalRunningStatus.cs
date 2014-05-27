﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Yodii.Model.LiveModel
{
    /// <summary>
    /// Defines the internal runtime status for a plugin (or a service) with the transition states. 
    /// To ease programmation, this object fully encapsulates the 5 different possible status available in the static
    /// fields <see cref="Disabled"/>, <see cref="Stopped"/>, <see cref="Stopping"/>, <see cref="Starting"/>, <see cref="Started"/>.
    /// </summary>
    public sealed class InternalRunningStatus
    {
        const int _disabled = 0;
        const int _stopped = 1;
        const int _stopping = 2;
        const int _starting = 3;
        const int _started = 4;

        int _v;

        /// <summary>
        /// Service or plugin is disabled. For service, it may be disabled, or there is no
        /// available plugin that offers this service.
        /// </summary>
        static public readonly InternalRunningStatus Disabled = new InternalRunningStatus( _disabled );

        /// <summary>
        /// Service or plugin is stopped.
        /// </summary>
        static public readonly InternalRunningStatus Stopped = new InternalRunningStatus( _stopped );

        /// <summary>
        /// Service or plugin is is currently stopping.
        /// </summary>
        static public readonly InternalRunningStatus Stopping = new InternalRunningStatus( _stopping );

        /// <summary>
        /// Service or plugin is currently starting.
        /// </summary>
        static public readonly InternalRunningStatus Starting = new InternalRunningStatus( _starting );

        /// <summary>
        /// Service or plugin is running.
        /// </summary>
        static public readonly InternalRunningStatus Started = new InternalRunningStatus( _started );

        InternalRunningStatus( int v )
        {
            _v = v;
        }

        /// <summary>
        /// True if the next status represents a valid transition from this status.
        /// </summary>
        /// <param name="next">Possible next status.</param>
        /// <param name="allowErrorTransition">True to authorize transitions due to error.</param>
        /// <returns>True if the next status is a valid next one (like <see cref="Starting"/> to <see cref="Started"/>). False otherwise.</returns>
        public bool IsValidTransition( InternalRunningStatus next, bool allowErrorTransition )
        {

            switch( _v )
            {
                case _disabled: return next._v == _stopped;
                case _stopped: return next._v == _starting || next._v == _disabled;
                case _stopping: return next._v == _stopped;
                case _starting: return next._v == _started || (allowErrorTransition && next._v < _starting); //When in Exception, we can pass directly from starting to 'stopping' (and directly to 'stopped' ?)
            }
            // When Started
            return next._v == _stopping;
        }

        /// <summary>
        /// True if the service or plugin is runnable: a runnable plugin is NOT disabled.
        /// </summary>
        public bool IsRunnable
        {
            get { return _v > _disabled; }
        }

        /// <summary>
        /// True if the service or plugin is stopped or disabled.
        /// </summary>
        public bool IsStoppedOrDisabled
        {
            get { return _v <= _stopped; }
        }

        /// <summary>
        /// True if the service or plugin is stopped or stopping.
        /// </summary>
        public bool IsStoppingOrStopped
        {
            get { return _v == _stopped || _v == _stopping; }
        }

        /// <summary>
        /// True if the service or plugin is started or starting.
        /// </summary>
        public bool IsStartingOrStarted
        {
            get { return _v == _starting || _v == _started; }
        }

        /// <summary>
        /// True if the service or plugin is starting or stopping.
        /// </summary>
        public bool IsStartingOrStopping
        {
            get { return _v == _stopping || _v == _starting; }
        }

        /// <summary>
        /// Compares the two <see cref="InternalRunningStatus"/>.
        /// The order is defined as: <see cref="Disabled"/> -> <see cref="Stopped"/> -> <see cref="IsStartingOrStopping"/> -> <see cref="Started"/>.
        /// The <see cref="Starting"/> and <see cref="Stopping"/> status are not ordered (they are different but not lesser nor greater than the other one):
        /// this comparison operator will always return false when comparing these two transition status.
        /// </summary>
        /// <param name="x">The first status to compare.</param>
        /// <param name="y">The second status to compare.</param>
        /// <returns>True if x is lesser than y (in the sense described in summary).</returns>
        public static bool operator <( InternalRunningStatus x, InternalRunningStatus y )
        {
            Debug.Assert( _stopping == 2 && _starting == 3, "If values change we may have to change the equation." );
            return x._v < y._v && !((x._v == 2 && y._v == 3) || (x._v == 3 && y._v == 2));
        }

        /// <summary>
        /// See <see cref="operator&lt;"/> for description of the ordering.
        /// </summary>
        /// <param name="x">The first status to compare.</param>
        /// <param name="y">The second status to compare.</param>
        /// <returns>True if x is greater than y.</returns>
        public static bool operator >( InternalRunningStatus x, InternalRunningStatus y )
        {
            Debug.Assert( _stopping == 2 && _starting == 3, "If values change we may have to change the equation." );
            return x._v > y._v && !((x._v == 2 && y._v == 3) || (x._v == 3 && y._v == 2));
        }

        /// <summary>
        /// See <see cref="operator&lt;"/> for description of the ordering.
        /// </summary>
        /// <param name="x">The first status to compare.</param>
        /// <param name="y">The second status to compare.</param>
        /// <returns>True if x is greater or equal to y.</returns>
        public static bool operator >=( InternalRunningStatus x, InternalRunningStatus y )
        {
            Debug.Assert( _stopping == 2 && _starting == 3, "If values change we may have to change the equation." );
            return x._v >= y._v && !((x._v == 2 && y._v == 3) || (x._v == 3 && y._v == 2));
        }

        /// <summary>
        /// See <see cref="operator&lt;"/> for description of the ordering.
        /// </summary>
        /// <param name="x">The first status to compare.</param>
        /// <param name="y">The second status to compare.</param>
        /// <returns>True if x is lesser or equal to y.</returns>
        public static bool operator <=( InternalRunningStatus x, InternalRunningStatus y )
        {
            Debug.Assert( _stopping == 2 && _starting == 3, "If values change we may have to change the equation." );
            return x._v <= y._v && !((x._v == 2 && y._v == 3) || (x._v == 3 && y._v == 2));
        }

        /// <summary>
        /// Overriden to return the status name.
        /// </summary>
        /// <returns>Name of the status ("Disabled", "Stopped", "Stopping", etc.).</returns>
        public override string ToString()
        {
            switch( _v )
            {
                case _disabled: return "Disabled";
                case _stopped: return "Stopped";
                case _stopping: return "Stopping";
                case _starting: return "Starting";
            }
            return "Started";
        }
    }
}
