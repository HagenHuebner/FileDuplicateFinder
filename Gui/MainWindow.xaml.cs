using System.Windows;
using System.Windows.Controls;
using System.Threading;

using FindDuplicates;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;

namespace Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GuiController ctrl;
        delegate void StatusUpdateCallBack(string status);
        delegate void UpdateUICallback();

        class DuplicateEntry
        {
            public DuplicateEntry(string name, bool isPath) 
            {
                Text = name;
                IsPath = isPath;
            }
            public override string ToString()
            {
                return Text;
            }

            public readonly string Text;
            public readonly bool IsPath;
        }

        public MainWindow()
        {
            InitializeComponent();
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
            ctrl.OnFinished = () =>
            {
                Dispatcher.BeginInvoke(new UpdateUICallback(UpdateGUI));
            };
            ctrl.StatusListener = (string txt) =>
            {
                StatusText.Dispatcher.BeginInvoke(new StatusUpdateCallBack(UpdateStatusText), new object[] { txt });
            };
            MinFileSizeEntry.Text = "0";
            SizeUnitSelection.SelectedIndex = 1;
            AskBeforeDeleteCheckBox.IsChecked = true;
            UpdateGUI();
        }

        private void UpdateStatusText(string txt) 
        {
            StatusText.Text = txt;
        }

        private void UpdateDeleteButtonState() 
        {
            DeleteButton.IsEnabled = DuplicateList.SelectedIndex != -1 && ((DuplicateEntry) DuplicateList.SelectedItem).IsPath;
        }

        private void UpdateGUI() 
        {
            RemoveButton.IsEnabled = FoldersToSearchList.SelectedIndex != -1;
            StartButton.IsEnabled = ctrl.AllowStart() && FoldersToSearchList.Items.Count > 0;
            StopButton.IsEnabled = ctrl.AllowStop();
            UpdateDeleteButtonState();
            UpdateResultList();
        }

        private void UpdateResultList() 
        {
            DuplicateList.Items.Clear();
            var res = ctrl.Result();
            if (res != null)
            {
                foreach (var set in res)
                {
                    DuplicateList.Items.Add(new DuplicateEntry("--- " + set.ViewString(), false));
                    foreach (var item in set.Items)
                        DuplicateList.Items.Add(new DuplicateEntry(item.FullPath, true));
                }
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var si = FoldersToSearchList.SelectedIndex;
            if(si != -1)
                FoldersToSearchList.Items.RemoveAt(si);
        }

        private void AddFoldersIfNew(string[] newFolders) 
        {
            var current = new List<string>();
            current.AddRange(FoldersToSearchList.Items.OfType<string>());
            var notAdded = new List<string>();
            foreach (var f in newFolders) 
            {
                if(!current.Any(x => BaseDirectory.PathsShareDirectory(x, f)))
                    FoldersToSearchList.Items.Add(f);
                else
                    notAdded.Add(f);
            }

            if (notAdded.Count > 0) 
            {
                var list = notAdded.Aggregate((a, b) => a + ",\n" + b);
                ShowError("The following folders where not added:\n" + list +
                    "\nbecause they contain previously added folders or vice versa.");
            }
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
                AddFoldersIfNew(dialog.SelectedPaths);
            }
            UpdateGUI();
        }

        private void FoldersToSearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGUI();
        }

        private long FileSizeMultiplier() 
        {
            return SizeUnitSelection.SelectedIndex switch
            {
                1 => 1024,
                2 => 1024 * 1024,
                3 => 1024 * 1024 * 1024,
                _ => (long)1,
            };
        }

        private void UpdateMinFileSize() 
        {
            long size;
            if (!long.TryParse(MinFileSizeEntry.Text, out size)) 
            {
                ShowError("Enter a whole number for the minimul file size.");
            }
            ctrl.minFileSize = size * FileSizeMultiplier();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateMinFileSize();
            foreach (var f in FoldersToSearchList.Items) 
            {
                if (!Directory.Exists(f.ToString())) 
                {
                    ShowError(f.ToString() + " does not exist.");
                    return;
                }
            }

            ctrl.Start();
            UpdateGUI();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ctrl.Stop();
            UpdateGUI();
        }

        private void ShowError(string msg) 
        {
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DuplicateList_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            UpdateDeleteButtonState();
        }

        private void DuplicateList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = DuplicateList.SelectedItem;
            var dup = (DuplicateEntry)item;
            if (dup != null && dup.IsPath) 
            {
                try
                {
                    if (dup.Text != null)
                    {
                        new Process
                        {
                            StartInfo = new ProcessStartInfo(dup.Text)
                            {
                                UseShellExecute = true
                            }
                        }.Start();
                    }
                }
                catch (Exception ex) 
                {
                    ShowError(ex.Message);
                }
            }
        }

        private void MinFileSizeEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateMinFileSize();
        }

        private void SizeUnitSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMinFileSize();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var path = ((DuplicateEntry)DuplicateList.SelectedItem).Text;
            if (!File.Exists(path)) 
            {
                ShowError(path + " does not exist");
                return;
            }
            
            var askUser = AskBeforeDeleteCheckBox.IsChecked.GetValueOrDefault(false);
            if (askUser) 
            {
                var res = MessageBox.Show("Delete " + path + " permanently?", "Delete file",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                File.Delete(path);
            }
            catch (Exception ex) 
            {
                ShowError(ex.Message);
            }
        }
    }
}
