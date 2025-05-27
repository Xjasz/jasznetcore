using JaszCore.Common;
using JaszCore.Core;
using JaszCore.Models;
using JaszCore.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace JaszCore.Services
{
    public enum StorageArea
    {
        UserState = 0,
        UserData = 1,
        SharedData = 2,
        Captures = 3,
        Logs = 4,
        Temporary = 100
    }

    [Service(typeof(StateStorageService))]
    public interface IStateStorageService
    {
        Task SaveStateAsync(object state, string type = "state");
        Task<T> LoadStateAsync<T>(string type = "state");
        bool RemoveFile(StorageArea area, string fileName);
        bool SaveFile(StorageArea area, string fileName, byte[] data);
        Task<byte[]> ReadFileAsync(StorageArea area, string fileName);
        string FilePath(StorageArea area, string fileName);
        Task LogInernalFilesAsync();
    }

    public class StateStorageService : BaseStorageService, IStateStorageService
    {
        private static LoggerService Log => ServiceLocator.Get<LoggerService>();


        public async Task SaveStateAsync(object state, string type = "state")
        {
            try
            {
                Log.Debug($"StateStorageService.DoSaveState {type}");

                InternalPath(type).Parent.EnsurePathExists();

                FilePathUtils workFile = InternalPath($"{DateTime.Now.Millisecond}");
                Log.Debug($"Saving state to {workFile} temporary file");

                FileInfo tempFile = null;
                try
                {
                    tempFile = workFile.CreateOrReplaceFile();
                    await tempFile.SaveToFile(state);

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
                Log.Error(ex, "Failed to save state {0}", 0, ex.Message);
            }
        }

        public async Task<T> LoadStateAsync<T>(string type = "state")
        {
            Log.Debug($"StateStorageService.DoLoadState {type} {typeof(T)}");
            try
            {
                FileInfo file = InternalPath(type).LookupFileByPath();
                if (file != null)
                {
                    return await file.LoadFromFile<T>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Load State {0}", 0, ex.Message);
            }
            return default;
        }
    }
}
