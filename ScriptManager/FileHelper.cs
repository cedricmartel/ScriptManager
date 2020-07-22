using System.IO;

namespace ScriptRunner
{
    public class FileHelper
    {

        /// <summary>
        /// Read content of file, utf8 only
        /// </summary>
        /// <param name="fichier"></param>
        /// <returns></returns>
        public static string GetFileContent(string fichier)
        {
            // on lit le fichier utf8
            FileInfo file = new FileInfo(fichier);
            return file.OpenText().ReadToEnd();
        }


        public static string FormatFileString(string input)
        {
            return input.Replace("\\", "/").Replace("//", "/").Replace("//", "/");
        }
    }
}
