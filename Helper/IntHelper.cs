namespace ScriptManager.Helper
{
    public static class IntHelper
    {
        public static int TryParseDefault(string stringToParse, int defaultValue)
        {
            int res = defaultValue;
            if (!string.IsNullOrEmpty(stringToParse))
            {
                int.TryParse(stringToParse, out res);
            }
            return res;
        }
    }
}
