using JaszCore.Common;
using JaszCore.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace JaszCore.Utils
{
    public static class ThreadUtils
    {

        static readonly IDictionary<string, SemaphoreSlim> SemaphoreLookup = new ConcurrentDictionary<string, SemaphoreSlim>();
        static readonly SemaphoreSlim SemaphoreLookupLock = new SemaphoreSlim(1, 1);
        private static LoggerService Log => ServiceLocator.Get<LoggerService>();

        public static async Task<SemaphoreSlim> EnterSyncCall([CallerMemberName] string callerName = "")
        {
            await SemaphoreLookupLock.WaitAsync();
            try
            {
                Log.Debug($"Enter {callerName}");
                if (!SemaphoreLookup.TryGetValue(callerName, out SemaphoreSlim result))
                {
                    result = new SemaphoreSlim(1, 1);
                    SemaphoreLookup.Add(callerName, result);
                }
                return result;
            }
            finally
            {
                SemaphoreLookupLock.Release();
            }
        }

        public static async Task ExitSyncCall([CallerMemberName] string callerName = "")
        {
            await SemaphoreLookupLock.WaitAsync();
            try
            {
                Log.Debug($"Exit {callerName}");
                if (SemaphoreLookup.TryGetValue(callerName, out SemaphoreSlim result))
                {
                    result.Release();
                }
            }
            finally
            {
                SemaphoreLookupLock.Release();
            }
        }
    }
}
