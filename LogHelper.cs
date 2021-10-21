using log4net;

namespace Common
{
    /// <summary>
    /// log4netHelper帮助类（暂时废弃）
    /// Author：Carey
    /// Create Date：2019年4月23日10:33:08 
    /// </summary>
    public class LogHelper
    {
        public static ILog GetLog<T>(T t)
        {
            ILog _log = LogManager.GetLogger("","");
            if (t !=null)
            {
                _log = LogManager.GetLogger(t.GetType());
            }

            return _log;
        }
        /// <summary>
        /// 根据Log名称获取Log instance
        /// </summary>
        /// <param name="loggerName">Log名称</param>
        public static ILog GetLog(string loggerName)
        {
            ILog _log = LogManager.GetLogger("", loggerName);
            
            return _log;
        }
    }
}