using JaszCore.Common;
using JaszCore.Services;
using System;
using System.Collections.Generic;

namespace JaszCore.Objects
{
    public class CommandObject
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();
        public enum COMMAND_TYPE { UNKNOWN = -1, WAKE = 0, EXIT = 1, CREATE = 2, UPDATE = 3, LIST = 4, BROWSER = 5, GOTO = 6 }
        public enum COMMAND_STATE { NONE = 0, CREATE = 1, UPDATE = 2, RUN = 3 }
        public COMMAND_TYPE CommandType { get; } = COMMAND_TYPE.UNKNOWN;
        public COMMAND_STATE CommandState { get; set; } = COMMAND_STATE.NONE;
        public object[] CommandInitializers;
        public object[] CommandParams;
        public string GOTO_Command_Text = "";
        public bool IsCommonCommand() { return CommandType != COMMAND_TYPE.WAKE && CommandType != COMMAND_TYPE.UNKNOWN ? true : false; }

        public string GetCommandTypeStringList()
        {
            var _speechCommands = new List<string>();
            foreach (string item in Enum.GetNames(typeof(COMMAND_TYPE)))
            {
                _speechCommands.Add(item);
            }
            var list = String.Join(", ", _speechCommands.ToArray());
            return list;
        }

        public CommandObject(COMMAND_TYPE commandType, object[] commandInitializers, object[] commandParams)
        {
            CommandType = commandType;
            CommandInitializers = commandInitializers;
            CommandParams = commandParams;
        }

        public void SetCommandState(COMMAND_STATE commandState)
        {
            Log.Debug($"SetCommandState starting....{commandState.ToString()}");
            CommandState = commandState;
            if (commandState == COMMAND_STATE.UPDATE)
            {
                Log.Debug($".....Updating {CommandType}.....");
                Log.Debug($"Please say words you would like to trigger the command with.  When you are finished press any key to end the update.");
                S.ServiceState = S.SERVICE_STATE.SPEECH_UPDATE_P2;
            }
            if (commandState == COMMAND_STATE.CREATE)
            {
                Log.Debug($"Ready to create {CommandType}:");
            }
        }

        public void UpdateCommandInitializers(string text)
        {
            Log.Debug($"Updating command with trigger: {text}");
            var _currentInitializers = CommandInitializers != null ? string.Join(",", CommandInitializers) : null;
            if (!_currentInitializers.Contains(text))
            {
                Array.Resize(ref CommandInitializers, CommandInitializers.Length + 1);
                CommandInitializers[CommandInitializers.Length - 1] = text;
            }
        }
    }
}
