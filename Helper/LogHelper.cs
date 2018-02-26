using System;
using System.Text;
using log4net;

namespace ScriptManager.Helper
{
    /// <summary>
    /// Extension of the class Eception
    /// </summary>
    public static class LogHelper
    {
        private static ILog logger = null;
        public static ILog Logger
        {
            get
            {
                if (logger == null)
                    logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

                return logger;
            }
        }

        #region debug
        /// <summary>
        /// Ajoute un log de debug
        /// </summary>
        /// <param name="exp">The Exception </param>
        public static void LogDebug(this Exception exp)
        {
            if (exp != null)
                Logger.Debug(exp.GetLog());
        }
        /// <summary>
        /// Ajoute un log de debug
        /// </summary>
        public static void LogDebug(string message)
        {
            Logger.Debug(message);
        }
        #endregion

        #region info
        /// <summary>
        /// Ajoute un log d'information
        /// </summary>
        /// <param name="exp">The Exception </param>
        public static void LogInfo(this Exception exp)
        {
            if (exp != null)
                Logger.Info(exp.GetLog());
        }

        public static void LogInfo(this Exception exp, string message)
        {
            Logger.Info(message);
            if (exp != null)
                Logger.Info(exp.GetLog());
        }

        /// <summary>
        /// Ajoute un log d'information
        /// </summary>
        public static void LogInfo(string message)
        {
            Logger.Info(message);
        }
        #endregion

        #region warn
        /// <summary>
        /// Ajoute un log de warning
        /// </summary>
        /// <param name="exp">The Exception </param>
        public static void LogWarn(this Exception exp)
        {
            if (exp != null)
                Logger.Warn(exp.GetLog());
        }
        /// <summary>
        /// Ajoute un log de warning
        /// </summary>
        public static void LogWarn(string message)
        {
            Logger.Warn(message);
        }
        #endregion

        #region error & fatal
        /// <summary>
        /// Ajoute un log d'erreur
        /// </summary>
        /// <param name="exp">The Exception </param>
        public static void LogError(this Exception exp)
        {
            if (exp != null)
                Logger.Error(exp.GetLog());
        }
        /// <summary>
        /// Ajoute un log d'erreur
        /// </summary>
        public static void LogError(string message)
        {
            Logger.Error(message);
        }

        /// <summary>
        /// Ajoute un log d'erreur fatale
        /// </summary>
        /// <param name="exp">The Exception </param>
        public static void LogFatal(this Exception exp)
        {
            if (exp != null)
                Logger.Fatal(exp.GetLog());
        }
        /// <summary>
        /// Ajoute un log d'erreur fatale
        /// </summary>
        public static void LogFatal(string message)
        {
            Logger.Fatal(message);
        }
        #endregion

        /// <summary>
        /// Add method Log to the Exception Class
        /// </summary>
        /// <param name="exp">The Exception </param>
        private static string GetLog(this Exception exp)
        {
            var myErrorMessage = new StringBuilder();
            while (exp != null)
            {
                myErrorMessage.Append("Server: " + Environment.MachineName + "\r\n");
                myErrorMessage.Append("ExceptionType: " + exp.GetType() + "\r\n");
                myErrorMessage.Append("Message: " + exp.Message + "\r\n");
                myErrorMessage.Append("Source: " + exp.Source + "\r\n");
                myErrorMessage.Append("Target site: " + Convert.ToString(exp.TargetSite) + "\r\n");
                myErrorMessage.Append(exp.StackTrace + "\r\n");
                exp = exp.InnerException;
                if(exp != null)
                    myErrorMessage.Append("Nested Exception----");
            }
            return myErrorMessage.ToString();
        }
    }
}