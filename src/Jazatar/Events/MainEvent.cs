using JaszCore.Common;
using JaszCore.Events;
using JaszCore.Services;
using System;

namespace Jazatar.Events
{
    public class MainEvent : IEvent
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();
        private static ISpeechRecognitionService SpeechRecService => ServiceLocator.Get<ISpeechRecognitionService>();
        private static ISpeechSynthesizerService SpeechSynService => ServiceLocator.Get<ISpeechSynthesizerService>();
        private static ITextRecognitionService TextRecService => ServiceLocator.Get<ITextRecognitionService>();
        private static ICommandService CommandService => ServiceLocator.Get<ICommandService>();

        private static string[] _STOPVALUES = { "stop", "end", "quit", "exit" };


        public void StartEvent(string[] args = null)
        {
            Log.Debug($"StartEvent executing at: {DateTime.Now}");
            Initialize();
            Log.Debug($"StartEvent completed at: {DateTime.Now}");
        }

        private void Initialize()
        {
            Log.Debug("Initialize starting....");
            SpeechSynService.Initialize();
            CommandService.Initialize();
            SpeechRecService.Initialize();
            TextRecService.Initialize();
            Log.Debug("Initialize finished....");
        }
    }
}
