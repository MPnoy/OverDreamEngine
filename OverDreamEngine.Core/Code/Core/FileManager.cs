using System.IO;
using System.Text;

namespace ODEngine.Core
{
    public static class FileManager
    {
        private static string dataPath = "Data/";

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }

        public static byte[] DataReadAllBytes(string path)
        {
            return File.ReadAllBytes(dataPath + NormalizePath(path));
        }

        public static string DataReadAllText(string path)
        {
            return File.ReadAllText(dataPath + NormalizePath(path));
        }

        public static string DataReadAllText(string path, Encoding encoding)
        {
            return File.ReadAllText(dataPath + NormalizePath(path), encoding);
        }

        public static string[] DataReadAllLines(string path)
        {
            return File.ReadAllLines(dataPath + NormalizePath(path));
        }

        public static byte[] SystemReadAllBytes(string path)
        {
            return File.ReadAllBytes(NormalizePath(path));
        }

        public static string SystemReadAllText(string path)
        {
            return File.ReadAllText(NormalizePath(path));
        }

        public static string SystemReadAllText(string path, Encoding encoding)
        {
            return File.ReadAllText(NormalizePath(path), encoding);
        }

        public static string[] SystemReadAllLines(string path)
        {
            return File.ReadAllLines(NormalizePath(path));
        }

        public static Stream DataGetReadStream(string path)
        {
            return new FileStream(dataPath + NormalizePath(path), FileMode.Open, FileAccess.Read);
        }

        public static Stream SystemGetReadStream(string path)
        {
            return new FileStream(NormalizePath(path), FileMode.Open, FileAccess.Read);
        }

        public static Stream GetWriteStream(string path, FileMode fileMode)
        {
            CreateDirectory(path);
            return new FileStream(NormalizePath(path), fileMode, FileAccess.Write);
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            CreateDirectory(path);
            File.WriteAllBytes(NormalizePath(path), bytes);
        }

        public static void WriteAllText(string path, string text, Encoding encoding)
        {
            CreateDirectory(path);
            File.WriteAllText(NormalizePath(path), text, encoding);
        }

        private static void CreateDirectory(string path)
        {
            var folderPath = Path.GetDirectoryName(path);

            if (folderPath.Length > 0 && !Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public static void AppendAllText(string path, string text)
        {
            File.AppendAllText(NormalizePath(path), text);
        }

        public static void AppendAllText(string path, string text, Encoding encoding)
        {
            File.AppendAllText(NormalizePath(path), text, encoding);
        }

        public static bool DataExists(string path)
        {
            return File.Exists(dataPath + NormalizePath(path));
        }

        public static bool SystemExists(string path)
        {
            return File.Exists(NormalizePath(path));
        }

        public static bool SystemDirectoryExists(string path)
        {
            return Directory.Exists(NormalizePath(path));
        }

        public static void Delete(string path)
        {
            File.Delete(NormalizePath(path));
        }

        public static string GetRelativePath(string relativeTo, string path)
        {
            return Path.GetRelativePath(NormalizePath(relativeTo), NormalizePath(path));
        }

        public static string GetExtension(string path)
        {
            return Path.GetExtension(NormalizePath(path));
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(NormalizePath(path));
        }

        private static void RemoveDataFromPaths(string[] paths)
        {
            var dataLength = dataPath.Length;

            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = paths[i].Substring(dataLength);
            }
        }

        public static string[] DataGetFiles(string path)
        {
            var ret = Directory.GetFiles(dataPath + NormalizePath(path));
            RemoveDataFromPaths(ret);
            return ret;
        }

        public static string[] DataGetFiles(string path, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var ret = Directory.GetFiles(dataPath + NormalizePath(path), searchPattern, searchOption);
            RemoveDataFromPaths(ret);
            return ret;
        }

        public static string[] SystemGetFiles(string path)
        {
            return Directory.GetFiles(NormalizePath(path));
        }

        public static string[] SystemGetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(NormalizePath(path), searchPattern);
        }

        public static string[] SystemGetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(NormalizePath(path), searchPattern, searchOption);
        }
    }
}