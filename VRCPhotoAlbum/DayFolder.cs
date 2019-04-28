using System.IO;

namespace VRCPhotoAlbum
{
    public class DayFolder
    {
        private string _path;
        public string path { get { return this._path; } }
        public string day { get; }
        public int photoNum { get; }
        public string ListText { get; }

        public DayFolder(string folderPath)
        {
            this._path = folderPath;
            var paths = folderPath.Split('\\');
            this.day = paths[paths.Length-1];

            string[] filesInSubFolder = Directory.GetFiles(
                this.path, "*", SearchOption.AllDirectories);

            this.photoNum = filesInSubFolder.Length;

            ListText = GetTextForList();
        }

        public string GetTextForList()
        {
            return this.day + '\t' + photoNum + "枚";
        }
    }
}
