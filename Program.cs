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
        public static ILog Logger;
        public static void LogAndInfos(string text)
        {
            Logger.Info(text);
            Console.WriteLine(text);
        }

        public const string DefaultSqlPath = @"SQL/";

        static void Main(string[] args)
        {
            try
            {
                #region initialisation cong & verif parametres

                // Initialize log4net.
                log4net.Config.XmlConfigurator.Configure();
                Logger = LogManager.GetLogger(typeof(Program));
                Logger.Info("");
                Logger.Info("");
                LogHelper.LogInfo("---------------------------------------------------");
                LogHelper.LogInfo("Application Start");

                if (args == null)
                {
                    PrintUsage();
                    return;
                }

                string connectionStringCode = string.Empty;
                string sqlPath = DefaultSqlPath;
                string csFile = null;
                bool disableScriptDiff = false;

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
                }

                if (string.IsNullOrEmpty(connectionStringCode) || string.IsNullOrEmpty(sqlPath))
                {
                    PrintUsage();
                    return;
                }

                // récupération chaine de connection
                string connectionString = null;
                if (!string.IsNullOrEmpty(csFile))
                {
                    // recuperation connectionString depuis le fichier 
                    var doc = XDocument.Load(csFile);
                    foreach(var el in doc.Root.Elements())
                    {
                        if (el.Name != "add")
                            continue;
                        var name = el.Attribute("name");
                        if (name?.Value != connectionStringCode)
                            continue;
                        connectionString = el.Attribute("connectionString")?.Value;
                        break;
                    }

                    if(string.IsNullOrEmpty(connectionString))
                    {
                        Console.WriteLine("Erreur: Aucune chaine de connection " + connectionStringCode + " n'a été trouvée dans " + csFile);
                        return;
                    }
                }
                else
                {
                    if (ConfigurationManager.ConnectionStrings.Count == 0)
                    {
                        Console.WriteLine("Erreur: Aucune chaine de connection n'a été trouvée dans Config/Database.config");
                        return;
                    }
                    connectionString = ConfigurationManager.ConnectionStrings[connectionStringCode].ConnectionString;
                }
                
                if (connectionString.Contains("provider connection string=\""))
                    connectionString = connectionString.Split(new[] { "\"" }, StringSplitOptions.None)[1];
                
                Console.WriteLine("Utilisation de la chaine de connection suivante:");
                Console.WriteLine(connectionString);
                #endregion

                // création de la table de logs si nécessaire
                if(!disableScriptDiff)
                    SqlHistoryDao.CreateLogTableIfNotExists(connectionString);

                // récupération liste des scripts déjà passés
                List<HistoriqueScriptSql> scriptsDejaPasses = new List<HistoriqueScriptSql>();
                if(!disableScriptDiff)
                    scriptsDejaPasses = SqlHistoryDao.ListScriptsDejaPasses(connectionString);

                // détection des scripts éligibles dans l'ordre 
                List<string> listFichiers;
                try
                {
                    listFichiers = ScriptDetector.FindFilesInDirectory(sqlPath);
                }
                catch (Exception ex)
                { 
                    LogAndInfos("ERROR : " + ex.FormatForLog());
                    return;
                }
                

                List<string> fichiersAPasser = listFichiers.Where(x => !scriptsDejaPasses.Any(y => FileHelper.FormatFileString(y.NomScript) == FileHelper.FormatFileString(x.Replace(sqlPath, DefaultSqlPath)))).ToList();

                #region execution scripts + logs dans la table de logs

                int nbScript = 0, nbErreurs = 0, nbSucces = 0;
                foreach (string fichier in fichiersAPasser)
                {
                    nbScript ++;
                    LogAndInfos("PASSAGE DU SCRIPT " + fichier);
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
                        LogAndInfos("ERREUR lors du passage du script " + fichier + " " + messageErreur);
                        nbErreurs++;
                    }
                    if(!disableScriptDiff)
                        SqlHistoryDao.InsertLog(FileHelper.FormatFileString(fichier.Replace(sqlPath, DefaultSqlPath)), messageErreur, connectionString);
                }

                #endregion
                LogAndInfos(string.Format("Fin du traitement: {0} scripts executés, {1} succes et {2} erreurs", nbScript, nbSucces, nbErreurs));

            }
            catch (Exception ex)
            {
                LogAndInfos("ERROR : " + ex.FormatForLog());
            }
        }

        private static void PrintUsage()
        {
            LogAndInfos("USAGE: ScriptManager.exe /csName NomDeLaChaineDeConnection [/sqlPath pathToSqlDirectory] [/envCode forcedEnvCode] [/csFile pathToConfFile] [/disableScriptDiff 1]");
            LogAndInfos("ex: /csName AgendisEntities /sqlPath \"../../../../SQL/\" /envCode \"SDIS95\" /csFile ../../AgendisConfig/VS/Database.Config");
            LogAndInfos("PARAMETRES: ");
            LogAndInfos(" - sqlPath: chemin vers le dossier de scripts (defaut ./SQL)");
            LogAndInfos(" - envCode: code de l'environnement: les scripts dont le nom contient =envCode= seront passés (defaut vide)");
            LogAndInfos(" - csFile: chemin vers le fichier de chaine de connexion (defaut ./Config/Database.config)");
            LogAndInfos(" - disableScriptDiff: passer à 1 pour ne pas faire de diff avec les scripts déjà passés (tous les scripts du dossier sql seront systématiquement passés à chaque appel)");
            LogAndInfos(" - csName: nom de la chaine de connexion à utiliser (obligatoire)");
            LogAndInfos("   --> les chaines de connection disponibles sont: ");
            for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
                LogAndInfos("      - " + ConfigurationManager.ConnectionStrings[i].Name);
        }

    }
}
