using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraSharp.Util;

namespace TerraSharp.Core
{
    public enum LogLevel
    {
        INFO,
        DEBUG,
        WARNING,
        ERROR,
        FATAL_ERROR
    }

    public class Logger
    {
        private static DateTime time = DateTime.Now;

        public static void WriteConsole(string message, LogLevel level)
        {
            time = DateTime.Now;
            string formattedTime = time.ToString("HH':'ss':'mm");

            StackTrace stackTrace = new();
            
            string methodName = stackTrace.GetFrame(1).GetMethod().Name;
            Type declaringType = stackTrace.GetFrame(1).GetMethod().DeclaringType;
            string className = declaringType != null ? declaringType.Name : "Unknown";

            switch (level) 
            {
                case LogLevel.INFO:
                    Console.WriteLine(ANSIFormatter.Format("&2[{0}&2] [Thread/INFO] [{1}&2]: {2}&r", formattedTime, className, message));
                    break;

                case LogLevel.DEBUG:
                    Console.WriteLine(ANSIFormatter.Format("&1[{0}&1] [Thread/DEBUG] [{1}&1]: {2}&r", formattedTime, className, message));
                    break;

                case LogLevel.WARNING:
                    Console.WriteLine(ANSIFormatter.Format("&6[{0}&6] [Thread/WARNING] [{1}&6]: {2}&r", formattedTime, className, message));
                    break;

                case LogLevel.ERROR:
                    Console.WriteLine(ANSIFormatter.Format("&4[{0}&4] [Thread/ERROR] [{1}&4]: {2}&r", formattedTime, className, message));
                    break;

                case LogLevel.FATAL_ERROR:
                    Console.WriteLine(ANSIFormatter.Format("&4[{0}&4] [Thread/FATAL ERROR] [{1}&4]: {2}&r", formattedTime, className, message));
                    Environment.Exit(-1);
                    break;
            }
        }
    }
}
