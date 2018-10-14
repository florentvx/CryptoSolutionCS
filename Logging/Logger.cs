using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public class Logger
    {
        private LoggingEventHandler LogEventHandler;

        public Logger(LoggingEventHandler logHandler) { LogEventHandler = logHandler; }

        public virtual void PublishEvent(LevelType lvl, string message)
        {
            LogMessageEventArgs lmea = new LogMessageEventArgs { Level = lvl, Message = message };
            LoggingEventHandler handler = LogEventHandler;
            LogEventHandler?.Invoke(this, lmea);
        }

        public virtual void PublishInfo(string message) { PublishEvent(LevelType.INFO, message); }
        public virtual void PublishError(string message) { PublishEvent(LevelType.ERROR, message); }
    }
}
