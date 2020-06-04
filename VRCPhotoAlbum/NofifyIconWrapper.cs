using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace VRCPhotoAlbum
{
    public partial class NofifyIconWrapper: Component
    {
        private const string STARTUP_PATH = @"Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        private Timer myTimer;

        private DateTime execTime = new DateTime(2019, 5, 15, 0, 0, 0);

        private bool separatingNow = false;

        public NofifyIconWrapper()
        {
            InitializeComponent();

            toolStripMenuItem_Open.Click += this.toolStripMenuItem_Open_Click;
            toolStripMenuItem_OrganizePhotos.Click += this.toolStripMenuItem_OrganizePhotos_Click;
            toolStripMenuItem_StartUp.Click += this.toolStripMenuItem_StartUp_Click;
            toolStripMenuItem_Exit.Click += this.toolStripMenuItem_Exit_Click;

            myTimer = SetTimer();
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

        private Timer SetTimer()
        {
            Timer timer;

            // 次の日の0時にする
            var nextDay = DateTime.Now.AddDays(1);
            execTime = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0);

            TimerCallback callback = state =>
            {
                if (execTime < DateTime.Now)
                {
                    SeparatePhotos(ref separatingNow);
                    execTime = execTime.AddDays(1);
                }
            };
            
            int M = 60 * 1000;
            timer = new Timer(callback, null, 0, 1 * M); // 30分おきに実行

            return timer;
        }

        private void SeparatePhotos(ref bool separatingNow)
        {
            if (separatingNow) return;

            separatingNow = true;

            int movedPhotoNum = 0;
            var result = MainWindow.MovePhotosToDayNameFolder(out movedPhotoNum);
            if (result)
                MessageBox.Show(movedPhotoNum + "枚の写真をフォルダに分けました");
            else
                MessageBox.Show("写真のフォルダ分けに失敗しました");

            separatingNow = false;
        }

        private void toolStripMenuItem_Open_Click(object sender, EventArgs e)
        {
            var wnd = new MainWindow();
            wnd.Show();
        }

        private void toolStripMenuItem_OrganizePhotos_Click(object sender, EventArgs e)
        {
            SeparatePhotos(ref separatingNow);
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
