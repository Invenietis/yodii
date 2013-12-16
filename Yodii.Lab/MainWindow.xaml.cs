using System;
using System.Windows;
using GraphX;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        readonly MainWindowViewModel _vm;

        /// <summary>
        /// Creates the main window.
        /// </summary>
        public MainWindow()
        {
            _vm = new MainWindowViewModel(false);
            this.DataContext = _vm;
            _vm.NewNotification += _vm_NewNotification;
            InitializeComponent();

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


            GraphArea.DefaultLayoutAlgorithm = _vm.GraphLayoutAlgorithmType;
            GraphArea.DefaultLayoutAlgorithmParams = _vm.GraphLayoutParameters;

            this.Loaded += MainWindow_Loaded;
        }

        void _vm_NewNotification( object sender, NotificationEventArgs e )
        {
            if( this.NotificationControl != null )
            {
                this.NotificationControl.AddNotification( e.Notification );
            }
        }

        void MainWindow_Loaded( object sender, RoutedEventArgs e )
        {
            GraphArea.GenerateGraph( _vm.Graph );
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
                this.GraphArea.RelayoutGraph();
            } else if (e.RequestType == GraphGenerationRequestType.RegenerateGraph)
            {
                this.GraphArea.GenerateGraph( _vm.Graph, true, true, true );
            }

        }

        private void StackPanel_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            FrameworkElement vertexPanel = sender as FrameworkElement;

            YodiiGraphVertex vertex = vertexPanel.DataContext as YodiiGraphVertex;

            _vm.SelectedVertex = null;
            _vm.SelectedVertex = vertex;
        }

        private void graphLayout_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            _vm.SelectedVertex = null;
        }

    }
}
