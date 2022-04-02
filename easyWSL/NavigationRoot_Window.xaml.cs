using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace easyWSL
{ 
    public sealed partial class NavigationRoot_Window : Window
    {
        public NavigationRoot_Window()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);
        }
        private void mainNavigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                rootFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
                NavView_Navigate(item as NavigationViewItem);
            }
        }

        private void NavView_Navigate(NavigationViewItem item)
        {
            switch (item.Name)
            {
                case "manageDistributionsButton":
                    rootFrame.Navigate(typeof(ManageDistrosPage));
                    break;

                case "addNewDistributionButton":
                    rootFrame.Navigate(typeof(RegisterNewDistro_Page));
                    break;

                case "manageSnapshots":
                    rootFrame.Navigate(typeof(ManageSnapshotsPage));
                    break;
            }
        }
    }
}
