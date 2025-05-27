using JaszCore.App;
using JaszCore.Common;
using JaszCore.Services;
using Jazatar.Events;
using Microsoft.Extensions.Configuration;

namespace Jazatar.App
{
    public class AppClient : BaseApplication, IAppClient
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        public AppClient(IConfiguration iconfiguration, string systemId, string[] appArgs) : base(iconfiguration, systemId, appArgs)
        {
            Log.Debug($"AppClient starting...");
            ServiceLocator.Register<IAppClient>(this);
            InitializeApp();
        }

        private void InitializeApp()
        {
            Log.Debug($"AppClient initializing...");
            if (GetAppArgs()[1].Contains("RUN_JAZATAR"))
            {
                new MainEvent().StartEvent();
            }
            if (GetAppArgs()[1].Contains("RUN_ALT"))
            {
                new AltEvent().StartEvent();
            }
        }
    }
}
