using System.Windows;
using System.Windows.Controls;
using System.Threading;

using FindDuplicates;
using System.Collections.Generic;
using System.Diagnostics;
using System;

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
            UpdateGUI();
        }

        private void UpdateStatusText(string txt) 
        {
            StatusText.Text = txt;
        }

        private void UpdateGUI() 
        {
            RemoveButton.IsEnabled = FoldersToSearchList.SelectedIndex != -1;
            StartButton.IsEnabled = !ctrl.IsRunning() && FoldersToSearchList.Items.Count > 0;
            StopButton.IsEnabled = ctrl.IsRunning();
            UpdateResultList();
        }

        private void UpdateResultList() 
        {
            var res = ctrl.Result();
            if (res != null)
            {
                foreach (var set in res)
                {
                    DuplicateList.Items.Add(new DuplicateEntry(set.ViewString(), false));
                    foreach (var item in set.Items)
                        DuplicateList.Items.Add(new DuplicateEntry(item.FullPath, true));
                }
            }
            else
                DuplicateList.Items.Clear();
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
                MessageBox.Show("Enter a whole number for the minimul file size.");
            }
            ctrl.minFileSize = size * FileSizeMultiplier();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateMinFileSize();
            ctrl.Start();
            UpdateGUI();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ctrl.Stop();
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
                    MessageBox.Show(ex.Message);
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
    }
}
