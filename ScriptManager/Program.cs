using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using ScriptManager.Helper;
using ScriptManager.Model;

namespace ScriptManager
{
    class Program
    {

        public const string DefaultSqlPath = @"SQL/";

        static void Main(string[] args)
        {
            try
            {
                #region initialisation cong & verif parametres

                // Initialize log4net.
                log4net.Config.XmlConfigurator.Configure();

                LogHelper.LogInfo("");
                LogHelper.LogInfo("---------------------------------------------------");
                LogHelper.LogInfo("Script manager Start");

                if (args == null)
                {
                    PrintUsage();
                    return;
                }

                string connectionStringCode = string.Empty;
                string sqlPath = DefaultSqlPath;
                string csFile = null;
                bool disableScriptDiff = false;
                string version = null;

                // read parameters
                for (int i = 0; i < args.Length; i += 2)
                {
                    var command = args[i];
                    var value = args[i + 1];

                    if (command == "/csName")
                        connectionStringCode = value;
                    if (command == "/sqlPath")
                        sqlPath = value;
                    if (command == "/envCode")
                        EnvironnementConfigs.EnvName = value;
                    if (command == "/csFile")
                        csFile = value;
                    if (command == "/disableScriptDiff")
                        disableScriptDiff = value == "1";
                    if (command == "/version")
                        version = value;
                }
                #endregion

                // mandatory parameters check
                if (string.IsNullOrEmpty(connectionStringCode) || string.IsNullOrEmpty(sqlPath))
                {
                    PrintUsage();
                    return;
                }

                // run scripts
                var runner = new ScriptRunner()
                {
                    ConnectionStringCode = connectionStringCode,
                    SqlPath = sqlPath,
                    DefaultSqlPath = DefaultSqlPath, 
                    CsFile = csFile,
                    DisableScriptDiff = disableScriptDiff, 
                    Version = version
                };
                runner.RunScripts();

            }
            catch (Exception ex)
            {
                LogHelper.LogAndInfo("ERROR : " + ex.FormatForLog());
            }
        }

        private static void PrintUsage()
        {
            LogHelper.LogAndInfo("USAGE: ScriptManager.exe /csName NomDeLaChaineDeConnection [/sqlPath pathToSqlDirectory] [/envCode forcedEnvCode] [/csFile pathToConfFile] [/disableScriptDiff 1] [/version versionString]");
            LogHelper.LogAndInfo("ex: ScriptManager.exe /csName NameOfCS /sqlPath \"../../../../SQL/\" /envCode \"SDIS95\" /csFile \"../../AgendisConfig/VS/Database.Config\" /version \"3.1.0#85\"");
            LogHelper.LogAndInfo("PARAMETERS: ");
            LogHelper.LogAndInfo(" - sqlPath: path of scripts folder (default is ./SQL)");
            LogHelper.LogAndInfo(" - envCode: environment code: scripts whose file name contains =envCode= will be executed (default empty)");
            LogHelper.LogAndInfo(" - csFile: path to connection strings file (defaut ./Config/Database.config)");
            LogHelper.LogAndInfo(" - disableScriptDiff: when set to 1, run all the scripts even those already passed previously (default 0)");
            LogHelper.LogAndInfo(" - version: label of version, will update row in system table SYS.EXTENDED_PROPERTIES whose name is 'VERSION' (not updated if not provided)");
            LogHelper.LogAndInfo(" - csName: name of connection string to be used (mandatory)");
            LogHelper.LogAndInfo("   --> Connection strings in default file are: ");
            for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
                LogHelper.LogAndInfo("      - " + ConfigurationManager.ConnectionStrings[i].Name);
        }

    }
}
