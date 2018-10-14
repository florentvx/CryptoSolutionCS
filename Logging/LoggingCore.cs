using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Logging
{
    public enum LevelType
    {
        INFO,
        DEBUG,
        WARNING,
        ERROR
    }

    public class LogMessageEventArgs : EventArgs
    {
        public LevelType Level { get; set; }
        public string Message { get; set; }

        public string PrintedMessage { get { return $"{Level.ToString()} - {Message}\n"; } }
        public Color GetMessageColor
        {
            get
            {
                switch (Level)
                {
                    case LevelType.INFO:
                        return Color.White;
                    case LevelType.DEBUG:
                        return Color.Green;
                    case LevelType.WARNING:
                        return Color.Orange;
                    case LevelType.ERROR:
                        return Color.Red;
                    default:
                        return Color.Yellow;
                }
            }
        }
    }

    public delegate void LoggingEventHandler(object sender, LogMessageEventArgs e);
}
