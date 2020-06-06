using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Gatosyocora.VRCPhotoAlbum.Helpers
{
    public class JsonHelper
    {
        public static bool ExportJsonFile<T>(T jsonClassData, string jsonFilePath)
        {
            var jsonText = JsonSerializer.Serialize(jsonClassData, typeof(T));
            File.WriteAllText(jsonFilePath, jsonText);
            return File.Exists(jsonFilePath);
        }

        public static T ImportJsonFile<T>(string path) where T : class
        {
            return JsonSerializer.Deserialize(File.ReadAllText(path), typeof(T)) as T;
        }

        public static string GetJsonFilePath()
        {
            var executeFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return executeFolderPath + @"/settings.json";
        }
    }
}
