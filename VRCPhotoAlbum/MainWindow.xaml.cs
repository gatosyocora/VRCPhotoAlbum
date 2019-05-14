using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Windows.Forms;
using TaskScheduler;

// ref
// https://qiita.com/wonderful_panda/items/36bf500094cc42f7ea97

namespace VRCPhotoAlbum
{
    public class FolderList
    {
        public List<DayFolder> subFolders { set; get; }

        public FolderList()
        {
            subFolders = new List<DayFolder>();
        }

        public void AddSubFolder(string fullPath)
        {
            this.subFolders.Add(new DayFolder(fullPath));
        }

        public List<string> GetSubFolderInfoForList()
        {
            var infos = new List<string>();

            foreach (var subFolder in subFolders)
            {
                infos.Add(subFolder.GetTextForList());
            }

            return infos;
        }

        public void SortAscendingSubFoldersByPhotoNum()
        {
            subFolders = subFolders.OrderByDescending(x => x.photoNum).ToList<DayFolder>();
        }

        public void SortDescendingSubFoldersByDay()
        {
            subFolders = subFolders.OrderBy(x => x.day).ToList<DayFolder>();
        }
    }

    public class Photo
    {
        public string Uri { get; set; }

        public Photo(string uri)
        {
            this.Uri = uri;
        }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public string folderPath;

        private FolderList folderList;

        private SubFolderWindow subWin;

        private DayFolder selectedSubFolder;

        public static string APP_NAME = "VRCPhotoAlbum";

        private const string APP_JSONFILE_NAME = "vrc_photo_album.json";

        public int ProgressValue;

        public MainWindow()
        {
            InitializeComponent();

            folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\VRChat";
            FolderPathTextBox.Text = folderPath;

            // initial state
            DayButton.IsEnabled = false;
            PhotoNumButton.IsEnabled = false;
            SubFolderButton.IsEnabled = false;

            this.Title = APP_NAME;
        }
        
        private bool MovePhotosToDayNameFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                System.Windows.MessageBox.Show("フォルダが見つかりません");
                return false;
            }

            // ディレクトリの作成とファイルの移動
            string[] files = Directory.GetFiles(
                folderPath, "*", SearchOption.TopDirectoryOnly);

            /*
            ProgressValue = 0;
            ProgBar.Maximum = files.Length;
            ProgBar.Value = ProgressValue;
            */

            foreach (var fileFullPath in files)
            {
                var filePaths = fileFullPath.Split('\\');
                var fileName = filePaths[filePaths.Length - 1];

                if (!Regex.IsMatch(fileName, "VRChat_1920x1080_[0-9_\\.-]+.png") || !Regex.IsMatch(fileName, "screen_1920x1080_[0-9_\\.-]+.png"))
                {
                    //ProgressValue++;
                    continue;
                }

                var day = fileName.Split('_')[2];
                var di = Directory.CreateDirectory(folderPath + "\\" + day);

                File.Move(fileFullPath, folderPath + "\\" + day + "\\" + fileName);

                //ProgressValue++;
            }

            // サブフォルダのリストを取得
            string[] folders = Directory.GetDirectories(
                folderPath, "*", SearchOption.AllDirectories);

            if (folders.Length <= 0)
            {
                System.Windows.MessageBox.Show("日付ごとのフォルダが作成できませんでした/見つかりませんでした");
                return false;
            }

            folderList = new FolderList();

            foreach (var folderFullPath in folders)
            {
                folderList.AddSubFolder(folderFullPath);
            }
            FolderListBox.ItemsSource = folderList.GetSubFolderInfoForList();

