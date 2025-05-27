using JaszCore.App;
using JaszCore.Common;
using JaszCore.Core;
using JaszCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JaszCore.Services
{
    public class BaseStorageService
    {
        private const string FOLDER_BASE = ".JaszCore";

        private static LoggerService Log => ServiceLocator.Get<LoggerService>();
        private static IAppClient AppClient => ServiceLocator.Get<IAppClient>();


        public bool RemoveFile(StorageArea area, string fileName)
        {
            Log.Debug($"BaseStorageService.RemoveFile {area} {fileName}");
            try
            {
                UserPath(area, fileName).LookupFileByPath()?.Delete();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to delete {area} {fileName}");
            }
            return false;
        }

        public bool SaveFile(StorageArea area, string fileName, byte[] data)
        {
            Log.Debug($"BaseStorageService.SaveFile {area} {fileName}");
            try
            {

                UserPath(area, fileName).WriteToPathAsync(data);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write to file {0} {1}", 0, area, fileName);
            }
            return false;
        }

        public async Task<byte[]> ReadFileAsync(StorageArea area, string fileName)
        {
            Log.Debug($"BaseStorageService.ReadFileAsync {area} {fileName}");

            try
            {
                return await UserPath(area, fileName).ReadFromPath();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write to file {0} {1}", 0, area, fileName);
            }
            return null;
        }

        public string FilePath(StorageArea area, string fileName)
        {
            FileInfo file = UserPath(area, fileName).LookupFileByPath();
            return file?.FullName;
        }


        protected static FilePathUtils UserPath(StorageArea area, string file = null)
        {
            string systemID = AppClient.GetSystemId();
            if (file != null)
            {
                return new FilePathUtils(new string[] { FOLDER_BASE, systemID.ToFileName() }.Concat(new string[] { area.ToString(), file }).ToArray());
            }
            return new FilePathUtils(new string[] { FOLDER_BASE, systemID.ToFileName() }.Concat(new string[] { area.ToString() }).ToArray());
        }

        protected static FilePathUtils InternalPath(params string[] parts)
        {
            _ = AppClient.GetSystemId();
            if (parts != null && parts.Length > 0)
            {
                return new FilePathUtils(new string[] { FOLDER_BASE }.Concat(parts).ToArray());
            }
            return new FilePathUtils(new string[] { FOLDER_BASE });
        }

        public async Task LogInernalFilesAsync()
        {
            Log.Debug("BaseStorageService.LogInernalFilesAsync");
            FilePathUtils root = InternalPath();
            DirectoryInfo rootFolder = root.LookupFolder();
            if (rootFolder != null)
            {
                await LogFolderContent(rootFolder);
            }
        }

        protected async Task LogFolderContent(DirectoryInfo folder)
        {
            Log.Debug($"Folder: {folder.FullName}");
            IList<FileInfo> files = folder.GetFiles();
            foreach (var file in files)
            {
                FileInfo fi = new FileInfo(file.FullName);
                Log.Debug($"File: {file.FullName}\t{fi.Length}");
            }
            IList<DirectoryInfo> folders = folder.GetDirectories();
            foreach (var child in folders)
            {
                await LogFolderContent(child);
            }
        }


        protected async Task<bool> SaveObjectAsync(StorageArea area, string fileName, object data)
        {
            Log.Debug($"BaseStorageService.SaveObjectAsync {area} {fileName}");
            try
            {
                FilePathUtils path = UserPath(area, fileName);
                path.Parent.EnsurePathExists();

                FilePathUtils workFile = UserPath(area, $"{DateTime.Now.Millisecond}");
                Log.Debug($"BaseStorageService.SaveObjectAsync Saving object to {workFile} temporary file");

                FileInfo tempFile = null;
                try
                {
                    tempFile = workFile.CreateOrReplaceFile();
                    await tempFile.SaveToFile(data);
                    tempFile.Create();
                    tempFile = null;
                }
                finally
                {
                    if (tempFile != null)
                    {
                        tempFile.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "BaseStorageService.SaveObjectAsync Failed to write to file {0} {1}", 0, area, fileName);
            }
            return false;
        }

        protected async Task<T> LoadObjectAsync<T>(StorageArea area, string fileName)
        {
            Log.Debug($"BaseStorageService.LoadObjectAsync {area} {fileName}");

            try
            {
                FileInfo file = UserPath(area, fileName).LookupFileByPath();
                if (file != null)
                {
                    return await file.LoadFromFile<T>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write to file {0} {1}", 0, area, fileName);
            }
            return default;
        }

    }
}
