using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptManager.Helper
{
    public static class ExceptionHelper
    {/// <summary>
        /// Add method Log to the Exception Class
        /// </summary>
        /// <param name="exp">The Exception </param>
        public static string FormatForLog(this Exception exp)
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
                if (exp != null)
                {
                    myErrorMessage.Append("Nested Exception----");
                    myErrorMessage.Append(exp.FormatForLog());
                }
            }
            return myErrorMessage.ToString();
        }
    }
}