            return true;
        }

        /// <summary>
        /// 写真を日付ごとのフォルダに分ける
        /// </summary>
        /// <param name="movedPhotoNum">移動した写真の数</param>
        /// <returns>移動に成功したか</returns>
        public static bool MovePhotosToDayNameFolder(out int movedPhotoNum)
        {
            movedPhotoNum = 0;
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\VRChat";

            if (!Directory.Exists(folderPath))
            {
                System.Windows.MessageBox.Show("フォルダが見つかりません");
                return false;
            }

            // ディレクトリの作成とファイルの移動
            string[] files = Directory.GetFiles(
                folderPath, "*", SearchOption.TopDirectoryOnly);

            foreach (var fileFullPath in files)
            {
                var filePaths = fileFullPath.Split('\\');
                var fileName = filePaths[filePaths.Length - 1];

                if (!Regex.IsMatch(fileName, "VRChat_1920x1080_[0-9_\\.-]+.png") || !Regex.IsMatch(fileName, "screen_1920x1080_[0-9_\\.-]+.png"))
                {
                    continue;
                }

                var day = fileName.Split('_')[2];
                var di = Directory.CreateDirectory(folderPath + "\\" + day);

                File.Move(fileFullPath, folderPath + "\\" + day + "\\" + fileName);
                movedPhotoNum++;
            }

            return true;
        }

        private void CreateOrLoadAppInfoFromJson()
        {
            var fs = new FileStream(APP_JSONFILE_NAME, FileMode.OpenOrCreate);
        }

        // https://dobon.net/vb/dotnet/system/associatedapp.html
        /*private void RegisterAsRelatedAppToPng()
        {
            string extension = ".png";
            string commandline = "\"" + Assembly.GetEntryAssembly().Location + "\" \"%1\"";
            string fileType = System.Windows.Forms.Application.ProductName;
            string verb = this.Title;
            string description = this.Title + "で開く(&V)";

            var cmdKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(
                "Folder\\shell\\" + verb + "\\command");
        }*/

        /*
        // https://www.doraxdora.com/blog/2017/10/07/post-2680/
        private static void RegistToSchedule()
        {
            ITaskService taskservice = null;
            try
            {
                taskservice = new TaskScheduler.TaskScheduler();
                taskservice.Connect(null, null, null, null);
                ITaskFolder rootfolder = null;

                try{

                    Assembly mainAssembly = Assembly.GetExecutingAssembly();
                    
                    rootfolder = taskservice.GetFolder("\\");
                    string path = "\\gatosyocora\\" + APP_NAME;

                    ITaskDefinition taskDefinition = taskservice.NewTask(0);

                    // RegistrationInfo
                    IRegistrationInfo registrationInfo = taskDefinition.RegistrationInfo;

                    // Actions
                    IActionCollection actionCollection = taskDefinition.Actions;
                    var execAction = (IExecAction)actionCollection.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);

                    // Triggers
                    ITriggerCollection triggerCollection = taskDefinition.Triggers;
                    //var timeTrigger = (ITimeTrigger)triggerCollection.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME);
                    var dailyTrigger = (IDailyTrigger)triggerCollection.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY);

                    // Settings
                    ITaskSettings taskSettings = taskDefinition.Settings;
                    taskSettings.Hidden = true;
                    taskSettings.Priority = 6;

                    // TimeTrigger
                    //timeTrigger.StartBoundary = "2017-10-02T00:00:00";
                    //timeTrigger.Repetition.Interval = "PT1H";
                    //timeTrigger.Enabled = true;

                    // dairyTrigger
                    dailyTrigger.DaysInterval = 1;
                    dailyTrigger.StartBoundary = "2019-05-12T00:00:00"; //DateTime.Now.AddDays(1).ToString("yyyy-MM-ddT00:00:00");
                    //dailyTrigger.Repetition.Interval = "PT3H";
                    dailyTrigger.Enabled = true;

                    // 全般タブ
                    registrationInfo.Author = "gatosyocora";
                    registrationInfo.Description = "VRChatの写真を日付ごとのフォルダに分ける";
                    IPrincipal principal = taskDefinition.Principal;
                    principal.UserId = $@"{Environment.UserDomainName}\\{Environment.UserName}";
                    principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_S4U;

                    var appDir = Path.GetDirectoryName(mainAssembly.Location);
                    execAction.Path = appDir + "\\MainWindow.xaml.cs";
                    // execAction.Arguments = "引数";
                    execAction.WorkingDirectory = appDir;

                    try
                    {
                        rootfolder.RegisterTaskDefinition(
                            path,
                            taskDefinition,
                            (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
                            null,
                            null,
                            _TASK_LOGON_TYPE.TASK_LOGON_NONE,
                            null
                        );
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.Write(e.ToString());
                        throw e;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                finally
                {
                    if (rootfolder != null)
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(rootfolder);
                }
            }
            finally
            {
                if (taskservice != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(taskservice);
            }
        }
        */

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            folderPath = FolderPathTextBox.Text;

            var result = MovePhotosToDayNameFolder(folderPath);

            if (result)
            {
                DayButton.IsEnabled = true;
                PhotoNumButton.IsEnabled = true;
            }
            else
            {
                DayButton.IsEnabled = false;
                PhotoNumButton.IsEnabled = false;
                return;
            }

            //CreateOrLoadAppInfoFromJson();
        }

        // selected dayfolder from list
        private void FolderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = e.AddedItems.Cast<string>().ToList<string>();
            if (items.Count() > 0)
                subWin = new SubFolderWindow(folderPath + "\\" + items[0].Split('\t')[0], this);

            selectedSubFolder = folderList.subFolders[FolderListBox.SelectedIndex];

            SubFolderButton.IsEnabled = true;
        }

        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            folderList.SortDescendingSubFoldersByDay();
            FolderListBox.ItemsSource = folderList.GetSubFolderInfoForList();
        }

        private void PhotoNumButton_Click(object sender, RoutedEventArgs e)
        {
            folderList.SortAscendingSubFoldersByPhotoNum();
            FolderListBox.ItemsSource = folderList.GetSubFolderInfoForList();
        }

        // selected photo
        private void PhotoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var image = e.AddedItems.Cast<Photo>().ToList<Photo>();
            if (image.Count() > 0)
                System.Diagnostics.Process.Start(image[0].Uri);
        }


        public partial class SubFolderWindow
        {
            public ObservableCollection<Photo> Photos;

            public SubFolderWindow(string folderPath, MainWindow win)
            {

                var folderPaths = folderPath.Split('\\');

                win.FolderNameLabel.Content = folderPaths[folderPaths.Length - 1];

                Photos = new ObservableCollection<Photo>();
                win.PhotoListView.ItemsSource = Photos;

                var filesInSubFolder = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly);

                if (filesInSubFolder != null || filesInSubFolder.Length > 0)
                    Init(filesInSubFolder);
            }

            public async void Init(string[] files)
            {
                await AddPhotoAsync(files);
            }

            public async Task AddPhotoAsync(string[] files)
            {
                foreach (var filePath in files)
                {
                    await Task.Delay(100);
                    Photos.Add(new Photo(filePath));
                }
            }
        }

        // For Debug
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // サブフォルダのリストを取得
            string[] folders = Directory.GetDirectories(
                folderPath, "*", SearchOption.AllDirectories);


            int num = 0;
            folders.Select(x => num += Directory.GetFiles(x, "*.png", SearchOption.TopDirectoryOnly).Length);
            
            /*
            ProgressValue = 0;
            ProgBar.Maximum = num;
            ProgBar.Value = ProgressValue;
            */

            foreach (var folderPath in folders)
            {
                var filesInSubFolder = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly);
                var folderPaths = folderPath.Split('\\');
                var folderName = folderPaths[folderPaths.Length - 1];

                foreach (var file in filesInSubFolder)
                {
                    var upFolder = file.Replace(folderName + "\\", "");
                    Console.Write(file + " -> " + upFolder + "\n");
                    File.Move(file, upFolder);

                    ProgressValue++;
                }
            }

            foreach(var folderPath in folders)
            {
                var filesInSubFolder = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);

                if (filesInSubFolder.Length <= 0)
                    Directory.Delete(folderPath);
            }
        }

        private void SubFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSubFolder != null)
                System.Diagnostics.Process.Start(selectedSubFolder.path); 
        }
    }
}
