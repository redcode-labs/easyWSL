using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.InteropServices;
using Windows.Storage.Pickers;
using WinRT;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using easyWslLib;

namespace easyWSL
{
    public sealed partial class RegisterNewDistro_Page : Page
    {

        private string distroTarballPath;
        private string distroSource;
        private Helpers helpers = new();
        private Windows.Storage.StorageFolder storageDirectory = Windows.Storage.ApplicationData.Current.LocalFolder;

        private Dictionary<string, string> distrosSources = new()
        {
            { "Ubuntu 20.04", "ubuntu:20.04"},
            { "Ubuntu 21.10", "ubuntu:21.10" },
            { "Debian Stable", "debian:stable" },
            { "Debian Testing", "debian:testing" },
            { "Debian Unstable", "debian:unstable"},
            { "Fedora 34", "fedora:34" },
            { "Fedora 35", "fedora:35" },
            { "Arch Linux", "archlinux:base-devel" },
            { "CentOS 7", "centos:7" },
            { "Kali Linux", "kalilinux/kali-rolling" },
            { "Parrot Security", "parrotsec/security" },
            { "Alpine Linux", "alpine:latest" }
        };


        public RegisterNewDistro_Page()
        {
            this.InitializeComponent();
            SupportedDistroListbox.ItemsSource = distrosSources.Keys;
            SupportedDistroListbox.SelectedIndex = 0;

        }
        private async void ChooseDistroTarball(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();

            IntPtr hwnd = (App.Current as App).MainWindowWindowHandle;

            var initializeWithWindow = filePicker.As<IInitializeWithWindow>();
            initializeWithWindow.Initialize(hwnd);

            filePicker.FileTypeFilter.Add(".tar");
            filePicker.FileTypeFilter.Add(".tar.bz");

            var folder = await filePicker.PickSingleFileAsync();
            distroTarballPath = folder != null ? folder.Path : string.Empty;
        }

