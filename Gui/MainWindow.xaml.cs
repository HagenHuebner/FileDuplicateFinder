﻿using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic.FileIO;
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
        delegate void StatusUpdateCallBack(StatusUpdate status);
        delegate void UpdateUICallback();

        public MainWindow()
        {
            InitializeComponent();
            ctrl = new();
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
            ctrl.StatusListener = (StatusUpdate update) =>
            {
                Dispatcher.BeginInvoke(new StatusUpdateCallBack(UpdateStatusText), new object[] { update });
            };
            MinFileSizeEntry.Text = "0";
            SizeUnitSelection.SelectedIndex = 1;
            FilePathFilterTypeSelection.SelectedIndex = 0;
            AskBeforeDeleteCheckBox.IsChecked = true;
            RecycleCheckBox.IsChecked = false;
            UpdateGUI();
        }

        private void UpdateStatusText(StatusUpdate upd) 
        {
            StatusText.Text = upd.Message;
            ProgressBar.Value = upd.Progress;
        }

        private void UpdateDeleteButtonState() 
        {
            DeleteButton.IsEnabled = DuplicateList.SelectedIndex != -1 &&
                ((DuplicateEntry) DuplicateList.SelectedItem).IsFile();
        }

        private void UpdateButtonStates() 
        {
            RemoveButton.IsEnabled = FoldersToSearchList.SelectedIndex != -1;
            StartButton.IsEnabled = ctrl.AllowStart() && FoldersToSearchList.Items.Count > 0;
            StopButton.IsEnabled = ctrl.AllowStop();
            UpdateDeleteButtonState();
            BatchDeleteButton.IsEnabled = DuplicateList.Items.Count > 0;
        }

        private void UpdateGUI() 
        {
            UpdateResultList();
            UpdateButtonStates();
        }

        private void UpdateResultList() 
        {
            DuplicateList.Items.Clear();
            var res = ctrl.Result();
            if (res != null)
            {
                foreach (var set in res)
                {
                    DuplicateList.Items.Add(new SetDuplicateEntry(set));
                    foreach (var item in set.Items)
                        DuplicateList.Items.Add(new FileDuplicateEntry(item));
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
            UpdateButtonStates();
        }

        private void FoldersToSearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStates();
        }

        private void FilePathFilterTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            ctrl.Filter.PartPartIsAtEnd = FilePathFilterTypeSelection.SelectedIndex == 0;
            UpdateButtonStates();
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
            if (!long.TryParse(MinFileSizeEntry.Text, out long size))
            {
                ShowError("Enter a whole number for the minimul file size.");
            }
            ctrl.Filter.MinSizeBytes = size * FileSizeMultiplier();
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

        private static void ShowError(string msg) 
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
            if (dup != null && dup.IsFile()) 
            {
                try
                {
                    if (dup.Text != null)
                    {
                        new Process
                        {
                            StartInfo = new ProcessStartInfo(dup.Text())
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

        private void FilePathFilter_TextChange(object sender, TextChangedEventArgs e) 
        {
            ctrl.Filter.PathPart = FilePathFilterText.Text;
        }

        private void SizeUnitSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMinFileSize();
        }

        private void DuplicateList_KeyPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            OnDeleteSelected();
        }

        private void OnDeleteSelected() 
        {
            var selObj = DuplicateList.SelectedItem;
            if (selObj == null)
                return;

            var item = (DuplicateEntry)selObj;
            if (!item.IsFile())
                return;

            var path = item.Text();
            if (!File.Exists(path))
            {
                ShowError(path + " does not exist");
                return;
            }

            var deletePermanently = !RecycleCheckBox.IsChecked.GetValueOrDefault(false);
            var askUser = AskBeforeDeleteCheckBox.IsChecked.GetValueOrDefault(false);
            if (askUser)
            {
                var msg = deletePermanently ? "Delete " + path + " permanently?" :
                    "Recycle " + path + " ?";

                var res = MessageBox.Show(msg, "Delete file",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                if (deletePermanently)
                    File.Delete(path);
                else
                    FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                DuplicateList.Items.Remove(selObj);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            OnDeleteSelected();
        }

        private List<DuplicateSet> AllDisplayedDuplicateSets() 
        {
            var ret = new List<DuplicateSet>();
            foreach(var obj in DuplicateList.Items) 
            {
                var dupEntry = (DuplicateEntry)obj;
                if (!dupEntry.IsFile()) 
                {
                    var set = ((SetDuplicateEntry)dupEntry).Set();
                    ret.Add(set);
                }
                
            }

            return ret;
        }

        private void BatchDelete_Click(object sender, RoutedEventArgs e) 
        {
            var delCtrl = ctrl.MkBatchDeletionController();
            delCtrl.duplicateProvider = AllDisplayedDuplicateSets;
            var batchWin = new BatchDeleteWindow(delCtrl);
            batchWin.Show();
        }
    }
}
