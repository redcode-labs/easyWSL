using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace easyWSL
{

    public sealed partial class MoreInfoDialog : Page
    {
        private ManageDistrosPage manageDistrosPage = new();

        public string name = "";
        public string version = "";
        public string path = "";

        public MoreInfoDialog(string name, string version, string path)
        {
            this.InitializeComponent();

            nameTextBox.Text = name;
            versionTextBlock.Text = version;
            pathTextBox.Text = path;
        }

        private void openVHDLocationButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", pathTextBox.Text);
        }
    }
}
