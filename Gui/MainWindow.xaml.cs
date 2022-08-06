﻿using System.Windows;
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
        delegate void UpdateUICallback();

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
                foreach (var list in res)
                {
                    var title = "---" + res.Count + "---";
                    DuplicateList.Items.Add(title);
                    foreach (var item in list)
                    {
                        DuplicateList.Items.Add(item.FullPath);
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
    }
}