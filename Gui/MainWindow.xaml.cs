using System.Windows;
using System.Windows.Controls;
using System.Threading;

using FindDuplicates;
using System.Collections.Generic;

namespace Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GuiController ctrl;
        delegate void StatusUpdateCallBack(string status);

        public MainWindow()
        {
            InitializeComponent();
            UpdateButtonState();
            ctrl = new GuiController();
            ctrl.PathProvider = () => {
                var ret = new List<string>();
                foreach (var i in FoldersToSearchList.Items)
                {
                    var s = i.ToString();
                    if(s != null)
                        ret.Add(s);
                }

                return ret;
            };
            ctrl.StatusListener = (string txt) =>
            {
                StatusText.Dispatcher.BeginInvoke(new StatusUpdateCallBack(UpdateStatusText), new object[] { txt });
            };
            SizeUnitSelection.SelectedIndex = 1;
        }

        private void UpdateStatusText(string txt) 
        {
            StatusText.Text = txt;
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
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Multiselect = true,
                Description = "Add directories to search.",
                UseDescriptionForTitle = true
            };
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



        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ctrl.Start();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
