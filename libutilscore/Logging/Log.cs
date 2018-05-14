
using NLog;

namespace libutilscore.Logging
{
    internal class Log
    {
        public static Logger logger { get; private set; }
        static Log()
        {
            LogManager.ReconfigExistingLoggers();
            logger = LogManager.GetCurrentClassLogger();
        }
    }
}
