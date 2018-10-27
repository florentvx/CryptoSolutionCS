using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public interface ILogger
    {
        LoggingEventHandler LoggingEventHandler { get; }
    }


    public static class Logger
    {
        public static void PublishEvent(this ILogger sender, LevelType lvl, string message)
        {
            string type = sender.GetType().ToString().Split('.').LastOrDefault();
            LogMessageEventArgs lmea = new LogMessageEventArgs { Level = lvl, Message = $"{type} - {message}" };
            LoggingEventHandler handler = sender.LoggingEventHandler;
            handler?.Invoke(sender, lmea);
        }

        public static void PublishInfo(this ILogger sender, string message) { PublishEvent(sender, LevelType.INFO, message); }
        public static void PublishDebug(this ILogger sender, string message) { PublishEvent(sender, LevelType.DEBUG, message); }
        public static void PublishWarning(this ILogger sender, string message) { PublishEvent(sender, LevelType.WARNING, message); }
        public static void PublishError(this ILogger sender, string message) { PublishEvent(sender, LevelType.ERROR, message); }
    }
}
