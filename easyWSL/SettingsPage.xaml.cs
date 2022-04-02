using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Management;
using System.Collections.Generic;
using System.IO;
using Windows.Storage.Pickers;

namespace easyWSL
{
    public sealed partial class SettingsPage : Page
    {
        private Helpers helpers = new();
        public SettingsPage()
        {
            this.InitializeComponent();
            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wslconfig");
            SetMax();
            if (File.Exists(configPath))
            {
                LoadConfig(configPath);
            }
            else
            {
                SetDefaults();
            }
            if(Environment.OSVersion.Version.Build >= 22000)
            {
                windows11OptionsStackPanel.Visibility = Visibility.Visible;
            }

        }

        private double GetRam()
        {
            double ramInMB = 0.0;
            ManagementObjectSearcher Search = new ManagementObjectSearcher("Select * From Win32_ComputerSystem");
            foreach (ManagementObject Mobject in Search.Get())
            {

                ramInMB = Convert.ToDouble(Mobject["TotalPhysicalMemory"]) / 1048576;
            }
            return ramInMB;
        }

        private void SetMax()
        {
            int processors = Environment.ProcessorCount;
            processorsSlider.Maximum = processors;

            double memoryMax = Math.Round(Convert.ToDouble(GetRam()) / 1024, 1);
            memoryNumberBox.Maximum = memoryMax;
        }
        
        private void SetDefaults()
        {
            // Memory
            double halfOfMemory = Convert.ToInt32(GetRam() / 2);
            if(halfOfMemory < 8192)
            {
                memoryNumberBox.Value = Math.Round(halfOfMemory / 1024, 1);
            }
            else
            {
                memoryNumberBox.Value = 8;
            }


            // Processors
            int processors = Environment.ProcessorCount;
            processorsSlider.Value = processors;

            // Swap size
            double swapSize = Convert.ToDouble(GetRam() / 4);
            swapSizeNumberBox.Value = Math.Round(swapSize / 1024, 1);

            // Swap File
            string swapPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AppData\\Local\\Temp\\swap.vhdx";
            swapFileTextBox.Text = swapPath;

            // Kernel path

            // Kernel commandline

            // Localhost forwarding
            localhostForwardingToggle.IsOn = true;

            // Page Reporting
            pageReportingToggle.IsOn = true;

            // Gui applications
            guiApplicationsToggle.IsOn = true;

            // Debug Console
            debugConsoleToggle.IsOn = false;

            // Nested Virtualization
            nestedVirtualisationToggle.IsOn = true;
            
            // VMIdle Timeout
            vmIdleTimeoutNumberBox.Value = 60000;
        }

        private void LoadConfig(string path)
        {
            Dictionary<string, string> configParsed = new();
            foreach (string line in File.ReadLines(path))
            {
                bool isFirstLine = line.Contains("[wsl2]") || line.Contains("[WSL2]");
                if (!isFirstLine)
                {
                    string currentLine = line;
                    currentLine = line.ReplaceLineEndings();
                    currentLine = currentLine.Replace(Environment.NewLine, "");

                    string[] kvp = currentLine.Split("=");
                    if(kvp.Length == 2)
                    {
                        configParsed.Add(kvp[0], kvp[1]);
                    }
                }
            }

            // Memory
            if (configParsed.ContainsKey("memory"))
            {
                string memory = configParsed["memory"];
                if (memory.Contains("MB"))
                {
                    double memoryD = Math.Round(Convert.ToDouble(memory.Replace("MB", "")) / 1024, 1);
                    memoryNumberBox.Value = memoryD;
                }
                else if (memory.Contains("GB"))
                {
                    int memoryINT = Convert.ToInt32(memory.Replace("GB", ""));
                    memoryNumberBox.Value = memoryINT;
                }
            }
            // Processors
            if (configParsed.ContainsKey("processors"))
            {
                int processors = Convert.ToInt32(configParsed["processors"]);
                processorsSlider.Value = processors;
            }
            // Swap size
            if (configParsed.ContainsKey("swap"))
            {
                string swapSize = configParsed["swap"];
                if (swapSize.Contains("MB"))
                {
                    double swapSizeD = Math.Round(Convert.ToDouble(swapSize.Replace("MB", "")) / 1024, 1);
                    swapSizeNumberBox.Value = swapSizeD;
                }
                else if (swapSize.Contains("GB"))
                {
                    int swapSizeINT = Convert.ToInt32(swapSize.Replace("GB", ""));
                    swapSizeNumberBox.Value = swapSizeINT;
                }
            }
            // Swap File
            if (configParsed.ContainsKey("swapfile"))
            {
                string swapPath = configParsed["swapfile"];
                swapPath = swapPath.Replace(@"\\", @"\");
                swapFileTextBox.Text = swapPath;
            }
            // Kernel path
            if (configParsed.ContainsKey("kernel"))
            {
                string kernelPath = configParsed["kernel"];
                kernelPath = kernelPath.Replace(@"\\", @"\");
                kernelPathTextBox.Text = kernelPath;
            }
            // Kernel commandline
            if (configParsed.ContainsKey("kernel"))
            {
                string kernelCmd = configParsed["kernelCommandLine"];
                kernelCommandLineTextBox.Text = kernelCmd;
            }
            // Localhost forwarding
            if (configParsed.ContainsKey("localhostForwarding"))
            {
                bool localhostForwarding = Convert.ToBoolean(configParsed["localhostForwarding"]);
                localhostForwardingToggle.IsOn = localhostForwarding;
            }
            // Page Reporting
            if (configParsed.ContainsKey("pageReporting"))
            {
                bool pageReporting = Convert.ToBoolean(configParsed["pageReporting"]);
                pageReportingToggle.IsOn = pageReporting;
            }
            // Gui applications
            if (configParsed.ContainsKey("guiApplications"))
            {
                bool guiApplications = Convert.ToBoolean(configParsed["guiApplications"]);
                guiApplicationsToggle.IsOn = guiApplications;
            }
            // Debug Console
            if (configParsed.ContainsKey("debugConsole"))
            {
                bool debugConsole = Convert.ToBoolean(configParsed["debugConsole"]);
                debugConsoleToggle.IsOn = debugConsole;
            }
            // Nested Virtualization
            if (configParsed.ContainsKey("nestedVirtualization"))
            {
                bool nestedVirtualization = Convert.ToBoolean(configParsed["nestedVirtualization"]);
                nestedVirtualisationToggle.IsOn = nestedVirtualization;
            }
            // VMIdle Timeout
            if (configParsed.ContainsKey("vmIdleTimeout"))
            {
                int vmIdleTimeout = Convert.ToInt32(configParsed["vmIdleTimeout"]);
                vmIdleTimeoutNumberBox.Value = vmIdleTimeout;
            }

        }

        private void processorsSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (processorsTextBlock != null)
            {
                processorsTextBlock.Text = $"{processorsSlider.Value} cores";

            }

        }

        private void vmIdleTimeoutNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (vmIdleTimeoutTextBlock != null)
            {
                double timeoutInSec = Math.Round(vmIdleTimeoutNumberBox.Value / 1000, 2);
                vmIdleTimeoutTextBlock.Text = $"{timeoutInSec} seconds";
            }
        }

