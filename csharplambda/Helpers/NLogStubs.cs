using System;
using Amazon.Lambda.Core;

namespace csharplambda.Helpers
{
    public class Logger
    {
        public string BaseLogger;
        public static ILambdaContext context;

        public Logger()
        {
        }

        public Logger(string baseLogger)
        {
            this.BaseLogger = baseLogger;
        }

        private void Log(string str, string level)
        {
            var d = DateTime.Now.ToString("h:mm:ss tt zz");
            var strf = $"[{d}]{BaseLogger}-{level} {str}";
            if (context != null)
                context.Logger.LogLine(strf);
            else
                Console.WriteLine(strf);
        }

        public void Trace(string str, Exception ex = null)
        {
            Log(str, "Trace");
        }

        public void Warn(string str, Exception ex = null)
        {
            Log(str, "Warn");
        }

        public void Debug(string str, Exception ex = null)
        {
            Log(str, "Debug");
        }

        public void Info(string str, Exception ex = null)
        {
            Log(str, "Info");
        }

        public void Error(string str, Exception ex = null)
        {
            Log(str, "Error");
        }

        public void Fatal(string str, Exception ex = null)
        {
            Log(str, "Fatal");
        }
    }

    public class LogManager
    {
        public static Logger GetLogger(string baselogger)
        {
            return new Logger(baselogger);
        }

    }
}
