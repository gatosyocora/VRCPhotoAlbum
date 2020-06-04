using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Image = System.Drawing.Image;
using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaToolSharp;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Gatosyocora.VRCPhotoAlbum
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Photo> _showedPhotoList = new ObservableCollection<Photo>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_OnLoaded;
        }

        private void MainWindow_OnLoaded(object sender, EventArgs args)
        {
            try
            {
                var photoList = LoadVRCPhotoList(@"D:\VRTools\vrc_meta_tool\meta_pic");

                foreach (var photo in photoList)
                {
                    _showedPhotoList.Add(photo);
                }
                PhotoListBox.ItemsSource = _showedPhotoList;
            }
            catch (Exception e)
            {
                Debug.Print($"{e.GetType().Name}: {e.Message}");
            }
        }

        private IEnumerable<Photo> LoadVRCPhotoList(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new ArgumentException($"{folderPath} is not exist.");
            }

            return Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories)
                        .Select(x =>
                        new Photo
                        {
                            FilePath = x,
                            OriginalImage = Image.FromFile(x),
                            MetaData = VrcMetaDataReader.Read(File.ReadAllBytes(x))
                        });
        }
    }
}
