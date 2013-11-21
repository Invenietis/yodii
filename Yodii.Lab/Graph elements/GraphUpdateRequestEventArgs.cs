using System;

namespace Yodii.Lab
{
    public class GraphUpdateRequestEventArgs : EventArgs
    {
        public GraphGenerationRequestType RequestType { get; private set; }

        public GraphUpdateRequestEventArgs( GraphGenerationRequestType type = GraphGenerationRequestType.RelayoutGraph )
        {
            RequestType = type;
        }

    }

    public enum GraphGenerationRequestType
    {
        RelayoutGraph,
        RegenerateGraph
    }
}
