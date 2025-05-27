using JaszCore.Common;
using JaszCore.Events;
using JaszCore.Services;
using JaszCore.Utils;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using System.Text;
using System.Threading;

namespace Jazatar.Events
{
    public class SpeechRecognitionEvent : IEvent
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        private const ALFormat _alFormat = ALFormat.Mono16;
        private const int _recordLength = 3;        // Length of recording
        private const int _samplingRate = 44100;     // Samples per second
        private const int _bufferLength = _samplingRate * _recordLength; // Record buffer total size
        private const ushort _bitsPerSample = 16;    // Mono16 16 bits per sample
        private const ushort _numChannels = 1;       // Mono16 1 channel

        private static string _outputFilePath = @$"{S.RES_DIR}\output.wav";

        private static string[] _stopValues = { "stop", "end", "quit", "exit" };
        private static bool _running = true;


        public void StartEvent(string[] args = null)
        {
            Log.Debug($"SpeechRecognitionEvent starting....");
            RunEvent();
            Log.Debug($"SpeechRecognitionEvent finished....");
        }

        private void RunEvent()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            CaptureRecording();
            ParseRecording();
            ParseRealtimeSpeech();
            while (_running)
            {
                Thread.Sleep(1000);
                //  Console.ReadLine();  Read input
            }
            Log.Debug($"completed at: {DateTime.Now}");
        }

        private void ParseRecording()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            recognizer.LoadGrammar(new DictationGrammar());
            recognizer.SetInputToWaveFile(_outputFilePath);
            recognizer.BabbleTimeout = new TimeSpan(Int32.MaxValue);
            recognizer.InitialSilenceTimeout = new TimeSpan(Int32.MaxValue);
            recognizer.EndSilenceTimeout = new TimeSpan(100000000);
            recognizer.EndSilenceTimeoutAmbiguous = new TimeSpan(100000000);

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

        private void ParseRealtimeSpeech()
        {
            Log.Debug($"executing at: {DateTime.Now}");
            using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US")))
            {
                recognizer.LoadGrammar(new DictationGrammar());

                recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(RecognizerSpeechRecognized);

                // Configure input to the speech recognizer.  
                recognizer.SetInputToDefaultAudioDevice();

                // Start asynchronous, continuous speech recognition.  
                recognizer.RecognizeAsync(RecognizeMode.Multiple);

                while (_running)
                {
                    Thread.Sleep(1000);
                    //  Console.ReadLine();  Read input
                }
            }
            Log.Debug($"completed at: {DateTime.Now}");
        }

        private void RecognizerSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            var parsedData = e.Result.Text;
            Log.Debug($"Recognized text: {parsedData}");
            if (parsedData.ToLower().Contains("read"))
            {
                OpenBrowser($"https://old.reddit.com/");
            }
            if (parsedData.ToLower().Contains("google"))
            {
                OpenBrowser($"https://www.google.com/");
            }
            if (StringUtils.ContainsAnyIgnoreCase(parsedData, _stopValues))
            {
                _running = false;
            }
        }

        private void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw new ApplicationException($"Type Error Default OS Browser is missing.... A default browser must be setup!!");
            }
        }

        private void CaptureRecording()
        {
            Log.Debug($"CaptureRecording starting");

            var recorders = AudioCapture.AvailableDevices;
            for (int i = 0; i < recorders.Count; i++)
            {
                Log.Debug($"Possible recorder: {recorders[i]}");
            }
            if (recorders.Count >= 1)
            {
                Log.Debug($"Recording from: {recorders[0]}");
                var fileStream = File.OpenWrite(_outputFilePath);
                using (var binaryWriter = new BinaryWriter(fileStream))
                {
                    WriteSoundFileHeader(binaryWriter);

                    // Record 4 seconds of data
                    int samplesWrote = 0;
                    using (var audioCapture = new AudioCapture(recorders[0], _samplingRate, _alFormat, _bufferLength))
                    {
                        var buffer = new short[_bufferLength];
                        audioCapture.Start();
                        for (int i = 0; i < _recordLength; ++i)
                        {
                            Thread.Sleep(1000);
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
                    binaryWriter.Write(36 + samplesWrote * (_bitsPerSample / 8) * _numChannels);
                    binaryWriter.Seek(40, SeekOrigin.Begin);                                        // Seek to data size position
                    binaryWriter.Write(samplesWrote * (_bitsPerSample / 8) * _numChannels);
                }
            }
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
            binaryWriter.Write(_numChannels);                                               // wChannels
            binaryWriter.Write(_samplingRate);                                              // dwSamplesPerSec
            binaryWriter.Write(_samplingRate * _numChannels * (_bitsPerSample / 8));        // dwAvgBytesPerSec
            binaryWriter.Write((ushort)(_numChannels * (_bitsPerSample / 8)));              // wBlockAlign
            binaryWriter.Write(_bitsPerSample);                                             // wBitsPerSample
            binaryWriter.Write(new char[] { 'd', 'a', 't', 'a' });                          // "data" chunk
            binaryWriter.Write(0);                                                          // fill in later
        }

        private void OtherAlexaCrapAsync()
        {
            //var client = new AccessTokenClient(AccessTokenClient.ApiDomainBaseAddress);
            //var accessToken = await client.Send(alexa_client, alexa_secret).GetAwaiter().GetResult();
            //var alexa_token = accessToken.Token;

            //var payload = new Dictionary<string, string> { { "testKey", "testValue" } };

            //var messages = new Alexa.NET.SkillMessageClient(Alexa.NET.SkillMessageClient.NorthAmericaEndpoint, alexa_token);
            //var messageToSend = new Alexa.NET.SkillMessaging.Message(payload, 300);
            //var userId = "A3V4RSJOPR7VSE";
            //var messageId = await messages.Send(messageToSend, userId);
        }
    }
}
