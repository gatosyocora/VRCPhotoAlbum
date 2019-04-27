using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

// ref
// http://laicos.hatenablog.com/entry/2018/01/15/235825
// https://www.techlive.tokyo/archives/1987
// http://yuuxxxx.hatenablog.com/entry/2014/02/01/232320

namespace VRCPhotoAlbum
{
    /// <summary>
    /// SubFolderWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SubFolderWindow : Window
    {
        public class Photo
        {
            public string Uri { get; set; }

            public Photo(string uri)
            {
                this.Uri = uri;
            }
        }

        public ObservableCollection<Photo> Photos;

        public SubFolderWindow(string folderPath)
        {
            InitializeComponent();

            var folderPaths = folderPath.Split('\\');

            FolderNameLabel.Content = folderPaths[folderPaths.Length - 1];

            Photos = new ObservableCollection<Photo>();
            PhotoListView.ItemsSource = Photos;

            var filesInSubFolder = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

            Init(filesInSubFolder);

        }

        public async void Init(string[] files)
        {
            await AddPhotoAsync(files);
        }

        public async Task AddPhotoAsync(string[] files)
        {
            foreach(var filePath in files)
            {
                await Task.Delay(100);
                Photos.Add(new Photo(filePath));
            }
        }

        private void PhotoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var image = e.AddedItems.Cast<Photo>().ToList<Photo>();
            var process = System.Diagnostics.Process.Start(image[0].Uri);
        }
    }
}
