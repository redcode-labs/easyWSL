using Microsoft.UI.Xaml;
using System;
using PInvoke;
using Windows.Web.Http;
using System.Reflection;

namespace easyWSL
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        IntPtr m_windowhandle;
        public IntPtr MainWindowWindowHandle { get { return m_windowhandle; } }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var isWSLInstalled = WslSdk.CheckIfWSLInstalled();
            if (!isWSLInstalled)
            {
                m_window = new WelcomeWindow();
            }
            else
            {
                m_window = new NavigationRoot_Window();
            }

            m_window.Activate();
            m_window.Title = "easyWSL";
            m_windowhandle = User32.GetActiveWindow();
            User32.ShowWindow(m_windowhandle, User32.WindowShowStyle.SW_MAXIMIZE);
        }

        private Window m_window;

        public static readonly HttpClient httpClient = new();

        public static string executableLocation = Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf(@"\"));
        public static Windows.Storage.StorageFolder tmpDirectory = Windows.Storage.ApplicationData.Current.TemporaryFolder;

    }
}
