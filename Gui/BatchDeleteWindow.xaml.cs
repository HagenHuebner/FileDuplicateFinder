using FindDuplicates;
using System.Linq;
using System.Windows;

namespace Gui
{
    /// <summary>
    /// Interaction logic for BatchDeleteWindow.xaml
    /// </summary>
    public partial class BatchDeleteWindow : Window
    {
        private readonly BatchDeletionGuiController ctrl_;
        delegate void UpdateStatusTextCallBack(StatusUpdate statusUpdate);
        delegate void EnableButtonCallBack(bool enable);
        delegate void UpdateGUICallback();

        public BatchDeleteWindow(BatchDeletionGuiController ctrl)
        {
            InitializeComponent();
            ctrl_ = ctrl;
            ctrl_.selectedPathsProvider = () => FoldersToDeleteFromList.Items.OfType<string>();
            foreach (var p in ctrl_.allPathProvider())
                FoldersToDeleteFromList.Items.Add(p);

            ctrl_.statusListener = (StatusUpdate update) =>
            {
                Dispatcher.BeginInvoke(new UpdateStatusTextCallBack(UpdateProgressWidgets), new object[] { update });
            };

            ctrl_.onFinished = () =>
            {
                Dispatcher.BeginInvoke(new EnableButtonCallBack(EnableButtons), new object[] { true });
                Dispatcher.BeginInvoke(new UpdateGUICallback(UpdateGui));
            };

            UpdateGui();
        }

        private void UpdateProgressWidgets(StatusUpdate statusUpdate)
        {
            ProgressBar.Value = statusUpdate.Progress;
            ProgressText.Text = statusUpdate.Message;
        }

        private void EnableButtons(bool enabled)
        {
            DeleteFilesButton.IsEnabled = enabled;
            CancelButton.IsEnabled = enabled;
            RemoveFolderButton.IsEnabled = enabled;
        }

        private void UpdateGui()
        {
            DeleteFilesButton.IsEnabled = ctrl_.DeleteEnabled();
            DeleteFileStatusText.Text = ctrl_.CannotDeleteAllErorrMessage();
        }

        private void DeleteFilesButton_clicked(object sender, RoutedEventArgs e)
        {
            var delFolders = CleanupFolderCheckBox.IsChecked.GetValueOrDefault(false);
            EnableButtons(false);
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
