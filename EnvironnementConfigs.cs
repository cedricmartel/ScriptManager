using System.Configuration;

namespace ScriptManager
{
    public static class EnvironnementConfigs
    {

        private static string envName = null;
        public static string EnvName
        {
            get
            {
                // récupération nom environnement en cours
                if (envName == null)
                {
                    envName = ConfigurationManager.AppSettings["EnvironmentName"];
                    envName = envName
                        .Replace("/", EnvSubNameSeparator)
                        .Replace("\\", EnvSubNameSeparator)
                        .Replace("?", EnvSubNameSeparator)
                        .Replace(":", EnvSubNameSeparator)
                        .Replace("*", EnvSubNameSeparator)
                        .Replace("\"", EnvSubNameSeparator)
                        .Replace("<", EnvSubNameSeparator)
                        .Replace(">", EnvSubNameSeparator);
                }
                return envName;
            }
            set
            {
                envName = value.Replace("/", EnvSubNameSeparator)
                        .Replace("\\", EnvSubNameSeparator)
                        .Replace("?", EnvSubNameSeparator)
                        .Replace(":", EnvSubNameSeparator)
                        .Replace("*", EnvSubNameSeparator)
                        .Replace("\"", EnvSubNameSeparator)
                        .Replace("<", EnvSubNameSeparator)
                        .Replace(">", EnvSubNameSeparator);
            }
        }

        public const string EnvDelimiterInFile = "=";

        /// <summary>
        /// Séparateur dans 
        /// </summary>
        public const string EnvSubNameSeparator = "-";
    }
}
