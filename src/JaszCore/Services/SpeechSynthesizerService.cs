using JaszCore.Common;
using JaszCore.Models;
using System;
using System.Speech.Synthesis;

namespace JaszCore.Services
{
    [Service(typeof(SpeechSynthesizerService))]
    public interface ISpeechSynthesizerService
    {
        void CloseService();
        void Initialize();
        void Say(string textToSpeech);
    }
    public class SpeechSynthesizerService : ISpeechSynthesizerService
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();
        private readonly SpeechSynthesizer _speechSynthesizer;

        public SpeechSynthesizerService()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            _speechSynthesizer = new SpeechSynthesizer();
            Log.Debug($"completed at: {DateTime.Now}");
        }

        public void Initialize()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            _speechSynthesizer.Speak("Hello, I am your Jazatar.");
            Log.Debug($"completed at: {DateTime.Now}");
        }

        public void CloseService()
        {
            Log.Debug($"TextRecognitionService exiting....");
        }

        public void Say(string textToSpeech)
        {
            Log.Debug(textToSpeech);
            _speechSynthesizer.Speak(textToSpeech);
        }
    }
}