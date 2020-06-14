using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Gatosyocora.VRCPhotoAlbum.Helpers
{
    public static class JsonHelper
    {
        public static bool ExportJsonFile<T>(T jsonClassData, string jsonFilePath)
        {
            File.WriteAllText(jsonFilePath, JsonSerializer.Serialize(jsonClassData, typeof(T)));
            return File.Exists(jsonFilePath);
        }

        public static T ImportJsonFile<T>(string path) where T : class => JsonSerializer.Deserialize(File.ReadAllText(path), typeof(T)) as T;

        public static string GetJsonFilePath() => $@"{Directory.GetCurrentDirectory()}/settings.json";
    }
}
