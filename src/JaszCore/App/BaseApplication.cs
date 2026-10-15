using JaszCore.Common;
using JaszCore.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace JaszCore.App
{

    public abstract class BaseApplication
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        private readonly string SystemId;
        private readonly string[] AppArgs;
        public string GetSystemId() => SystemId;
        public string[] GetAppArgs() => AppArgs;

        public BaseApplication(IConfiguration iconfiguration, string systemId, string[] systemArgs)
        {
            Log.Debug($"BaseApplication starting...");
            SystemId = systemId;
            AppArgs = systemArgs;
            // ServiceLocator.Register<IEmailService>(new EmailService(iconfiguration["Emails:SERVICE_EMAIL"], iconfiguration["Emails:SERVICE_PASS"]));
            // ServiceLocator.Register<IDatabaseService>(new DatabaseService(iconfiguration.GetConnectionString("JASZMAIN_CONNECTION"), iconfiguration.GetConnectionString("JASZOUTER_CONNECTION")));
        }

        public static void ApplicationExit(object sender, EventArgs e)
        {
            Log.Debug($"BaseApplication exiting....");
            ServiceLocator.TryGet<ISpeechRecognitionService>()?.CloseService();
            Log.CloseService();
        }

        public static void ApplicationErrorHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error((Exception)e.ExceptionObject, "Error Message");
            var emailService = ServiceLocator.TryGet<IEmailService>();
            if (S.IsSendingErrorEmail() && emailService != null)
            {
                emailService.SendDevErrorEmail(e);
            }
            Environment.Exit(1);
        }
    }
}
