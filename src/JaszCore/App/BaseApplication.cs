using JaszCore.Common;
using JaszCore.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace JaszCore.App
{

    public abstract class BaseApplication
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();
        private static IEmailService EmailService => ServiceLocator.Get<IEmailService>();
        private static ISpeechRecognitionService SpeechRecService => ServiceLocator.Get<ISpeechRecognitionService>();

        private readonly string SystemId;
        private readonly string[] AppArgs;
        public string GetSystemId() => SystemId;
        public string[] GetAppArgs() => AppArgs;

        public BaseApplication(IConfiguration iconfiguration, string systemId, string[] systemArgs)
        {
            Log.Debug($"BaseApplication starting...");
            SystemId = systemId;
            AppArgs = systemArgs;
            ServiceLocator.Register<IEmailService>(new EmailService(iconfiguration["Emails:SERVICE_EMAIL"], iconfiguration["Emails:SERVICE_PASS"]));
            ServiceLocator.Register<IDatabaseService>(new DatabaseService(iconfiguration.GetConnectionString("JASZMAIN_CONNECTION"), iconfiguration.GetConnectionString("JASZOUTER_CONNECTION")));
        }

        public static void ApplicationExit(object sender, EventArgs e)
        {
            Log.Debug($"BaseApplication exiting....");
            SpeechRecService.CloseService();
            Log.CloseService();
        }

        public static void ApplicationErrorHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (S.IsSendingErrorEmail())
            {
                EmailService.SendDevErrorEmail(e);
            }
            Log.Error((Exception)e.ExceptionObject, "Error Message");
            Environment.Exit(1);
        }
    }
}
