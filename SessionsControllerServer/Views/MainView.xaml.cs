using System;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;
using SessionsControllerServer.ViewModels;

namespace SessionsControllerServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly NancyHost _nancyHost;

        public MainWindow()
        {
            InitializeComponent();

            HostConfiguration hostConfigs = new HostConfiguration()
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };
            var url = System.Configuration.ConfigurationManager.AppSettings["url"];
            _nancyHost = new NancyHost(hostConfigs, new Uri(url));
            _nancyHost.Start();

            DataContext = TinyIoCContainer.Current.Resolve<MainViewModel>();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _nancyHost.Stop();
            _nancyHost.Dispose();

            base.OnClosing(e);
        }

        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