        [ComImport]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IInitializeWithWindow
        {
            void Initialize(IntPtr hwnd);
        }
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }
        private async void registerDistroProceedButton_Click(object sender, RoutedEventArgs e)
        {
            DockerDownloader dockerDownloader = new(App.tmpDirectory.Path,
                new PlatformHelpers(Path.Combine(App.executableLocation, "dep", "bsdtar.exe"), HttpProgressCallback));


            string image;
            var distroName = distroNameTextBox.Text;
            string userName = "";
            string password = "";
            

            if (distroName == "" || distroName.Contains(" "))
            {
                await showErrorModal();
                return;
            }
            if (newUserSwitch.IsOn == true && distroSource == "Supported distro list")
            {
                userName = userNameTextBox.Text;
                if (userName == "" || userName.Contains(" "))
                {
                    await showErrorModal();
                    return;
                }
                string passwd1 = password1TextBox.Password.ToString();
                string passwd2 = password2TextBox.Password.ToString();
                if (passwd1 != passwd2)
                {
                    ContentDialog wrongPasswordDialog = new ContentDialog();
                    wrongPasswordDialog.XamlRoot = registerDistroProceedButton.XamlRoot;
                    wrongPasswordDialog.Title = "Passwords are not identical";
                    wrongPasswordDialog.CloseButtonText = "Cancel";
                    wrongPasswordDialog.DefaultButton = ContentDialogButton.Close;
                    var result = await wrongPasswordDialog.ShowAsync();
                    return;
                }
                else
                {
                    password = passwd1;
                }
            }


            switch (distroSource)
            {
                case "Supported distro list":
                    image = distrosSources[SupportedDistroListbox.SelectedItem as string];
                    registeringStatusTextBlock.Text = $"Downloading the {distroName} distribution";
                    registerDistroProgressBar.IsIndeterminate = false;
                    registeringStatusTextBlock.Visibility = Visibility.Visible;
                    registerDistroProgressBar.Visibility = Visibility.Visible;

                    try
                    {
                        await dockerDownloader.DownloadImage(image);
                    }
                    catch (DockerDownloader.DockerException)
                    {
                        registeringStatusTextBlock.Visibility = Visibility.Collapsed;
                        registerDistroProgressBar.Visibility = Visibility.Collapsed;
                        await showErrorModal();
                        return;
                    }

                    registerDistroProgressBar.Visibility = Visibility.Collapsed;
                    registeringStatusTextBlock.Visibility = Visibility.Collapsed;

                    await dockerDownloader.CombineLayers();
                    await RegisterDistro(distroName, Path.Combine(App.tmpDirectory.Path, "install.tar.bz"));
                    break;
                case "Docker Hub":
                    image = dockerImageTextBox.Text;
                    registeringStatusTextBlock.Text = $"Downloading the {distroName} distribution";
                    registerDistroProgressBar.IsIndeterminate = false;
                    registeringStatusTextBlock.Visibility = Visibility.Visible;
                    registerDistroProgressBar.Visibility = Visibility.Visible;

                    try
                    {
                        await dockerDownloader.DownloadImage(image);
                    }
                    catch (DockerDownloader.DockerException)
                    {
                        registeringStatusTextBlock.Visibility = Visibility.Collapsed;
                        registerDistroProgressBar.Visibility = Visibility.Collapsed;
                        await showErrorModal();
                        return;
                    }
                    
                    registerDistroProgressBar.Visibility = Visibility.Collapsed;
                    registeringStatusTextBlock.Visibility = Visibility.Collapsed;

                    await dockerDownloader.CombineLayers();
                    await RegisterDistro(distroName, Path.Combine(App.tmpDirectory.Path, "install.tar.bz"));
                    break;
                case "Local hard drive":
                    if(File.Exists(distroTarballPath))
                    {
                        await RegisterDistro(distroName, distroTarballPath);

                    }
                    else
                    {
                        await showErrorModal();
                        return;
                    }
                    
                    break;
            }

            string postInstallCommand = String.Join(
                "pwconv; ",
                "grpconv; ",
                "chmod 0744 /etc/shadow; ",
                "chmod 0744 /etc/gshadow; ",
                "chown -R root:root /bin/su; ",
                "chmod 755 /bin/su; ",
                "chmod u+s /bin/su; ",
                "touch /etc/fstab; "
            );
            await helpers.ExecuteCommandInWSLAsync(distroName, postInstallCommand);

            if(distroSource == "Supported distro list")
            {
                ShowProgressBar($"Configuring the {distroName} distrbution");
                string configureCommand = String.Join(
                    // apt
                    "if command -v apt; then apt-get -y update; apt-get -y install sudo bash curl; fi; ",
                    // pacman
                    "if command -v pacman; then pacman -Syu --noconfirm; pacman -S --noconfirm sudo bash curl; fi; ",
                    // apk
                    "if command -v apk; then apk update; apk add sudo shadow bash curl; fi; ",
                    // dnf
                    "if command -v dnf; then dnf -y update; dnf -y install sudo bash passwd curl; fi; "
                );
                await helpers.ExecuteCommandInWSLAsync(distroName, configureCommand);

                if (newUserSwitch.IsOn == true)
                {


                    await helpers.ExecuteCommandInWSLAsync(distroName, $"useradd -m {userName} -s $(type -p bash)");
                    await helpers.ExecuteCommandInWSLAsync(distroName, $"echo '{userName}:{password}' | chpasswd");

                    if (isAdminSwitch.IsOn == true)
                    {
                        await helpers.ExecuteCommandInWSLAsync(distroName, $"echo '{userName} ALL=(ALL:ALL) ALL' >> /etc/sudoers; ");
                    }

                    await helpers.ExecuteCommandInWSLAsync(distroName, $"echo '[user]' >> /etc/wsl.conf");
                    await helpers.ExecuteCommandInWSLAsync(distroName, $"echo 'default={userName}' >> /etc/wsl.conf");

                    if (winHelloSwitch.IsOn == true)
                    {
                        string pamPath = Path.Combine(App.executableLocation, @"dep\pam_wsl_hello.so");
                        string authPath = Path.Combine(App.executableLocation, @"dep\WindowsHelloBridge.exe");
                        string command = String.Concat(

                            "if [ -d \"/lib/x86_64-linux-gnu/security/\" ]; then ",
                                $"cp $(wslpath \"{pamPath}\") /lib/x86_64-linux-gnu/security/; ",
                                "chown root:root /lib/x86_64-linux-gnu/security/pam_wsl_hello.so; ",
                                "chmod 644 /lib/x86_64-linux-gnu/security/pam_wsl_hello.so; ",
                                "chmod +x /lib/x86_64-linux-gnu/security/pam_wsl_hello.so; ",
                            "fi; ",

                            "if [ -d \"/lib64/security/\" ]; then ",
                                $"cp -f $(wslpath \"{pamPath}\") /lib64/security/; ",
                                "chown root:root /lib64/security/pam_wsl_hello.so; ",
                                "chmod 644 /lib64/security/pam_wsl_hello.so; ",
                                "chmod +x /lib64/security/pam_wsl_hello.so; ",
                            "fi; ",

                            "if [ -d \"/usr/lib/security\" ]; then ",
                                $"cp $(wslpath \"{pamPath}\") /lib/x86_64-linux-gnu/security/; ",
                                "chown root:root /lib/x86_64-linux-gnu/security/pam_wsl_hello.so; ",
                                "chmod 644 /lib/x86_64-linux-gnu/security/pam_wsl_hello.so; ",
                                "chmod +x /lib/x86_64-linux-gnu/security/pam_wsl_hello.so; ",
                            "fi; ",

                            "if [ -d \"/lib/security\" ]; then ",
                                $"cp $(wslpath \"{pamPath}\") /lib/security/; ",
                                "chown root:root /lib/security/pam_wsl_hello.so; ",
                                "chmod 644 /lib/security/pam_wsl_hello.so; ",
                                "chmod +x /lib/security/pam_wsl_hello.so; ",
                            "fi; ",
                            "sed -i \"1a\\\\auth       sufficient pam_wsl_hello.so\" /etc/pam.d/sudo; ",
                            "mkdir -p /etc/pam_wsl_hello/; ",
                            $"echo \"authenticator_path = \\\"$(wslpath \"{authPath}\")\\\"\" >> /etc/pam_wsl_hello/config; ",
                            "echo \"win_mnt = \\\"/mnt/c\\\"\" >> /etc/pam_wsl_hello/config; ",
                            $"su - {userName} -c '$(wslpath \"{authPath}\") creator \"pam_wsl_hello_{userName}\"'; ",
                            "mkdir -p /etc/pam_wsl_hello/public_keys; ",
                            $"cp /home/{userName}/pam_wsl_hello_{userName}.pem /etc/pam_wsl_hello/public_keys/; ",
                            $"rm -f /home/{userName}/pam_wsl_hello_{userName}.pem; "
                        );
                        Trace.WriteLine($"cmd: {command}");
                        await helpers.ExecuteCommandInWSLAsync(distroName, command);
                    }
                }

                if (pythonCheckbox.IsChecked == true)
                {
                    string cmd = String.Concat(
                        // apt
                        "if command -v apt; then apt -y install python3 python3-pip python3-dev; fi; ",
                        // pacman
                        "if command -v pacman; then pacman -S --noconfirm python; fi; ",
                        // apk
                        "if command -v apk; then apk add python3 py3-pip; fi; ",
                        // dnf
                        "if command -v dnf; then dnf -y install python; fi; "
                    );
                    await helpers.ExecuteCommandInWSLAsync(distroName, cmd);
                }

                if (nodeCheckbox.IsChecked == true)
                {
                    string cmd = String.Concat(
                        // apt
                        "if command -v apt; then curl -fsSL https://deb.nodesource.com/setup_17.x | bash -; apt -y install nodejs; fi; ",
                        // pacman
                        "if command -v pacman; then pacman -S --noconfirm nodejs npm; fi; ",
                        // apk
                        "if command -v apk; then apk add nodejs npm; fi; ",
                        // dnf
                        "if command -v dnf; then dnf -y install nodejs npm; fi; "
                    );
                    await helpers.ExecuteCommandInWSLAsync(distroName, cmd);
                }

                if (goCheckbox.IsChecked == true)
                {
                    string cmd = String.Concat(
                        // apt
                        "if command -v apt; then apt -y install golang-go; mkdir -p $HOME/go; echo 'export GOPATH=$HOME/go' >> $HOME/.bashrc; source $HOME/.bashrc; fi; ",
                        // pacman
                        "if command -v pacman; then pacman -Syu --noconfirm; pacman -S --noconfirm go; mkdir -p $HOME/go; echo 'export GOPATH=$HOME/go' >> .bashrc; source .bashrc; fi; ",
                        // apk
                        "if command -v apk; then apk update; apk add go; fi; ",
                        // dnf
                        "if command -v dnf; then dnf -y update; dnf -y install golang; fi; "
                    );
                    await helpers.ExecuteCommandInWSLAsync(distroName, cmd);
                }

                if (cppCheckbox.IsChecked == true)
                {
                    string cmd = String.Concat(
                        // apt
                        "if command -v apt; then apt -y install gcc clang g++ ; fi; ",
                        // pacman
                        "if command -v pacman; then pacman -S --noconfirm gcc clang; fi; ",
                        // apk
                        "if command -v apk; then apk add build-base; fi; ",
                        // dnf
                        "if command -v dnf; then dnf -y install gcc clang gcc-c++; fi; "
                    );
                    await helpers.ExecuteCommandInWSLAsync(distroName, cmd);
                }

                if (haskellCheckbox.IsChecked == true)
                {
                    string cmd = String.Concat(
                        // apt
                        "if command -v apt; then apt -y install haskell-platform; fi; ",
                        // pacman
                        "if command -v pacman; then pacman -S --noconfirm ghc stack cabal-install; fi; ",
                        // apk
                        //"if command -v apk; then apk update; apk add sudo shadow bash; fi; ",
                        // dnf
                        "if command -v dnf; then dnf -y install stack ghc haskell-platform cabal-install; fi; "
                    );
                    await helpers.ExecuteCommandInWSLAsync(distroName, cmd);
                }

                if (javaCheckbox.IsChecked == true)
                {
                    string cmd = String.Concat(
                        // apt
                        "if command -v apt; then apt -y install default-jre default-jdk; fi; ",
                        // pacman
                        "if command -v pacman; then pacman -S --noconfirm jre-openjdk jdk-openjdk; fi; ",
                        // apk
                        "if command -v apk; then apk add openjdk11; fi; ",
                        // dnf
                        "if command -v dnf; then dnf -y install java-11-openjdk.x86_64 java-11-openjdk-devel.x86-64; fi; "
                    );
                    await helpers.ExecuteCommandInWSLAsync(distroName, cmd);
                }

                await helpers.ExecuteProcessAsynch("wsl.exe", $"-t {distroName}");

                HideProgressBar();

            }

            
            await ShowRegisteredModal(distroName);
        }

