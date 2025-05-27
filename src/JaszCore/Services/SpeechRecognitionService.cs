using JaszCore.Common;
using JaszCore.Models;
using JaszCore.Objects;
using JaszCore.Utils;
using OpenTK.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static JaszCore.Common.S;
using static JaszCore.Objects.CommandObject;

namespace JaszCore.Services
{
    [Service(typeof(SpeechRecognitionService))]
    public interface ISpeechRecognitionService
    {
        void CloseService();
        void CaptureRecording();
        void Initialize();
        void SetSpeechServiceState(S.SERVICE_STATE speechServiceState);
        void TurnOffSpeechCommands();
        void ResetActivator();
        void CancelOperations();
    }
    public class SpeechRecognitionService : ISpeechRecognitionService
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();
        private static ICommandService CommandService => ServiceLocator.Get<ICommandService>();

        private static string _RECORDER = null;
        private static readonly string START_VAL = "<--:";
        private static readonly string END_VAL = ":-->";
        private static readonly string SPLIT_VAL = "<~~>";
        private static readonly int JUMP_VAL = START_VAL.Length;
        private static readonly int _activationWaitTime = 15000;
        private static DateTime _activatedStart = DateTime.UtcNow;
        private static DateTime _activatedStop = DateTime.UtcNow.AddMilliseconds(_activationWaitTime);
        public enum SPEECH_SERVICE_STATE { NONE = 0, CREATE = 1, UPDATE = 2, RUN = 3, UPDATE_P2 = 4, }
        private SPEECH_SERVICE_STATE SpeechServiceState { get; set; } = SPEECH_SERVICE_STATE.NONE;

        private readonly SpeechRecognitionEngine _speechRecognizer;
        private readonly IList<CommandObject> _speechCommands;
        private static readonly CancellationTokenSource _cancelTokenSrc = new CancellationTokenSource();

