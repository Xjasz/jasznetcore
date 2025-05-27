using JaszCore.Objects;
using OpenTK.Audio.OpenAL;
using System;
using System.IO;
using System.Reflection;

namespace JaszCore.Common
{
    public static class S
    {
        public static readonly string ORG_NAME = Assembly.GetExecutingAssembly().GetName().Name;
        public static readonly string APP_NAME = Assembly.GetEntryAssembly().GetName().Name;
        public static readonly string NL = Environment.NewLine;

        // App Files
        public static readonly string LOG_FILE = $"{Assembly.GetEntryAssembly().GetName().Name}.log";
        public static readonly string STATE_FILE = $"{Assembly.GetEntryAssembly().GetName().Name}.jasz";
        public static readonly string PROP_FILE = "Properties\\appSettings.json";

        // App Directories
        public static readonly string RES_DIR = $"{Directory.GetCurrentDirectory()}\\Resources";
        public static readonly string REQ_DIR = $"{Directory.GetCurrentDirectory()}\\Requests";
        public static readonly string LOG_DIR = $"{Directory.GetCurrentDirectory()}\\Logs";

        // App MimeTypes
        public static readonly string MIME_UNKNOWN = "*/*";
        public static readonly string MIME_JSON = "application/json";
        public static readonly string MIME_XML = "application/xml";
        public static readonly string MIME_PNG = "image/png";

        // App Additional Options
        public static readonly string PROD_EMAIL = "testemail@gmail.com";
        public static readonly bool DEV_EMAIL_ERRORS = false;
        public static readonly bool DEV_INDENT = false;
        public static readonly string DEV_EMAIL = "testemail@gmail.com";
        public static readonly bool PROD_EMAIL_ERRORS = true;
        public static readonly bool PROD_INDENT = false;

        // App Other
        public static string FILE_RUNLINE = $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~   (NEW RUN {DateTime.Now})   ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{S.NL}";
        public static string ERROR_TITLELINE = S.NL + "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% (Error Occurred: " + DateTime.Now + ") %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%";
        public static string ERROR_SPACELINE = S.NL + "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%";

        public static APP_RUN_STATE AppRunState = APP_RUN_STATE.START;
        public enum APP_RUN_STATE { START, STOP, WAIT }

        public static APP_MODE AppMode = APP_MODE.NONE;
        public enum APP_MODE { DEV, PROD, NONE }

        public const ALFormat _ALFORMAT = ALFormat.Mono16;
        public const int _RECORDLENGTH = 1;                                // Length of recording
        public const int _SAMPLERATE = 44100;                              // Samples per second
        public const int _BUFFERLENGTH = _SAMPLERATE * _RECORDLENGTH;      // Record buffer total size
        public const ushort _BITSPERSAMPLE = 16;                           // Mono16 16 bits per sample
        public const ushort _NUMCHANNLES = 1;                              // Mono16 1 channel

        public static string OUTPUT_SOUND_FOLDER = @$"{S.RES_DIR}\";
        private static Random _RANDOM = new Random();

        public static SERVICE_STATE ServiceState { get; set; } = SERVICE_STATE.SPEECH_NONE;

        public enum SERVICE_STATE { SPEECH_NONE = 0, SPEECH_CREATE = 1, SPEECH_UPDATE = 2, SPEECH_RUN = 3, SPEECH_UPDATE_P2 = 4, TEXT_NONE = 10, TEXT_CREATE = 11, TEXT_UPDATE = 12, TEXT_RUN = 13 }

        public static COMMAND_RETURN_STATE CommandReturnState = COMMAND_RETURN_STATE.RESET;
        public enum COMMAND_RETURN_STATE { RESET, NONE }

        public static CommandObject CurrentWakeCommand = new CommandObject(CommandObject.COMMAND_TYPE.WAKE, new string[] { "jazz", "hey jazz", "hey jess", "jess", "just" }, null);
        public static CommandObject DefaultExitCommand = new CommandObject(CommandObject.COMMAND_TYPE.EXIT, new string[] { "stop", "stopped", "end", "quit", "exit", "exited" }, null);
        public static CommandObject DefaultListCommand = new CommandObject(CommandObject.COMMAND_TYPE.LIST, new string[] { "list command", "lists command", "lists commands", "listed command", "lists commanded" }, null);
        public static CommandObject DefaultCreateCommand = new CommandObject(CommandObject.COMMAND_TYPE.CREATE, new string[] { "create command", "creates commands", "created commands", "created command", "crates command", "crated command", "crated commands" }, null);
        public static CommandObject DefaultUpdateCommand = new CommandObject(CommandObject.COMMAND_TYPE.UPDATE, new string[] { "update command", "updated command", "updates command", "updates commands", "updated commands", "of the", "up the" }, null);
        public static CommandObject DefaultBrowserCommand = new CommandObject(CommandObject.COMMAND_TYPE.BROWSER, new string[] { "open browser", "browser", "bowser", "firefox", "fire fox" }, null);
        public static CommandObject DefaultGotoCommand = new CommandObject(CommandObject.COMMAND_TYPE.GOTO, new string[] { "goto", "go to", "go too" }, null);


        public static bool IsIndenting() { return AppMode == APP_MODE.PROD ? PROD_INDENT : DEV_INDENT; }
        public static bool IsSendingErrorEmail() { return AppMode == APP_MODE.PROD ? PROD_EMAIL_ERRORS : DEV_EMAIL_ERRORS; }
        public static string GetErrorEmail() { return AppMode == APP_MODE.PROD ? PROD_EMAIL : DEV_EMAIL; }
        public static string GetReceipient() { return AppMode == APP_MODE.PROD ? PROD_EMAIL : DEV_EMAIL; }
        public static Random GetNewRandom() { return new Random(); }

        public static string GetFileHeaderLine(string type)
        {
            var titleLine = $"  {S.APP_NAME} {type}  ";
            var adjustT = (140 - titleLine.Length) / 2;
            titleLine = adjustT > 0 ? new string('#', titleLine.Length % 2 == 0 ? adjustT : adjustT + 1) + titleLine + new string('#', adjustT) : titleLine;
            var headerLine = $"############################################################################################################################################{S.NL}############################################################################################################################################{S.NL}{titleLine}{S.NL}############################################################################################################################################{S.NL}############################################################################################################################################{S.NL}";
            return headerLine;
        }
    }
}
