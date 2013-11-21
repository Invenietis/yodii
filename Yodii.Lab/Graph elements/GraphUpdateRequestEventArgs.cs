using System;

namespace Yodii.Lab
{
    public class GraphUpdateRequestEventArgs : EventArgs
    {
        public GraphGenerationRequestType RequestType { get; private set; }
        public GraphX.LayoutAlgorithmTypeEnum? NewLayout { get; private set; }
        public GraphX.GraphSharp.Algorithms.Layout.ILayoutParameters AlgorithmParameters { get; private set; }

        public GraphUpdateRequestEventArgs( GraphGenerationRequestType type = GraphGenerationRequestType.RelayoutGraph,
            GraphX.LayoutAlgorithmTypeEnum? newLayout = null,
            GraphX.GraphSharp.Algorithms.Layout.ILayoutParameters algoParams = null )
        {
            RequestType = type;
            NewLayout = newLayout;
            AlgorithmParameters = algoParams;
        }

    }

    public enum GraphGenerationRequestType
    {
        RelayoutGraph,
        RegenerateGraph
    }
}
