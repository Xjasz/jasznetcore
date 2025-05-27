using JaszCore.Common;
using JaszCore.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JaszCore.Services
{
    [Service(typeof(TextRecognitionService))]
    public interface ITextRecognitionService
    {
        void CloseService();
        void Initialize();
        void SetTextServiceState(S.SERVICE_STATE serviceState);
        void HandleUserInput(string text);
        void ListenForInput();
        void CancelOperations();
    }
    public class TextRecognitionService : ITextRecognitionService
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();
        private static ISpeechRecognitionService SpeechRecService => ServiceLocator.Get<ISpeechRecognitionService>();

        private static readonly CancellationTokenSource _cancelTokenSrc = new CancellationTokenSource();

        public TextRecognitionService()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            Log.Debug($"completed at: {DateTime.Now}");
        }

        public void Initialize()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            Console.CancelKeyPress += ConsoleCancelKeyPress;
            var cancelToken = _cancelTokenSrc.Token;
            try
            {
                Task.Run(() => ListenForInput(), cancelToken);
                cancelToken.WaitHandle.WaitOne();
                cancelToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                Log.Debug("Operation Canceled - Triggered OperationCanceledException....");
            }
            Log.Debug($"completed at: {DateTime.Now}");
        }

        public void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Log.Debug($"ConsoleCancelKeyPress....");
            e.Cancel = true;
            CancelOperations();
        }

        public void CloseService()
        {
            Log.Debug($"TextRecognitionService exiting....");
        }

        public void SetTextServiceState(S.SERVICE_STATE serviceState)
        {
            Log.Debug($"SetTextServiceState.....");
            S.ServiceState = serviceState;
        }

        public void ListenForInput()
        {
            Log.Debug("Started ListenForInput....");
            while (!_cancelTokenSrc.Token.IsCancellationRequested)
            {
                Thread.Sleep(500);
                var userInput = Console.ReadLine()?.Trim();
                if (userInput != null && userInput.Length > 0)
                {
                    if (userInput == "quit speech")
                    {
                        SpeechRecService.CancelOperations();
                    }
                    else if (userInput == "quit text")
                    {
                        CancelOperations();
                    }
                    else if (userInput == "x")
                    {
                        SpeechRecService.CaptureRecording();
                    }
                }
            }
            Log.Debug("Finished ListenForInput....");
        }

        public void HandleUserInput(string text)
        {
            Log.Debug($".....HandleUserInput.....");
            if (text?.Trim().ToLower() == "end")
            {
                SpeechRecService.TurnOffSpeechCommands();
            }
        }

        public void CancelOperations()
        {
            Log.Debug("Canceling operations...");
            _cancelTokenSrc.Cancel();
        }
    }
}