        public async Task ShowRegisteredModal(string name)
        {
            ContentDialog registerDistroDialog = new ContentDialog();
            registerDistroDialog.XamlRoot = registerDistroProceedButton.XamlRoot;
            registerDistroDialog.Title = $"{name} has been registered";
            registerDistroDialog.SecondaryButtonText = "Close";
            registerDistroDialog.PrimaryButtonText = "Run distribution";
            registerDistroDialog.DefaultButton = ContentDialogButton.Primary;

            var result = await registerDistroDialog.ShowAsync();

            if (result == ContentDialogResult.Secondary)
            {
                this.Frame.Navigate(typeof(ManageDistrosPage));
            }
            else if (result == ContentDialogResult.Primary)
            {
                helpers.StartWSLDistroAsync(name);
                this.Frame.Navigate(typeof(ManageDistrosPage));
            }

        }

        public void HttpProgressCallback(Windows.Web.Http.HttpProgress progress)
        {
            if (progress.TotalBytesToReceive == null) return;

            registerDistroProgressBar.Minimum = 0;
            registerDistroProgressBar.Maximum = (double)progress.TotalBytesToReceive;
            registerDistroProgressBar.Value = progress.BytesReceived;
        }
        
        public async Task RegisterDistro(string distroName, string tarballPath)
        {
            ShowProgressBar($"Registering the {distroName} distribution");

            var distroStoragePath = Path.Combine(storageDirectory.Path, distroName);
            Directory.CreateDirectory(distroStoragePath);

            await helpers.ExecuteProcessAsynch("wsl.exe", $"--import {distroName} {distroStoragePath} {tarballPath}");

            HideProgressBar();
        }

