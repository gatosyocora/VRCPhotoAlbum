using System.Windows;

namespace Gatosyocora.VRCPhotoAlbum
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += App_StartUp;
        }

        private void App_StartUp(object sender, StartupEventArgs e)
        {
            Views.MainWindow.Instance.Show();
        }
    }
}
