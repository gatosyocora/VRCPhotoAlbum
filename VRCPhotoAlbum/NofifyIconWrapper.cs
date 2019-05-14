using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VRCPhotoAlbum
{
    public partial class NofifyIconWrapper: Component
    {
        private const string STARTUP_PATH = @"Software\\Microsoft\\Windows\\CurrentVersion\\Run";

        public NofifyIconWrapper()
        {
            InitializeComponent();

            toolStripMenuItem_Open.Click += this.toolStripMenuItem_Open_Click;
            toolStripMenuItem_OrganizePhotos.Click += this.toolStripMenuItem_OrganizePhotos_Click;
            toolStripMenuItem_StartUp.Click += this.toolStripMenuItem_StartUp_Click;
            toolStripMenuItem_Exit.Click += this.toolStripMenuItem_Exit_Click;
        }

        public NofifyIconWrapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private bool CheckRegistedAsStartUp()
        {
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    STARTUP_PATH, true);

            return regKey.GetValue(MainWindow.APP_NAME) != null;
        }

        // https://dobon.net/vb/dotnet/system/osstartuprun.html
        private bool RegistAsStartUpAppToCurrentUser()
        {
            try
            {
                Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    STARTUP_PATH, true);

                if (regKey == null) return false;

                Assembly assembly = Assembly.GetEntryAssembly();

                if (assembly == null) return false;

                regKey.SetValue(MainWindow.APP_NAME, System.Windows.Forms.Application.ExecutablePath);

                regKey.Close();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private bool RemoveFromStartUpAppToCurrentUser()
        {
            try
            {
                Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    STARTUP_PATH, true);

                if (regKey == null) return false;

                Assembly assembly = Assembly.GetEntryAssembly();

                if (assembly == null) return false;

                regKey.DeleteValue(MainWindow.APP_NAME);

                regKey.Close();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void toolStripMenuItem_Open_Click(object sender, EventArgs e)
        {
            var wnd = new MainWindow();
            wnd.Show();
        }

        private void toolStripMenuItem_OrganizePhotos_Click(object sender, EventArgs e)
        {
            MainWindow.MovePhotosToDayNameFolder();
        }

        private void toolStripMenuItem_StartUp_Click(object sender, EventArgs e)
        {
            var registedStartUp = CheckRegistedAsStartUp();

            if (!registedStartUp)
            {
                var result = RegistAsStartUpAppToCurrentUser();

                if (result)
                {
                    MessageBox.Show("スタートアップに登録しました");
                }
            }
            else
            {
                var result = RemoveFromStartUpAppToCurrentUser();

                if (result)
                {
                    MessageBox.Show("スタートアップから解除しました");
                }
            }
        }

        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
