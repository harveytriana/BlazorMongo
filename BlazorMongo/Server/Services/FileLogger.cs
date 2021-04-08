// ===============================
// VISIONARY S.A.S.
// ===============================
using System;
using System.IO;

// First approach learning Xamarin Forms
//
namespace BlazorMongo.Server.Services
{
    public class FileLogger
    {
        static readonly string _logFile = @"C:\_tracelog\blazormongo.log";

        public void Reset()
        {
            if ( File.Exists(_logFile)) {
                File.Delete(_logFile);
            }
        }

        public void Log(string text)
        {
            try {
                using var fs = File.Open(_logFile, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var tw = new StreamWriter(fs); 
                tw.WriteLine($"{DateTime.Now:dd-MM-yy HH:mm:ss} {text}");
            }
            catch { }
        }

        public string LogFile()
        {
            return _logFile;
        }
    }
}
