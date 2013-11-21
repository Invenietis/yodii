using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Threading;
using Yodii.Model;
using System.Diagnostics;
using Yodii.Lab.Mocks;
using Yodii.Lab.Utils;
using Yodii.Lab.ConfigurationEditor;
using GraphX;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        readonly MainWindowViewModel _vm;

        public MainWindow()
        {
            _vm = new MainWindowViewModel();
            this.DataContext = _vm;
            InitializeComponent();

            GraphArea.DefaultLayoutAlgorithm = GraphX.LayoutAlgorithmTypeEnum.KK;
            GraphArea.DefaultEdgeRoutingAlgorithm = GraphX.EdgeRoutingAlgorithmTypeEnum.SimpleER;
            GraphArea.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA;
            //GraphArea.SetVerticesDrag( true, true );
            GraphArea.EnableParallelEdges = true;
            GraphArea.EdgeShowSelfLooped = true;
            GraphArea.EdgeCurvingEnabled = true;
            GraphArea.UseNativeObjectArrange = false;
            GraphArea.SideExpansionSize = new Size( 100, 100 );

            GraphArea.GenerateGraphFinished += GraphArea_GenerateGraphFinished;
            GraphArea.RelayoutFinished += GraphArea_RelayoutFinished;

            ZoomBox.IsAnimationDisabled = false;
            ZoomBox.MaxZoomDelta = 2;

            _vm.Graph.GraphUpdateRequested += Graph_GraphUpdateRequested;

        }

        void GraphArea_RelayoutFinished( object sender, EventArgs e )
        {
            ZoomBox.ZoomToFill();
            ZoomBox.Mode = GraphX.Controls.ZoomControlModes.Custom;
            foreach( var item in GraphArea.VertexList )
            {
                DragBehaviour.SetIsDragEnabled( item.Value, true );
                item.Value.EventOptions.PositionChangeNotification = true;
            }
            GraphArea.ShowAllEdgesLabels();
            GraphArea.InvalidateVisual();
        }

        void GraphArea_GenerateGraphFinished( object sender, EventArgs e )
        {
            GraphArea.GenerateAllEdges();
            ZoomBox.ZoomToFill();
            ZoomBox.Mode = GraphX.Controls.ZoomControlModes.Custom;
            foreach( var item in GraphArea.VertexList )
            {
                DragBehaviour.SetIsDragEnabled( item.Value, true );
                item.Value.EventOptions.PositionChangeNotification = true;
            }
            GraphArea.ShowAllEdgesLabels();
            GraphArea.InvalidateVisual();
        }

        void Graph_GraphUpdateRequested( object sender, GraphUpdateRequestEventArgs e )
        {
            if( e.NewLayout != null )
            {
                GraphArea.DefaultLayoutAlgorithm = (GraphX.LayoutAlgorithmTypeEnum)e.NewLayout;
            }
            if( e.AlgorithmParameters != null )
            {
                GraphArea.DefaultLayoutAlgorithmParams = e.AlgorithmParameters;
            }

            if( e.RequestType == GraphGenerationRequestType.RelayoutGraph)
            {
                this.GraphArea.GenerateGraph( _vm.Graph );
            } else if (e.RequestType == GraphGenerationRequestType.RegenerateGraph)
            {
                this.GraphArea.RelayoutGraph();
            }

        }

        private void StackPanel_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            FrameworkElement vertexPanel = sender as FrameworkElement;

            YodiiGraphVertex vertex = vertexPanel.DataContext as YodiiGraphVertex;

            _vm.SelectedVertex = vertex;
        }

        private void graphLayout_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            _vm.SelectedVertex = null;
        }

    }
}
