using JaszCore.Common;
using JaszCore.Models;
using JaszCore.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace JaszCore.Services
{
    [Service(typeof(LoggerService))]
    public interface ILoggerService
    {
        void Debug(string message, int size = 0, params object[] args);

        void Error(Exception ex, string message = null, int size = 0, params object[] args);

        void CloseService();

        void ShowLogHistory();

        void ShowSessionLogs();
    }

    public class LoggerService : ILoggerService
    {
        private static string _previousMethod = "";
        private static int _indent = 0;
        private readonly StringBuilder AppLogger;

        public LoggerService()
        {
            Debug($"LoggerService starting....");
            AppLogger ??= new StringBuilder();
            var filePath = Path.Combine(S.LOG_DIR, S.LOG_FILE);
            var headerLine = S.GetFileHeaderLine("Logs");
            var newRunLine = S.FILE_RUNLINE;
            if (!File.Exists(filePath))
            {
                var fileStream = File.Create(filePath);
                fileStream.Close();
                AppLogger?.Insert(0, headerLine + newRunLine);
            }
            else
            {
                AppLogger?.Insert(0, newRunLine);
            }
        }

        public void Debug(string message, int size = 0, params object[] args)
        {
            try
            {
                var sDate = $"{DateTime.Now:HH:mm:ss.fff}";
                var sClass = NameOfCallingClass();
                var sMethod = NameOfCallingMethod();
                var sThread = NameOfCallingThread();

                _indent = size + (S.IsIndenting() && _previousMethod == sMethod && !sMethod.Contains("Constructor") ? _indent + 1 : 0);
                _previousMethod = sMethod;
                var indent = _indent > 0 ? new string('.', _indent) : "";


                var debugInfo = $"{sThread}|{sDate}|{sClass}||{sMethod}|{indent}>";
                var sMessage = args.IsEmpty() ? message : string.Format(message, args);
                var stringLine = debugInfo + sMessage;
                Console.WriteLine(stringLine);
                AppLogger?.AppendLine(stringLine);
            }
            catch (Exception ex)
            {
                var stringLine = $"{Environment.CurrentManagedThreadId}|{DateTime.Now:HH:mm:ss.fff}> {message} !! ERROR !!{ex.Message}";
                Console.WriteLine(stringLine);
                AppLogger?.AppendLine(stringLine);
            }
        }

        public void Error(Exception ex, string message = null, int size = 0, params object[] args)
        {
            var spacer = S.ERROR_SPACELINE;
            var title = S.ERROR_TITLELINE;
            var etarget = S.NL + "TargetSite:      " + ex?.TargetSite?.Name;
            var emessage = S.NL + "Message:         " + ex?.Message;
            var estack = S.NL + "StackTrace:   " + ex?.StackTrace;
            var iemessage = ex?.InnerException?.Message != null ? S.NL + "InMessage:       " + ex?.InnerException?.Message : "";
            var iestack = ex?.InnerException?.StackTrace != null ? S.NL + "InStackTrace: " + ex?.InnerException?.StackTrace : "";
            var imessage = message != null ? S.NL + "AppMessage: " + message : "";
            var errorMessage = spacer + title + spacer + etarget + emessage + estack + iemessage + iestack + imessage + spacer + spacer;
            Debug(errorMessage);
        }

        public void CloseService()
        {
            Debug($"LoggerService exiting....");
            var filePath = Path.Combine(S.LOG_DIR, S.LOG_FILE);
            if (File.Exists(filePath))
            {
                var bytes = Encoding.ASCII.GetBytes(AppLogger?.ToString());
                var fileStream = File.Open(filePath, FileMode.Append);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Close();
            }
            AppLogger?.Clear();
        }

        public void ShowLogHistory()
        {
            var title = $"------------------------------>  HISTORY LOGS  <------------------------------";
            var end = $"---------------------------------->  END  <-----------------------------------";
            var filePath = Path.Combine(S.LOG_DIR, S.LOG_FILE);
            var logHistory = "";
            if (File.Exists(filePath))
            {
                var fileStream = File.Open(filePath, FileMode.Open);
                var bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                fileStream.Close();
                logHistory = Encoding.ASCII.GetString(bytes);
            }
            logHistory = logHistory?.Length > 2 ? logHistory[0..^2] : logHistory;
            Console.WriteLine(title);
            Console.WriteLine(logHistory);
            Console.WriteLine(end);
        }

        public void ShowSessionLogs()
        {
            var title = $"------------------------------>  SESSION LOGS  <------------------------------";
            var end = $"---------------------------------->  END  <-----------------------------------";
            var sessionLogs = AppLogger?.ToString();
            sessionLogs = sessionLogs?.Length > 2 ? sessionLogs[0..^2] : sessionLogs;
            Console.WriteLine(title);
            Console.WriteLine(sessionLogs);
            Console.WriteLine(end);
        }

        private string NameOfCallingClass()
        {
            var method = new StackTrace().GetFrame(2).GetMethod();
            var className = method.DeclaringType != null ? method.DeclaringType.FullName : method.Name;
            className = className.Contains(".") ? className.Substring(className.LastIndexOf(".") + 1) : className;
            className = className.Contains(".ctor") ? "Class" : className;
            var adjustC = 15 - className.Length;
            className = adjustC > 0 ? className + new string('_', adjustC) : className.Substring(0, 15);
            return className;
        }

        private string NameOfCallingMethod()
        {
            var sMethod = new StackTrace().GetFrame(2).GetMethod().Name;
            sMethod = sMethod.Contains(".ctor") ? "Constructor" : sMethod;
            var adjustM = 15 - sMethod.Length;
            sMethod = adjustM > 0 ? sMethod + new string('_', adjustM) : sMethod.Substring(0, 15);
            return sMethod;
        }

        private string NameOfCallingThread()
        {
            var sThread = Environment.CurrentManagedThreadId.ToString();
            var adjustT = 3 - sThread.Length;
            sThread = adjustT > 0 ? new string('0', adjustT) + sThread : sThread.Substring(0, 3);
            return sThread;
        }

    }
}