        private void distroSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            distroSource = e.AddedItems[0].ToString();
            switch (distroSource)
            {
                case "Supported distro list":
                    if (SupportedDistroListbox != null)
                    {
                        SupportedDistroListbox.Visibility = Visibility.Visible;
                        SupportedDistroListbox.SelectedIndex = 0;
                        tarballFileChooserButton.Visibility = Visibility.Collapsed;
                        dockerImageTextBox.Visibility = Visibility.Collapsed;
                        dockerImageDescription.Visibility = Visibility.Collapsed;
                        tarballDescription.Visibility = Visibility.Collapsed;
                    }
                    if (createUserStackPanel != null)
                    {
                        createUserStackPanel.Visibility = Visibility.Visible;
                    }
                    break;
                case "Docker Hub":
                    dockerImageTextBox.Visibility = Visibility.Visible;
                    SupportedDistroListbox.Visibility = Visibility.Collapsed;
                    tarballFileChooserButton.Visibility = Visibility.Collapsed;
                    createUserStackPanel.Visibility = Visibility.Collapsed;
                    dockerImageDescription.Visibility = Visibility.Visible;
                    tarballDescription.Visibility = Visibility.Collapsed;
                    break;
                case "Local hard drive":
                    tarballFileChooserButton.Visibility = Visibility.Visible;
                    SupportedDistroListbox.Visibility = Visibility.Collapsed;
                    dockerImageTextBox.Visibility = Visibility.Collapsed;
                    createUserStackPanel.Visibility = Visibility.Collapsed;
                    dockerImageDescription.Visibility = Visibility.Collapsed;
                    tarballDescription.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void newUserSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var isEnabled = newUserSwitch.IsOn;
            if (isEnabled)
            {
                userNameTextBox.Visibility = Visibility.Visible;
                passwordTextBoxGrid.Visibility = Visibility.Visible;
                isAdminSwitch.Visibility = Visibility.Visible;
                winHelloSwitch.Visibility = Visibility.Visible;
            }
            else
            {
                userNameTextBox.Visibility = Visibility.Collapsed;
                passwordTextBoxGrid.Visibility = Visibility.Collapsed;
                isAdminSwitch.Visibility = Visibility.Collapsed;
                winHelloSwitch.Visibility = Visibility.Collapsed;
            }
        }
        private void ShowProgressBar(string text)
        {
            registeringStatusTextBlock.Text = text;
            registeringStatusTextBlock.Visibility = Visibility.Visible;
            registerDistroProgressBar.Visibility = Visibility.Visible;
            registerDistroProgressBar.IsIndeterminate = true;
        }

        private void HideProgressBar()
        {
            registerDistroProgressBar.IsIndeterminate = false;
            registerDistroProgressBar.Visibility = Visibility.Collapsed;
            registeringStatusTextBlock.Visibility = Visibility.Collapsed;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ManageDistrosPage));
        }

        private async Task showErrorModal()
        {
            ContentDialog errorDialog = new ContentDialog();
            errorDialog.XamlRoot = registerDistroProceedButton.XamlRoot;
            errorDialog.Title = "Error";
            errorDialog.CloseButtonText = "Cancel";
            errorDialog.DefaultButton = ContentDialogButton.Close;
            errorDialog.Content = "There were problems with registering your distribution.";
            await errorDialog.ShowAsync();
        }
    }


}
