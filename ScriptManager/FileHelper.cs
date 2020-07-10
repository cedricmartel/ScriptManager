using System.IO;
using System.Text;
using ScriptManager.Helper;

namespace ScriptManager
{
    public class FileHelper
    {
        public static string GetFileContent(string fichier)
        {
            Encoding fileEncoding = EncodingHelper.GetType(fichier);
            string scriptContent;
            if (fileEncoding.Equals(Encoding.UTF8))
            {
                // on lit le fichier utf8
                FileInfo file = new FileInfo(fichier);
                scriptContent = file.OpenText().ReadToEnd();
            }
            else
            {
                // on fait comme on peut
                scriptContent = File.ReadAllText(fichier, fileEncoding);
            }
            return scriptContent;
        }


        public static string FormatFileString(string input)
        {
            return input.Replace("\\", "/").Replace("//", "/").Replace("//", "/");
        }
    }
}
