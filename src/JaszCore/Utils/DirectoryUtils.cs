using JaszCore.Common;
using JaszCore.Services;
using System.IO;
using System.Linq;

namespace JaszCore.Utils
{
    public static class DirectoryUtils
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        public static DirectoryInfo[] GetDirectoriesByCreationTime(string path)
        {
            var info = new DirectoryInfo(path);
            var folders = info.GetDirectories().OrderBy(p => p.CreationTime).ToArray();
            return folders;
        }

        public static FileInfo[] GetFilesByCreationTime(string path)
        {
            var info = new DirectoryInfo(path);
            var files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
            return files;
        }

        public static FileAttributes GetFileAttributes(string path)
        {
            var fileAttributes = File.GetAttributes(path);
            return fileAttributes;
        }

        public static long GetFileSize(string path)
        {
            var fileInfo = new FileInfo(path);
            var size = fileInfo.Length;
            return size;
        }
    }
}
