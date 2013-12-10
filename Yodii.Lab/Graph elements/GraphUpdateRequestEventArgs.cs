using System;

namespace Yodii.Lab
{
    /// <summary>
    /// Request to regenerate and display graph.
    /// </summary>
    public class GraphUpdateRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Type of regerenation request.
        /// </summary>
        public GraphGenerationRequestType RequestType { get; private set; }

        /// <summary>
        /// New layout to apply, if it is changing.
        /// </summary>
        public GraphX.LayoutAlgorithmTypeEnum? NewLayout { get; private set; }

        /// <summary>
        /// New layout parameters to apply, if they are changing.
        /// </summary>
        public GraphX.GraphSharp.Algorithms.Layout.ILayoutParameters AlgorithmParameters { get; private set; }

        internal GraphUpdateRequestEventArgs( GraphGenerationRequestType type = GraphGenerationRequestType.RelayoutGraph,
            GraphX.LayoutAlgorithmTypeEnum? newLayout = null,
            GraphX.GraphSharp.Algorithms.Layout.ILayoutParameters algoParams = null )
        {
            RequestType = type;
            NewLayout = newLayout;
            AlgorithmParameters = algoParams;
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
