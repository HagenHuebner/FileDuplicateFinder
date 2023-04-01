using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


using FindDuplicates;

namespace Gui
{
    /// <summary>
    /// Interaction logic for BatchDeleteWindow.xaml
    /// </summary>
    public partial class BatchDeleteWindow : Window
    {
        private readonly BatchDeletionGuiController ctrl_;
        delegate void UpdateUICallback(StatusUpdate statusUpdate);

        public BatchDeleteWindow(BatchDeletionGuiController ctrl)
        {
            InitializeComponent();
            ctrl_ = ctrl;
            ctrl_.selectedPathsProvider = () => FoldersToDeleteFromList.Items.OfType<string>();
            foreach (var p in ctrl_.allPathProvider())
                FoldersToDeleteFromList.Items.Add(p);

            ctrl_.statusListener = (StatusUpdate update) =>
            {
                Dispatcher.BeginInvoke(new UpdateUICallback(UpdateProgressWidgets), new object[] { update });
            };

            UpdateGui();
        }

        private void UpdateProgressWidgets(StatusUpdate statusUpdate) 
        {
            ProgressBar.Value = statusUpdate.Progress;
            ProgressText.Text = statusUpdate.Message;
        }

        private void UpdateGui() 
        {
            DeleteFilesButton.IsEnabled = ctrl_.DeleteEnabled();
            DeleteFileStatusText.Text = ctrl_.CannotDeleteAllErorrMessage();
        }

        private void DeleteFilesButton_clicked(object sender, RoutedEventArgs e)
        {
            var delFolders = CleanupFolderCheckBox.IsChecked.GetValueOrDefault(false);
            ctrl_.DeleteDuplicatesAsync(delFolders);
        }

        private void RemoveFolderButton_clicked(object sender, RoutedEventArgs e) 
        {
            var i = FoldersToDeleteFromList.SelectedIndex;
            if (i != -1)
                FoldersToDeleteFromList.Items.RemoveAt(i);

            UpdateGui();
        }

        private void CancelButton_clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}
