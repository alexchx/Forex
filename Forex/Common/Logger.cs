using log4net;
using log4net.Config;
using System;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Forex
{
    public class Logger
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static Logger()
        {
            XmlConfigurator.Configure();
        }

        public static void LogError(string message, params object[] args)
        {
            Log(TraceLevel.Error, null, message, args);
        }

        public static void LogError(Exception exception)
        {
            Log(TraceLevel.Error, exception, null);
        }

        public static void LogError(Exception exception, string message, params object[] args)
        {
            Log(TraceLevel.Error, exception, message, args);
        }

        public static void LogWarning(string message, params object[] args)
        {
            Log(TraceLevel.Warning, null, message, args);
        }

        public static void LogWarning(Exception exception)
        {
            Log(TraceLevel.Warning, exception, null);
        }

        public static void LogWarning(Exception exception, string message, params object[] args)
        {
            Log(TraceLevel.Warning, exception, message, args);
        }

        public static void LogInformation(string message, params object[] args)
        {
            Log(TraceLevel.Info, null, message, args);
        }

        public static void LogInformation(Exception exception)
        {
            Log(TraceLevel.Info, exception, null);
        }

        public static void LogInformation(Exception exception, string message, params object[] args)
        {
            Log(TraceLevel.Info, exception, message, args);
        }

        public static void Log(TraceLevel level, Exception exception, string message, params object[] args)
        {
            string revisedMsg = string.IsNullOrEmpty(message) ? string.Empty : string.Format(message, args);
            
            if (exception is DbEntityValidationException valEx)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var eve in valEx.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        builder.AppendFormat("{0}, {1}; ", ve.PropertyName, ve.ErrorMessage);
                    }
                }

                revisedMsg += "\tEntityValidationErrors: " + builder.ToString();
            }

            switch (level)
            {
                case TraceLevel.Error:
                    _log.Error(revisedMsg, exception);
                    break;
                case TraceLevel.Warning:
                    _log.Warn(revisedMsg, exception);
                    break;
                default:
                    _log.Info(revisedMsg, exception);
                    break;
            }
        }
    }
}
