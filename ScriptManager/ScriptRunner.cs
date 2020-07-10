using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using ScriptManager.Helper;
using ScriptManager.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;

namespace ScriptManager
{
    public class ScriptRunner
    {
        internal string ConnectionStringCode = string.Empty;
        internal string SqlPath = string.Empty;
        internal string CsFile = null;
        internal bool DisableScriptDiff = false;
        internal string DefaultSqlPath = string.Empty;
        internal string Version = null;

        internal void RunScripts()
        {
            // récupération chaine de connection
            string connectionString = null;
            if (!string.IsNullOrEmpty(CsFile))
            {
                // recuperation connectionString depuis le fichier 
                var doc = XDocument.Load(CsFile);
                foreach (var el in doc.Root.Elements())
                {
                    if (el.Name != "add")
                        continue;
                    var name = el.Attribute("name");
                    if (name?.Value != ConnectionStringCode)
                        continue;
                    connectionString = el.Attribute("connectionString")?.Value;
                    break;
                }

                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("Error: No connection string " + ConnectionStringCode + " found in file " + CsFile);
                    return;
                }
            }
            else
            {
                if (ConfigurationManager.ConnectionStrings.Count == 0)
                {
                    Console.WriteLine("Error: No connection string have been found in Config/Database.config");
                    return;
                }
                connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringCode].ConnectionString;
            }

            if (connectionString.Contains("provider connection string=\""))
                connectionString = connectionString.Split(new[] { "\"" }, StringSplitOptions.None)[1];

            Console.WriteLine("Using the following connection string: ");
            Console.WriteLine(connectionString);


            // création de la table de logs si nécessaire
            if (!DisableScriptDiff)
                SqlHistoryDao.CreateLogTableIfNotExists(connectionString);

            // récupération liste des scripts déjà passés
            List<HistoriqueScriptSql> scriptsDejaPasses = new List<HistoriqueScriptSql>();
            if (!DisableScriptDiff)
                scriptsDejaPasses = SqlHistoryDao.ListScriptsDejaPasses(connectionString);

            // list scripts files to be run, with the correct order 
            List<string> listFichiers;
            try
            {
                listFichiers = ScriptDetector.FindFilesInDirectory(SqlPath);
            }
            catch (Exception ex)
            {
                LogHelper.LogAndInfo("ERROR : " + ex.FormatForLog());
                return;
            }

            List<string> fichiersAPasser = listFichiers.Where(x => !scriptsDejaPasses.Any(y => FileHelper.FormatFileString(y.NomScript) == FileHelper.FormatFileString(x.Replace(SqlPath, DefaultSqlPath)))).ToList();

            #region run scripts and log execution

            int nbScript = 0, nbErreurs = 0, nbSucces = 0;
            foreach (string fichier in fichiersAPasser)
            {
                nbScript++;
                LogHelper.LogAndInfo("RUN SCRIPT " + fichier);
                string messageErreur = string.Empty;
                try
                {
                    string scriptContent = FileHelper.GetFileContent(fichier);

                    var connection = new ServerConnection(new SqlConnection(connectionString))
                    {
                        StatementTimeout = 2592000
                    };
                    var server = new Server(connection);
                    server.ConnectionContext.ExecuteNonQuery(scriptContent);
                    nbSucces++;
                }
                catch (Exception ex)
                {
                    messageErreur = ex.FormatForLog();
                    LogHelper.LogAndInfo("ERROR running script " + fichier + " " + messageErreur);
                    nbErreurs++;
                }
                if (!DisableScriptDiff)
                    SqlHistoryDao.InsertLog(FileHelper.FormatFileString(fichier.Replace(SqlPath, DefaultSqlPath)), messageErreur, connectionString);
            }

            LogHelper.LogAndInfo(string.Format("FINISHED : {0} scripts run, {1} success and {2} errors", nbScript, nbSucces, nbErreurs));

            // update version information
            if (!string.IsNullOrEmpty(Version))
            {
                try
                {
                    string versionScript = @"
    DECLARE @Version VARCHAR(100) = '" + Version.Replace("'", "''") + @"';
    IF NOT EXISTS (SELECT 1 FROM SYS.EXTENDED_PROPERTIES WHERE [major_id] = 0 AND [minor_id] = 0 AND [name] = N'Version')
        EXEC sp_addextendedproperty @name = 'Version', @value = @Version;
    ELSE IF EXISTS (SELECT 1 FROM SYS.EXTENDED_PROPERTIES WHERE [major_id] = 0 AND [minor_id] = 0 AND [name] = N'Version' AND value < @Version)
        EXEC sp_updateextendedproperty @name = 'Version', @value = @Version; 
    ";

                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        var cmd = new SqlCommand(versionScript, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    LogHelper.LogAndInfo("Version number has been set to " + Version);
                }
                catch (Exception ex)
                {
                    var error = ex.FormatForLog();
                    LogHelper.LogAndInfo("ERROR updating version number information: " + error);
                }

            }
            #endregion
        }
    }
}
