using JaszCore.App;
using JaszCore.Common;
using Jazatar.App;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using static JaszCore.Common.S;

namespace Jaztar
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(BaseApplication.ApplicationExit);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(BaseApplication.ApplicationErrorHandler);
            if (args == null || args.Length == 0 || args[0].Length < 1)
            {
                Console.WriteLine("Stopping no valid arguments were set...");
                Environment.Exit(1);
            }
            StartupApplication(args);
        }

        private static void StartupApplication(string[] args)
        {
            Console.WriteLine("Program Start");

            if (args.Length > 1)
            {
                S.AppMode = args[1] == "PROD" ? APP_MODE.PROD : args[1] == "DEV" ? APP_MODE.DEV : APP_MODE.NONE;
            }

            var rootPath = new DirectoryInfo(Directory.GetCurrentDirectory());


            var orgProperties = Path.Combine(rootPath.Parent.Parent.Parent.Parent.FullName, S.ORG_NAME.ToLower(), S.ORG_NAME, S.PROP_FILE);
            var appProperties = Path.Combine(rootPath.Parent.Parent.FullName, S.PROP_FILE);

            var propertiesPath = File.Exists(appProperties) ? appProperties : File.Exists(orgProperties) ? orgProperties : null;

            if (propertiesPath != null)
            {
                var builder = new ConfigurationBuilder().SetBasePath(rootPath.FullName).AddJsonFile(propertiesPath, optional: false, reloadOnChange: true).AddEnvironmentVariables();

                IConfiguration config = builder.Build();

                new AppClient(config, "dev", args);
            }

            //var builder = new ConfigurationBuilder().SetBasePath(rootPath.FullName).AddJsonFile(_propertiesPath, optional: false, reloadOnChange: true);
            //var _iConfiguration = builder.Build();
            //new AppClient(_iConfiguration, "dev", args[0]);
        }
    }
}
