using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using FindDuplicates;

namespace Gui
{
    /// <summary>
    /// Interaction logic for BatchDeleteWindow.xaml
    /// </summary>
    public partial class BatchDeleteWindow : Window
    {
        private readonly BatchDeletionGuiController ctrl_ = new();

        public BatchDeleteWindow(Func<List<string>> searchPathProvider)
        {
            InitializeComponent();
            ctrl_.allPathProvider = searchPathProvider;
            ctrl_.selectedPathsProvider = () => FoldersToDeleteFromList.Items.OfType<string>();
            foreach (var p in ctrl_.allPathProvider())
                FoldersToDeleteFromList.Items.Add(p);

            UpdateGui();
        }

        private void UpdateGui() 
        {
            DeleteFilesButton.IsEnabled = ctrl_.DeleteEnabled();
            DeleteFileStatusText.Text = ctrl_.CannotDeleteAllErorrMessage();
        }

        private void DeleteFilesButton_clicked(object sender, RoutedEventArgs e)
        {
            //TODO actually trigger deletion
            BatchDeleteFiles();
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

        private void BatchDeleteFiles()
        {
            BackgroundWorker deletionWorder = new();
            deletionWorder.WorkerReportsProgress = true;
            deletionWorder.DoWork += worker_DoWork;
            deletionWorder.ProgressChanged += worker_ProgressChanged;
            deletionWorder.RunWorkerCompleted += OnWorkerFinished;
            deletionWorder.RunWorkerAsync();
        }


        private void OnWorkerFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                Close();
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i <= 100; i++) //perform deletion here
            {
                ((BackgroundWorker)sender).ReportProgress(i);
                Thread.Sleep(100);
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }
    }

}