        private void revertToDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            SetDefaults();
        }

        private async void applyButton_Click(object sender, RoutedEventArgs e)
        {
            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wslconfig");
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }

            List<string> configLines = new()  { "[WSL]" };

            // Memory
            int memoryINT = (int)Math.Round(memoryNumberBox.Value * 1024, 0);
            string memory = memoryINT.ToString() + "MB";
            configLines.Add($"memory={memory}");

            // Processors
            int processors = Convert.ToInt32(processorsSlider.Value);
            configLines.Add($"processors={processors}");

            // Swap size
            int swapSizeINT = (int)Math.Round(swapSizeNumberBox.Value, 0);
            string swapSize = swapSizeINT.ToString() + "GB";
            configLines.Add($"swap={swapSize}");

            // Swap File
            string swapPath = swapFileTextBox.Text;
            swapPath = swapPath.Replace(@"\", @"\\");
            configLines.Add($"swapfile={swapPath}");

            // Kernel path
            string kernelPath = kernelPathTextBox.Text;
            if(File.Exists(kernelPath))
            {
                configLines.Add($"kernel={kernelPath}");
            }

            // Kernel commandline
            string commandline = kernelCommandLineTextBox.Text;
            commandline = commandline.Replace(@"\", @"\\");
            configLines.Add($"kernelCommandLine={commandline}");

            // Localhost forwarding
            string localForwarding = localhostForwardingToggle.IsOn.ToString().ToLower();
            configLines.Add($"localhostForwarding={localForwarding}");

            // Page Reporting
            string pageReporting = pageReportingToggle.IsOn.ToString().ToLower();
            configLines.Add($"pageReporting={pageReporting}");

            // Gui applications
            string guiApp = guiApplicationsToggle.IsOn.ToString().ToLower();
            configLines.Add($"guiApplications={guiApp}");

            // Debug Console
            string debug = debugConsoleToggle.IsOn.ToString().ToLower();
            configLines.Add($"debugConsole={debug}");

            // Nested Virtualization
            string nested = nestedVirtualisationToggle.IsOn.ToString().ToLower();
            configLines.Add($"nestedVirtualization={nested}");

            // VMIdle Timeout
            int timeout = Convert.ToInt32(vmIdleTimeoutNumberBox.Value);
            configLines.Add($"vmIdleTimeout={timeout}");

            File.AppendAllLines(configPath, configLines);

            ContentDialog confirmDialog = new ContentDialog();
            confirmDialog.XamlRoot = applyButton.XamlRoot;
            confirmDialog.Title = $"Do you want to relaunch WSL?";
            confirmDialog.Content = "In order to apply the changes you have to relaunch WSL. Be aware that this is going to stop all distributions that are running at the moment.";
            confirmDialog.SecondaryButtonText = "No";
            confirmDialog.PrimaryButtonText = "Yes";
            confirmDialog.DefaultButton = ContentDialogButton.Primary;

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await helpers.ExecuteProcessAsynch("wsl.exe", "--shutdown");
            }
        }
        private async void swapFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();

            IntPtr hwnd = (App.Current as App).MainWindowWindowHandle;
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            filePicker.FileTypeFilter.Add(".vhdx");
            var swapFilePath = await filePicker.PickSingleFileAsync();
            if(swapFilePath != null)
            {
                swapFileTextBox.Text = swapFilePath.Path;
            }
        }

        private async void kernelPathButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();

            IntPtr hwnd = (App.Current as App).MainWindowWindowHandle;
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            filePicker.FileTypeFilter.Add("*");
            var kernelFilePath = await filePicker.PickSingleFileAsync();
            if(kernelFilePath != null)
            {
                kernelPathTextBox.Text = kernelFilePath.Path;
            }
        }

        private async void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialogContent = new();
            ContentDialog aboutDialog = new ContentDialog();
            aboutDialog.XamlRoot = aboutButton.XamlRoot;
            aboutDialog.Title = $"About";
            aboutDialog.Content = aboutDialogContent;
            aboutDialog.DefaultButton = ContentDialogButton.Close;
            aboutDialog.CloseButtonText = "Close";

            await aboutDialog.ShowAsync();
        }
    }
}
