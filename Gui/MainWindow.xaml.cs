using System.Windows;
using System.Windows.Controls;

namespace Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateButtonState();
        }

        private void UpdateButtonState() 
        {
            RemoveButton.IsEnabled = FoldersToSearchList.SelectedIndex != -1;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var si = FoldersToSearchList.SelectedIndex;
            if(si != -1)
                FoldersToSearchList.Items.RemoveAt(si);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                foreach (var p in dialog.SelectedPaths)
                    FoldersToSearchList.Items.Add(p);
            }
        }

        private void FoldersToSearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonState();
        }
    }
}
