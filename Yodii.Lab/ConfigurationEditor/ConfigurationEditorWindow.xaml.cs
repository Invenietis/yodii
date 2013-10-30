using System;
using System.Windows;
using System.Windows.Controls;
using Yodii.Model;

namespace Yodii.Lab.ConfigurationEditor
{
    /// <summary>
    /// Interaction logic for ConfigurationEditorWindow.xaml
    /// </summary>
    public partial class ConfigurationEditorWindow : Window
    {
        #region Fields

        /// <summary>
        /// Used when resetting combo boxes without triggering ConfigurationManager Add events.
        /// Prevents reentrancy.
        /// </summary>
        bool isResettingSelection;

        readonly ConfigurationManager _configurationManager;

        #endregion Fields

        #region Constructor
        internal ConfigurationEditorWindow(ConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;

            this.DataContext = this;

            InitializeComponent();
        }
        #endregion Constructor

        #region Properties

        public ConfigurationManager ConfigurationManager
        {
            get
            {
                return _configurationManager;
            }
        }

        #endregion Properties

        #region Event handlers
        private void ConfigurationStatusValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Both at 1 if old value was replaced (and not initialized, like when the window starts up)
            if (!isResettingSelection && e.RemovedItems.Count == 1 && e.AddedItems.Count == 1)
            {
                // Find the selected item, starting from the bottom
                ComboBox box = sender as ComboBox;
                FrameworkElement parentElement = box.Parent as FrameworkElement; // It should be a DockPanel, in our case
                ConfigurationItem item = parentElement.DataContext as ConfigurationItem;

                ConfigurationStatus oldStatus = item.Status;
                ConfigurationStatus newStatus = (ConfigurationStatus)e.AddedItems[0];

                // Now we can change it.
                var statusChangeResult = item.SetStatus(newStatus, "ConfigurationEditor");

                if (!statusChangeResult.IsSuccessful)
                {
                    System.Windows.MessageBox.Show(
                        String.Format("Couldn't change status to {0}:\n{1}", newStatus, String.Join("; ", statusChangeResult.FailureCauses))
                        ,
                        "Status change failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK,
                        MessageBoxOptions.None
                        );
                    isResettingSelection = true;
                    box.SelectedItem = oldStatus;
                }
            }
            isResettingSelection = false;
        }

        private void DeleteConfigurationItemButton_Click(object sender, RoutedEventArgs e)
        {
            // Find the selected item, starting from the bottom
            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement; // It should be a DockPanel, in our case
            ConfigurationItem item = parentElement.DataContext as ConfigurationItem;

            var deleteItemResult = item.Layer.Items.Remove(item.ServiceOrPluginId);

            if (!deleteItemResult.IsSuccessful)
            {
                System.Windows.MessageBox.Show(
                    String.Format("Couldn't remove {0} from layer:\n{1}", item.ServiceOrPluginId, String.Join("; ", deleteItemResult.FailureCauses))
                    ,
                    "Removal failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None
                    );
            }
        }

        private void NewConfigurationItemButton_Click(object sender, RoutedEventArgs e)
        {
            // Find the selected item, starting from the bottom
            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement; // DockPanel

            ComboBox newItemStatusBox = parentElement.FindName("NewConfigurationStatusValue") as ComboBox;
            TextBox newItemTextBox = parentElement.FindName("NewPluginOrServiceId") as TextBox;

            if (newItemStatusBox.SelectedItem == null || String.IsNullOrEmpty(newItemTextBox.Text))
                return;

            ConfigurationStatus newStatus = (ConfigurationStatus)newItemStatusBox.SelectedItem;

            string newItemId = newItemTextBox.Text;


            FrameworkElement parent2Element = parentElement.Parent as FrameworkElement; // Layer StackPanel

            ConfigurationLayer layer = parent2Element.DataContext as ConfigurationLayer;

            isResettingSelection = true; // Won't try to update selection on true (or causes reentrancy)
            var addItemResult = layer.Items.Add(newItemId, newStatus, "ConfigurationEditor");

            if (!addItemResult.IsSuccessful)
            {
                System.Windows.MessageBox.Show(
                    String.Format("Couldn't add new item {0} to layer:\n{1}", newItemId, String.Join("; ", addItemResult.FailureCauses))
                    ,
                    "Add failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None
                    );
            }
            else
            {
                newItemTextBox.Clear();
                newItemStatusBox.SelectedIndex = 0;
            }

            isResettingSelection = false;
        }

        private void NewLayerButton_Click(object sender, RoutedEventArgs e)
        {
            string newLayerName = this.NewLayerName.Text;

            ConfigurationLayer newLayer = new ConfigurationLayer(newLayerName);

            var layerAddResult = _configurationManager.Layers.Add(newLayer);

            if (!layerAddResult.IsSuccessful)
            {
                System.Windows.MessageBox.Show(
                    String.Format("Couldn't add a new layer:\n{0}", String.Join("; ", layerAddResult.FailureCauses))
                    ,
                    "Layer add failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None
                    );
            }
            else
            {
                this.NewLayerName.Clear();
            }
        }

        private void DeleteLayerButton_Click(object sender, RoutedEventArgs e)
        {
            // Find the selected item, starting from the bottom
            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement; // StackPanel
            ConfigurationLayer layer = parentElement.DataContext as ConfigurationLayer;

            var deleteLayerResult = _configurationManager.Layers.Remove(layer);

            if (!deleteLayerResult.IsSuccessful)
            {
                System.Windows.MessageBox.Show(
                    String.Format("Couldn't remove layer:\n{0}", String.Join("; ", deleteLayerResult.FailureCauses))
                    ,
                    "Layer removal failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None
                    );
            }
        }
        #endregion Event handlers
    }
}