        public SpeechRecognitionService()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            _speechRecognizer ??= new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            _speechCommands ??= new List<CommandObject>();
            var filePath = Path.Combine(S.RES_DIR, S.STATE_FILE);
            if (!File.Exists(filePath))
            {
                LoadDefaultSpeechCommands();
            }
            else
            {
                var fileStream = File.Open(filePath, FileMode.Open);
                var bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                fileStream.Close();
                var _speechRecognitionData = new StringBuilder().Insert(0, Encoding.ASCII.GetString(bytes));
                LoadStorageSpeechCommands(_speechRecognitionData);
            }
            Log.Debug($"completed at: {DateTime.Now}");
        }

        public void Initialize()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            _speechRecognizer.LoadGrammar(new DictationGrammar());
            _speechRecognizer.SetInputToDefaultAudioDevice();
            _speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
            _speechRecognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(RecognizerSpeechRecognized);
            Task.Run(() =>
            {
                Log.Debug("Started ListenForInput....");
                ResetActivator();
                while (!_cancelTokenSrc.Token.IsCancellationRequested)
                {
                    Thread.Sleep(500);
                }
                _speechRecognizer.RecognizeAsyncStop();
                Log.Debug("Recognition canceled.");
            }, _cancelTokenSrc.Token);
            Log.Debug($"completed at: {DateTime.Now}");
        }

        public void CloseService()
        {
            Log.Debug($"SpeechRecognitionService exiting....");
            CancelOperations();
            SaveSpeechCommandsToStorage();
        }

        public void ResetActivator()
        {
            Log.Debug($".....Jasz Awoke.....");
            _activatedStart = DateTime.UtcNow;
            _activatedStop = DateTime.UtcNow.AddMilliseconds(_activationWaitTime);
        }

        public void SetSpeechServiceState(S.SERVICE_STATE speechServiceState)
        {
            Log.Debug($"SetSpeechServiceState.....");
            S.ServiceState = speechServiceState;
        }

        public void TurnOffSpeechCommands()
        {
            Log.Debug($"TurnOffSpeechCommands.....");
            S.CurrentWakeCommand.SetCommandState(CommandObject.COMMAND_STATE.NONE);
            foreach (var command in _speechCommands)
            {
                command.SetCommandState(CommandObject.COMMAND_STATE.NONE);
            }
            SpeechServiceState = SPEECH_SERVICE_STATE.NONE;
        }

        private void RecognizerSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            var parsedData = e.Result.Text.ToLower();
            Log.Debug($"Parsed speech data: {parsedData}");
            _activatedStart = DateTime.UtcNow;
            Log.Debug($"Start->{_activatedStart} || Stop->{_activatedStop}");
            if (parsedData.Length < 2)
                return;
            else if (SpeechServiceState == SPEECH_SERVICE_STATE.UPDATE)
                CheckCommandToUpdate(parsedData);
            else if (SpeechServiceState == SPEECH_SERVICE_STATE.UPDATE_P2)
                UpdateCommandTrigger(parsedData);
            else if (_activatedStart > _activatedStop)
                CheckWakeCommand(parsedData);
            else if (_activatedStart <= _activatedStop)
                CheckCommandToRun(parsedData);
        }

        private void CheckCommandToUpdate(string parsedData)
        {
            Log.Debug($"CheckCommandToUpdate....");
            var _currentWakeInitializers = S.CurrentWakeCommand.CommandInitializers != null ? string.Join(",", S.CurrentWakeCommand.CommandInitializers) : null;
            if (_currentWakeInitializers.Contains(parsedData))
                S.CurrentWakeCommand.SetCommandState(CommandObject.COMMAND_STATE.UPDATE);
            else
            {
                foreach (var command in _speechCommands)
                {
                    var initializers = string.Join(",", command.CommandInitializers);
                    if (initializers.Contains(parsedData))
                    {
                        command.SetCommandState(CommandObject.COMMAND_STATE.UPDATE);
                        break;
                    }
                }
            }
        }

        private void UpdateCommandTrigger(string parsedData)
        {
            Log.Debug($"UpdateCommand Event....");
            if (S.CurrentWakeCommand.CommandState == CommandObject.COMMAND_STATE.UPDATE)
            {
                S.CurrentWakeCommand.UpdateCommandInitializers(parsedData);
            }
            else
            {
                foreach (var command in _speechCommands)
                {
                    if (command.CommandState == CommandObject.COMMAND_STATE.UPDATE)
                    {
                        command.UpdateCommandInitializers(parsedData);
                        break;
                    }
                }
            }
        }

        private void CheckCommandToRun(string parsedData)
        {
            Log.Debug($"CheckCommandToRun....");
            foreach (var command in _speechCommands)
            {
                var initializers = string.Join(",", command.CommandInitializers);
                if (initializers.Contains(parsedData))
                {
                    if (command.CommandType == COMMAND_TYPE.GOTO)
                    {
                        var whereToGo = parsedData.Replace("goto", "").Replace("go to", "").Replace("go too", "").Trim();
                        command.GOTO_Command_Text = whereToGo;
                    }
                    var commandReturnState = CommandService.RunCommand(command);
                    if (commandReturnState == S.COMMAND_RETURN_STATE.RESET)
                    {
                        ResetActivator();
                    }
                    break;
                }
            }
        }

        private void CheckWakeCommand(string parsedData)
        {
            var _currentWakeInitializers = CurrentWakeCommand.CommandInitializers != null ? string.Join(",", CurrentWakeCommand.CommandInitializers) : null;
            Log.Debug($"CheckWakeCommand ->{_currentWakeInitializers}");
            if (_currentWakeInitializers.Contains(parsedData))
            {
                var commandReturnState = CommandService.RunCommand(CurrentWakeCommand);
                if (commandReturnState == S.COMMAND_RETURN_STATE.RESET)
                {
                    ResetActivator();
                }
            }
        }

        private void LoadStorageSpeechCommands(StringBuilder speechRecognitionData)
        {
            Log.Debug($"LoadStorageSpeechCommands starting....");
            var currentStartIndex = 0;
            var finalIndex = speechRecognitionData.Length;
            while (currentStartIndex < finalIndex)
            {
                currentStartIndex = StringUtils.GetStringBuilderIndexOf(speechRecognitionData, START_VAL, currentStartIndex, true) + JUMP_VAL;
                var currentEndIndex = StringUtils.GetStringBuilderIndexOf(speechRecognitionData, END_VAL, currentStartIndex, true) - currentStartIndex;
                var commandP1 = speechRecognitionData.ToString(currentStartIndex, currentEndIndex);
                Enum.TryParse(commandP1, out CommandObject.COMMAND_TYPE commandType);
                currentStartIndex = currentStartIndex + currentEndIndex + JUMP_VAL;

                currentStartIndex = StringUtils.GetStringBuilderIndexOf(speechRecognitionData, START_VAL, currentStartIndex, true) + JUMP_VAL;
                currentEndIndex = StringUtils.GetStringBuilderIndexOf(speechRecognitionData, END_VAL, currentStartIndex, true) - currentStartIndex;
                var commandP2 = speechRecognitionData.ToString(currentStartIndex, currentEndIndex);
                var commandInitializers = commandP2 != null && commandP2 != "null" ? commandP2.Split(SPLIT_VAL) : null;
                currentStartIndex = currentStartIndex + currentEndIndex + JUMP_VAL;
                var commandInitText = commandInitializers != null ? string.Join(",", commandInitializers) : "null";

                currentStartIndex = StringUtils.GetStringBuilderIndexOf(speechRecognitionData, START_VAL, currentStartIndex, true) + JUMP_VAL;
                currentEndIndex = StringUtils.GetStringBuilderIndexOf(speechRecognitionData, END_VAL, currentStartIndex, true) - currentStartIndex;
                var commandP3 = speechRecognitionData.ToString(currentStartIndex, currentEndIndex);
                var commandParams = commandP3 != null && commandP3 != "null" ? commandP3.Split(SPLIT_VAL) : null;
                currentStartIndex = currentStartIndex + currentEndIndex + JUMP_VAL;

                currentStartIndex = StringUtils.GetStringBuilderIndexOf(speechRecognitionData, START_VAL, currentStartIndex, true);
                currentStartIndex = currentStartIndex == -1 ? finalIndex : currentStartIndex;

                var commandObject = new CommandObject(commandType, commandInitializers, commandParams);
                Log.Debug($"Loaded--> Type:{commandObject.CommandType} Position:{currentStartIndex}/{finalIndex} \nInitializers:{commandInitText}");

                if (commandObject.CommandType == CommandObject.COMMAND_TYPE.WAKE)
                {
                    S.CurrentWakeCommand = commandObject;
                }
                if (commandObject.IsCommonCommand())
                {
                    _speechCommands.Add(commandObject);
                }
            }
            Log.Debug($"LoadStorageSpeechCommands ending....");
        }

        private void LoadDefaultSpeechCommands()
        {
            Log.Debug($"LoadDefaultSpeechCommands starting....");
            _speechCommands.Add(S.DefaultExitCommand);
            _speechCommands.Add(S.DefaultListCommand);
            _speechCommands.Add(S.DefaultCreateCommand);
            _speechCommands.Add(S.DefaultUpdateCommand);
            _speechCommands.Add(S.DefaultBrowserCommand);
        }

        private void SaveSpeechCommandsToStorage()
        {
            Log.Debug($"SaveSpeechCommandsToStorage starting....");
            var filePath = Path.Combine(S.RES_DIR, S.STATE_FILE);
            if (File.Exists(filePath))
            {
                Log.Debug($"Removing old file...");
                File.Delete(filePath);
            }
            if (!File.Exists(filePath))
            {
                Log.Debug($"File exists : {filePath}  saving....");
                var savedSpeechCommandBuilder = new StringBuilder();
                savedSpeechCommandBuilder.Append(S.GetFileHeaderLine("Storage"));

                var _currentWakeType = S.CurrentWakeCommand.CommandType.ToString();
                var _currentWakeInitializers = S.CurrentWakeCommand.CommandInitializers == null || S.CurrentWakeCommand.CommandInitializers.Length < 1 ? "null" : string.Join(SPLIT_VAL, S.CurrentWakeCommand.CommandInitializers);
                var _currentWakeParameters = S.CurrentWakeCommand.CommandParams == null || S.CurrentWakeCommand.CommandParams.Length < 1 ? "null" : string.Join(SPLIT_VAL, S.CurrentWakeCommand.CommandParams);
                savedSpeechCommandBuilder.Append(START_VAL + _currentWakeType + END_VAL);
                savedSpeechCommandBuilder.Append(START_VAL + _currentWakeInitializers + END_VAL);
                savedSpeechCommandBuilder.Append(START_VAL + _currentWakeParameters + END_VAL);

                foreach (var command in _speechCommands)
                {
                    var commandType = command.CommandType.ToString();
                    var commandInitializers = command.CommandInitializers == null || command.CommandInitializers.Length < 1 ? "null" : string.Join(SPLIT_VAL, command.CommandInitializers);
                    var commandParameters = command.CommandParams == null || command.CommandParams.Length < 1 ? "null" : string.Join(SPLIT_VAL, command.CommandParams);
                    savedSpeechCommandBuilder.Append(START_VAL + commandType + END_VAL);
                    savedSpeechCommandBuilder.Append(START_VAL + commandInitializers + END_VAL);
                    savedSpeechCommandBuilder.Append(START_VAL + commandParameters + END_VAL);
                }

                var bytes = Encoding.ASCII.GetBytes(savedSpeechCommandBuilder?.ToString());
                var fileStream = File.Open(filePath, FileMode.Append);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Close();
                savedSpeechCommandBuilder?.Clear();
            }
            Log.Debug($"SaveSpeechCommandsToStorage ending....");
        }

        public void CaptureRecording()
        {
            Log.Debug($"CaptureRecording starting");
            if (_RECORDER == null)
            {
                SetRecorder();
            }
            var randomFilepath = S.OUTPUT_SOUND_FOLDER + new Random().Next().ToString() + ".wav";
            var fileStream = File.OpenWrite(randomFilepath);
            using (var binaryWriter = new BinaryWriter(fileStream))
            {
                WriteSoundFileHeader(binaryWriter);
                // Record sound to data
                int samplesWrote = 0;
                using (var audioCapture = new AudioCapture(_RECORDER, S._SAMPLERATE, S._ALFORMAT, S._BUFFERLENGTH))
                {
                    var buffer = new short[S._BUFFERLENGTH];
                    audioCapture.Start();
                    for (int i = 0; i < S._RECORDLENGTH; ++i)
                    {
                        Thread.Sleep(S._RECORDLENGTH * 1000);
                        var samplesAvailable = audioCapture.AvailableSamples;
                        audioCapture.ReadSamples(buffer, samplesAvailable);
                        for (var x = 0; x < samplesAvailable; ++x)
                        {
                            binaryWriter.Write(buffer[x]);
                        }
                        samplesWrote += samplesAvailable;
                        Log.Debug($"Wrote {samplesAvailable}/{samplesWrote} samples...");
                    }
                    audioCapture.Stop();
                }
                binaryWriter.Seek(4, SeekOrigin.Begin);                                         // Seek to overall size
                binaryWriter.Write(36 + samplesWrote * (S._BITSPERSAMPLE / 8) * S._NUMCHANNLES);
                binaryWriter.Seek(40, SeekOrigin.Begin);                                        // Seek to data size position
                binaryWriter.Write(samplesWrote * (S._BITSPERSAMPLE / 8) * S._NUMCHANNLES);
                binaryWriter.Close();
            }
            fileStream.Close();
            Log.Debug($"Saved sound file: {randomFilepath}");
            Log.Debug($"CaptureRecording ending");
        }

        private void WriteSoundFileHeader(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(new char[] { 'R', 'I', 'F', 'F' });                          // WAVE header char
            binaryWriter.Write(0);                                                          // fill in later
            binaryWriter.Write(new char[] { 'W', 'A', 'V', 'E' });                          // WAVE header char continued
            binaryWriter.Write(new char[] { 'f', 'm', 't', ' ' });                          // "fmt " chunk (Google: WAVEFORMATEX structure)
            binaryWriter.Write(16);                                                         // chunkSize (in bytes)
            binaryWriter.Write((ushort)1);                                                  // wFormatTag (PCM = 1)
            binaryWriter.Write(S._NUMCHANNLES);                                             // wChannels
            binaryWriter.Write(S._SAMPLERATE);                                              // dwSamplesPerSec
            binaryWriter.Write(S._SAMPLERATE * S._NUMCHANNLES * (S._BITSPERSAMPLE / 8));    // dwAvgBytesPerSec
            binaryWriter.Write((ushort)(S._NUMCHANNLES * (S._BITSPERSAMPLE / 8)));          // wBlockAlign
            binaryWriter.Write(S._BITSPERSAMPLE);                                           // wBitsPerSample
            binaryWriter.Write(new char[] { 'd', 'a', 't', 'a' });                          // "data" chunk
            binaryWriter.Write(0);                                                          // fill in later
        }

        private void SetRecorder()
        {
            Log.Debug($"Setting Recorder");
            var recorders = AudioCapture.AvailableDevices;
            for (int i = 0; i < recorders.Count; i++)
            {
                Log.Debug($"Possible recorder: {recorders[i]}");
            }
            _RECORDER = recorders[0];
            Log.Debug($"Recording from: {_RECORDER}");
        }

        private void ParseRecording(string recordFilePath)
        {
            Log.Debug($"executing at: {DateTime.Now}");
            SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            recognizer.LoadGrammar(new DictationGrammar());
            recognizer.SetInputToWaveFile(recordFilePath);
            recognizer.BabbleTimeout = new TimeSpan(Int32.MaxValue);
            recognizer.InitialSilenceTimeout = new TimeSpan(Int32.MaxValue);
            recognizer.EndSilenceTimeout = new TimeSpan(Int32.MaxValue);
            recognizer.EndSilenceTimeoutAmbiguous = new TimeSpan(Int32.MaxValue);

            StringBuilder stringBuilder = new StringBuilder();
            while (true)
            {
                try
                {
                    var recText = recognizer.Recognize();
                    if (recText == null)
                    {
                        break;
                    }
                    stringBuilder.Append(recText.Text);
                }
                catch (Exception)
                {
                    Log.Debug($"End of file read");
                    break;
                }
            }
            Log.Debug($"Speech read from file: {stringBuilder.ToString()}");
            Log.Debug($"completed at: {DateTime.Now}");
        }

        public void CancelOperations()
        {
            Log.Debug("Canceling operations...");
            _cancelTokenSrc.Cancel();
        }
    }
}