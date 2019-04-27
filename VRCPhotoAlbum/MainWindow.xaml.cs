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

        public int ProgressValue { set; get; }

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "VRCPhotoAlbum";

            folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\VRChat";
            FolderPathTextBox.Text = folderPath;

            DayButton.IsEnabled = false;
            PhotoNumButton.IsEnabled = false;
        }

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
            }

        }

        private bool MovePhotosToDayNameFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("フォルダが見つかりません");
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

                if (!Regex.IsMatch(fileName, "VRChat_1920x1080_[0-9_\\.-]+.png"))
                {
                    ProgressValue++;
                    continue;
                }

                var day = fileName.Split('_')[2];
                var di = Directory.CreateDirectory(folderPath + "\\" + day);

                File.Move(fileFullPath, folderPath + "\\" + day + "\\" + fileName);

                ProgressValue++;
            }

            // サブフォルダのリストを取得
            string[] folders = Directory.GetDirectories(
                folderPath, "*", SearchOption.AllDirectories);

            if (folders.Length <= 0)
            {
                MessageBox.Show("日付ごとのフォルダが作成できませんでした/見つかりませんでした");
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

        private void FolderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = e.AddedItems.Cast<string>().ToList<string>();
            if (items.Count() > 0)
                subWin = new SubFolderWindow(folderPath + "\\" + items[0].Split('\t')[0], this);
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

            MessageBox.Show("Finish");
        }
    }
}
