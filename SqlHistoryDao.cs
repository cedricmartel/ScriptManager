using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using ScriptManager.Helper;
using ScriptManager.Model;

namespace ScriptManager
{
    public static class SqlHistoryDao
    {

        public const string LogTableName = "dbo.HistoriqueScriptSql";

        public static void CreateLogTableIfNotExists(string connectionString)
        {

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("select 1 from " + LogTableName, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                if (ex is SqlException)
                {
                    // on créé la table si elle n'existe pas
                    Program.LogAndInfos("Erreur : la table " + LogTableName + " n'existe pas, elle va etre créée");
                    try
                    {
                        using (var conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            var cmd = new SqlCommand("CREATE TABLE " + LogTableName + "(DateExecution DATETIME2, NomScript VARCHAR(500), MessageErreur VARCHAR(MAX))", conn);
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        Program.LogAndInfos("Erreur lors de la création de la table " + LogTableName);
                        Program.Logger.Error(e.FormatForLog());
                        throw;
                    }
                }
                else
                {
                    Program.LogAndInfos("Erreur : impossible de lire la table " + LogTableName);
                    Program.Logger.Error(ex.FormatForLog());
                    throw;
                }
            }
        }

        public static List<HistoriqueScriptSql> ListScriptsDejaPasses(string connectionString)
        {
            var scriptsDejaPasses = new List<HistoriqueScriptSql>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("select * from " + LogTableName, conn);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        scriptsDejaPasses.Add(new HistoriqueScriptSql
                        {
                            DateExecution = (DateTime) rdr["DateExecution"],
                            MessageErreur = Convert.ToString(rdr["MessageErreur"]),
                            NomScript = Convert.ToString(rdr["NomScript"])
                        });
                    }
                }
                conn.Close();
            }
            return scriptsDejaPasses;
        }

        public static void InsertLog(string fichier, string messageErreur, string connectionString)
        {
            // insertion dans la table de logs de scripts 
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmdInsert = new SqlCommand(@"
                            INSERT INTO " + LogTableName + @"(DateExecution, NomScript, MessageErreur)
                            VALUES (getdate(), '" + FileHelper.FormatFileString(fichier.Replace("'", "''")) + "', '" + messageErreur.Replace("'", "''") + @"') ", conn);
                cmdInsert.ExecuteNonQuery();
                conn.Close();
            }
        }

            
    }
}
