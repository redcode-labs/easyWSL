﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using easyWslLib;

namespace easyWSL
{
    public sealed partial class ManageDistrosPage : Page
    {
        public class EditedDistro
        {
            public string name { get; set; }
            public string version { get; set; }
            public string path { get; set; }
        }

        private Windows.Storage.StorageFolder storageDirectory = Windows.Storage.ApplicationData.Current.LocalFolder;


        public ManageDistrosPage()
        { 
            InitializeComponent();
            RefreshInstalledDistros();
        }

        private WslSdk wslSdk = new();
        private Helpers helpers = new();
        private ManageSnapshotsPage manageSnapshotsPage = new();
        public string selectedDistroName;

        private async Task RefreshInstalledDistros()
        {
            await  WslSdk.GetInstalledDistributions();
            distrosListView.ItemsSource = null;
            distrosListView.Items.Clear();
            distrosListView.ItemsSource = WslSdk.InstalledDistros.Values;
            if(distrosListView.Items.Count != 0) 
            { 
                distrosListView.SelectedIndex = 0;
            }
            
        }
        private void distrosListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                selectedDistroName = distrosListView.SelectedValue.ToString();
            }
        }
        private async void removeDistroButton_Click(object sender, RoutedEventArgs e)
        {
            confirmDistroRemovalDialog.Title = $"Are you sure you want to remove {selectedDistroName}?";
            confirmDistroRemovalDialog.XamlRoot = removeDistroButton.XamlRoot;
            ContentDialogResult dialogResult = await confirmDistroRemovalDialog.ShowAsync();
            if (dialogResult == ContentDialogResult.Primary)
            {
                await helpers.ExecuteProcessAsynch("wsl.exe", $"--unregister {selectedDistroName}");
                string distroStoragePath = Path.Combine(storageDirectory.Path, selectedDistroName);
                if(Directory.Exists(distroStoragePath))
                {
                    Directory.Delete(distroStoragePath, true);
                }
                await RefreshInstalledDistros();
            }
        }

        private async void moreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var name = selectedDistroName;
            var version = WslSdk.InstalledDistros[name].version;
            var path = WslSdk.InstalledDistros[name].path;

            var dialogContent = new MoreInfoDialog(name, version, path);

            ContentDialog getMoreInfoDialog = new ContentDialog();
            getMoreInfoDialog.XamlRoot = moreInfoButton.XamlRoot;
            getMoreInfoDialog.Title = "More info";
            getMoreInfoDialog.CloseButtonText = "Close";
            getMoreInfoDialog.Content = dialogContent;
            

            var result = await getMoreInfoDialog.ShowAsync();

        }

        private async void setDefaultDistroButton_Click(object sender, RoutedEventArgs e)
        {
            await helpers.ExecuteProcessAsynch("wsl.exe", $"-s {selectedDistroName}");
        }
        private void openFilesystemButton_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(@"\\wsl$", selectedDistroName);
            Process.Start("explorer.exe", path);
        }

        private async void startDistroButton_Click(object sender, RoutedEventArgs e)
        {
            helpers.StartWSLDistroAsync(selectedDistroName);
        }

        private async void stopDistroButton_Click(object sender, RoutedEventArgs e)
        {
            await helpers.ExecuteProcessAsynch("wsl.exe", $"-t {selectedDistroName}");
        }

    }
}
