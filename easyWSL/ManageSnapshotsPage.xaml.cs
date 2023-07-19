using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace easyWSL
{
    public sealed partial class ManageSnapshotsPage : Page
    {
        private WslSdk wslSdk = new();
        private Windows.Storage.StorageFolder storageDirectory = Windows.Storage.ApplicationData.Current.LocalFolder;

        private Helpers helpers = new();

        private List<string> snapshotsList = new();

        public string nameToRegister { get; set; }

        public ManageSnapshotsPage()
        {
            InitializeComponent();
            RefreshDistros();
        }

        public async Task RefreshDistros()
        {
            await WslSdk.GetInstalledDistributions();
            var distrosList = WslSdk.InstalledDistros.Values.Select(o => o.name);
            distrosComboBox.ItemsSource = distrosList;

            FillSnapshotsListView();
        }

        private void distrosComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillSnapshotsListView();
        }

        private async void addSnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            var snapshotsStoragePath = Path.Combine(storageDirectory.Path, "snapshots");
            var distroName = distrosComboBox.SelectedItem as string;
            ShowProgressBar($"Creating snapshot of {distroName}");

            Guid fileUUID = Guid.NewGuid();
            var snapshotName = fileUUID.ToString();

            var snapshotPath = Path.Combine(snapshotsStoragePath, distroName, $"{snapshotName}.tar.gz");
            await helpers.ExecuteProcessAsynch("wsl.exe", $"--export {distroName} {snapshotPath}");

            HideProgressBar();

            ContentDialog succedDialog = new ContentDialog();
            succedDialog.Title = "Succesfuly created snapshot";
            succedDialog.CloseButtonText = "Cancel";
            succedDialog.DefaultButton = ContentDialogButton.Close;
            FillSnapshotsListView();
        }

        private void removeSnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            int index = snapshotsListView.SelectedIndex;
            var snapshotPath = snapshotsList[index];

            if (File.Exists(snapshotPath))
            {
                File.Delete(snapshotPath);
            }
            string distro = distrosComboBox.SelectedValue as string;
            string path = Path.Combine(storageDirectory.Path, "snapshots", distro);
            if (Directory.Exists(path))
            {
                int filesInDir = Directory.EnumerateFiles(path).Count();
                if (filesInDir == 0)
                {
                    Directory.Delete(path, true);
                }
                
            }
            FillSnapshotsListView();
        }

        private async void FillSnapshotsListView()
        {
            await WslSdk.GetInstalledDistributions();
            var distrosList = WslSdk.InstalledDistros.Values.Select(o => o.name);

            var snapshotsStoragePath = Path.Combine(storageDirectory.Path, "snapshots");
            try
            {
                Directory.CreateDirectory(snapshotsStoragePath);
            }
            catch (Exception e)
            {
                await showErrorModal();
            }
            foreach (string name in distrosList)
            {
                Directory.CreateDirectory(Path.Combine(snapshotsStoragePath, name));
            }

            var distro = distrosComboBox.SelectedValue as string;
            if (distro == null)
            {
                return;
            }

            var snapshotsPath = Path.Combine(storageDirectory.Path, "snapshots", distro);
            snapshotsList = Directory.EnumerateFiles(snapshotsPath, "*.tar.gz").ToList();

            List<string> listviewLabels = new();

            foreach (var snapshot in snapshotsList)
            {
                listviewLabels.Add(File.GetCreationTime(snapshot).ToString());
            }

            snapshotsListView.ItemsSource = null;
            snapshotsListView.Items.Clear();
            snapshotsListView.ItemsSource = listviewLabels;
            snapshotsListView.SelectedIndex = 0;

            if (snapshotsList.Count == 0)
            {
                snapshotsTitle.Text = $"There are no snapshots of {distro} distribution";
            }
            else
            {
                snapshotsTitle.Text = $"Snapshots of {distro} distribution";
            }
        }

        private void ShowProgressBar(string text)
        {
            snapshottingStatusTextBlock.Text = text;
            snapshottingStatusTextBlock.Visibility = Visibility.Visible;
            snapshottingProgressBar.Visibility = Visibility.Visible;
        }

        private void HideProgressBar()
        {
            snapshottingProgressBar.Visibility = Visibility.Collapsed;
            snapshottingStatusTextBlock.Visibility = Visibility.Collapsed;
        }

        private async void registerDistroFromSnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox distroNameTextBox = new TextBox();
            distroNameTextBox.Header = "Name";

            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = registerDistroFromSnapshotButton.XamlRoot;
            dialog.Title = "Register from snapshot";
            dialog.PrimaryButtonText = "Register";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = distroNameTextBox;
            
             

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string name = distroNameTextBox.Text;

                if (name == "" || name.Contains(" "))
                {
                    await showErrorModal();
                    return;
                }
                else
                {
                    int index = snapshotsListView.SelectedIndex;
                    var snapshotPath = snapshotsList[index];
                    await RegisterDistro(name, snapshotPath);
                }
            }
        }

        private async Task RegisterDistro(string name, string path)
        {
            ShowProgressBar($"Registering the {name} distribution");
            var distroStoragePath = Path.Combine(storageDirectory.Path, name);
            Directory.CreateDirectory(distroStoragePath);
            await helpers.ExecuteProcessAsynch("wsl.exe", $"--import {name} {distroStoragePath} {path}");

            HideProgressBar();

            ContentDialog registerDistroDialog = new ContentDialog();
            registerDistroDialog.XamlRoot = registerDistroFromSnapshotButton.XamlRoot;
            registerDistroDialog.Title = $"{name} has been registered";
            registerDistroDialog.SecondaryButtonText = "Close";
            registerDistroDialog.PrimaryButtonText = "Run distribution";
            registerDistroDialog.DefaultButton = ContentDialogButton.Primary;

            var result = await registerDistroDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                helpers.StartWSLDistroAsync(name);
            }
        }

        private void openSnapshotsButton_Click(object sender, RoutedEventArgs e)
        {
            string distro = distrosComboBox.SelectedValue as string;
            string path = Path.Combine(storageDirectory.Path, "snapshots", distro);
            Process.Start("explorer.exe", path);
        }
        private async Task showErrorModal()
        {
            ContentDialog errorDialog = new ContentDialog();
            errorDialog.XamlRoot = registerDistroFromSnapshotButton.XamlRoot;
            errorDialog.Title = "Error";
            errorDialog.CloseButtonText = "Cancel";
            errorDialog.DefaultButton = ContentDialogButton.Close;
            errorDialog.Content = "There were problems with registering your distribution.";
            await errorDialog.ShowAsync();
        }
    }
}

