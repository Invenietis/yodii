using System;

namespace Yodii.Lab
{
    /// <summary>
    /// Request to regenerate and display graph.
    /// </summary>
    public class GraphUpdateRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Type of regeneration request.
        /// </summary>
        public GraphGenerationRequestType RequestType { get; private set; }

        internal GraphUpdateRequestEventArgs( GraphGenerationRequestType type = GraphGenerationRequestType.RelayoutGraph )
        {
            RequestType = type;
        }

    }

    /// <summary>
    /// Type of graph regeneration to request.
    /// </summary>
    public enum GraphGenerationRequestType
    {
        /// <summary>
        /// Request a layout update.
        /// </summary>
        RelayoutGraph,

        /// <summary>
        /// Request a complete regeneration of the graph contents.
        /// </summary>
        RegenerateGraph
    }
}
