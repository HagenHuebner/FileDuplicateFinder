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

        class Duplicate
        {
            public Duplicate(string name, bool isPath) 
            {
                Name = name;
                IsPath = isPath;
            }
            public override string ToString()
            {
                return Name;
            }

            public readonly string Name;
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
                    DuplicateList.Items.Add(new Duplicate(set.ViewString(), false));
                    foreach (var item in set.Items)
                    {
                        DuplicateList.Items.Add(new Duplicate(item.FullPath, true));
                    }
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

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
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
            var dup = (Duplicate)item;
            if (dup != null && dup.IsPath) 
            {
                try
                {
                    if (dup.Name != null)
                    {
                        new Process
                        {
                            StartInfo = new ProcessStartInfo(dup.Name)
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
    }
}
