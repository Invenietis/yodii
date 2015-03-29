using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Yodii.ObjectExplorer.Windows
{
    /// <summary>
    /// Interaction logic for ObjectExplorerWindow.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class ObjectExplorerWindow
    {
        IConfigurationManagerWrapper _configurationManager;

        public ObjectExplorerWindow( IConfigurationManagerWrapper configurationManager )
        {
            _configurationManager = configurationManager;

            BindingErrorListener.Listen( m => MessageBox.Show( m ) );
            InitializeComponent();
        }

        private void ConfigManagerButton_Click( object sender, RoutedEventArgs e )
        {
            _configurationManager.ShowOrActivate();
        }
    }

    [ExcludeFromCodeCoverage]
    public class BindingErrorListener : TraceListener
    {
        private Action<string> logAction;
        public static void Listen( Action<string> logAction )
        {
            PresentationTraceSources.DataBindingSource.Listeners
                .Add( new BindingErrorListener() { logAction = logAction } );
        }
        public override void Write( string message )
        {
            logAction( message );
        }
        public override void WriteLine( string message )
        {
            logAction( message );
        }
    }
}
