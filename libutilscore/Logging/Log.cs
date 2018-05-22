
using NLog;

namespace libutilscore.Logging
{
    public class Log
    {
        public static Logger Logger { get; private set; }
        static Log()
        {
            LogManager.ReconfigExistingLoggers();
            Logger = LogManager.GetCurrentClassLogger();
        }
    }
}
