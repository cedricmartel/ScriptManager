using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptRunner
{
    public class ScriptDetector
    {

        /// <summary>
        /// chargement des fichiers dans l'ordre suivant
        /// 1) dans /SQL
        /// 2) dnas les sous répertoires de /SQL 
        /// </summary>
        public static List<string> FindFilesInDirectory(string directory)
        {
            List<string> res = new List<string>();
            string[] fichiers = Directory.GetFiles(directory);
            if (fichiers.Any())
                // on ajoute les fichiers ordonnés par numéro de script
                // dnas le cas ou les fichiers sont nommés de cette façon 002-CML-insertions varables pour paymen referentiels.configurations et referentiels.parametres
                // si on n'arrive pas à parser le numero, on le met à la fin
                res.AddRange(OrdonneFichiersDossiers(fichiers.Where(x => x.ToLower().EndsWith(".sql"))));

            string[] repertoires = Directory.GetDirectories(directory);
            foreach (var repertoire in OrdonneFichiersDossiers(repertoires))
                res.AddRange(FindFilesInDirectory(repertoire));
            return res;
        }

        private static IEnumerable<string> OrdonneFichiersDossiers(IEnumerable<string> fichiers)
        {
            return fichiers.Select(FileHelper.FormatFileString).OrderBy(x => x).ToList();
        }

    }
}
