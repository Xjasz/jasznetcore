using JaszCore.Common;
using JaszCore.Models;
using JaszCore.Objects;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static JaszCore.Common.S;
using static JaszCore.Objects.CommandObject;

namespace JaszCore.Services
{
    [Service(typeof(CommandService))]
    public interface ICommandService
    {
        void CloseService();
        void Initialize();
        COMMAND_RETURN_STATE RunCommand(CommandObject commandObject);
    }
    public class CommandService : ICommandService
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();
        private static ISpeechSynthesizerService SpeechSynService => ServiceLocator.Get<ISpeechSynthesizerService>();

        ProcessStartInfo _processStartInfo;

        public CommandService()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            Log.Debug($"completed at: {DateTime.Now}");
        }

        public void Initialize()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            Log.Debug($"completed at: {DateTime.Now}");
        }

        public void CloseService()
        {
            Log.Debug($"TextRecognitionService exiting....");
        }

        public COMMAND_RETURN_STATE RunCommand(CommandObject commandObject)
        {
            Log.Debug($"executing at: {DateTime.Now}");
            var commandReturnState = S.COMMAND_RETURN_STATE.NONE;
            var commandTypes = commandObject.GetCommandTypeStringList();
            if (commandObject.CommandType == COMMAND_TYPE.WAKE)
            {
                commandReturnState = S.COMMAND_RETURN_STATE.RESET;
            }
            if (commandObject.CommandType == COMMAND_TYPE.BROWSER)
            {
                var browserPath = Environment.GetEnvironmentVariable("BROWSER_EXE_LOC");
                if (!string.IsNullOrEmpty(browserPath))
                {
                    SpeechSynService.Say("Opening browser.");
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _processStartInfo = new ProcessStartInfo(browserPath);
                        _processStartInfo.UseShellExecute = true;
                        Process.Start(_processStartInfo);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", browserPath);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Process.Start("open", browserPath);
                    }
                }
                else
                {
                    SpeechSynService.Say("Default OS browser was not set.  Please add default browser.");
                }
                commandReturnState = S.COMMAND_RETURN_STATE.RESET;
            }
            if (commandObject.CommandType == COMMAND_TYPE.LIST || commandObject.CommandType == COMMAND_TYPE.UPDATE || commandObject.CommandType == COMMAND_TYPE.CREATE)
            {
                Log.Debug($"Commands: {commandTypes}");
                if (commandObject.CommandType == COMMAND_TYPE.LIST)
                {
                    SpeechSynService.Say("I have listed your commands.");
                }
                else if (commandObject.CommandType == COMMAND_TYPE.UPDATE)
                {
                    SpeechSynService.Say("Which command you would like to update?");
                    S.ServiceState = S.SERVICE_STATE.SPEECH_UPDATE;
                }
                else if (commandObject.CommandType == COMMAND_TYPE.CREATE)
                {
                    SpeechSynService.Say("Which command you would like to create?");
                    S.ServiceState = S.SERVICE_STATE.SPEECH_CREATE;
                }
                commandReturnState = S.COMMAND_RETURN_STATE.RESET;
            }
            if (commandObject.CommandType == COMMAND_TYPE.EXIT)
            {
                SpeechSynService.Say("Exiting.");
                commandReturnState = S.COMMAND_RETURN_STATE.RESET;
                Environment.Exit(1);
            }
            if (commandObject.CommandType == COMMAND_TYPE.UNKNOWN)
            {
                SpeechSynService.Say("Unknown command. Please use one of the following commands.");
                Log.Debug($"Commands: {commandTypes}");
            }
            if (commandObject.CommandType == COMMAND_TYPE.GOTO && _processStartInfo != null)
            {
                if (commandObject.GOTO_Command_Text.ToString() != null)
                {
                    if (commandObject.GOTO_Command_Text.ToLower().Contains("read"))
                    {
                        _processStartInfo.Arguments = "https://www.reddit.com/";
                        Process.Start(_processStartInfo);
                    }
                    else if (commandObject.GOTO_Command_Text.ToLower().Contains("google"))
                    {
                        _processStartInfo.Arguments = "https://www.google.com/";
                        Process.Start(_processStartInfo);
                    }

                    if (_processStartInfo.Arguments == null)
                    {
                        SpeechSynService.Say("Unknown command. go to.");
                        Process.Start(_processStartInfo);
                    }
                }
                commandReturnState = S.COMMAND_RETURN_STATE.RESET;
            }
            return commandReturnState;
        }
    }
}