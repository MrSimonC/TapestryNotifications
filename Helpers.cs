namespace TapestryNotifications
{
    public static class Helpers
    {

        public static string CleanText(string? toProcess)
        {
            if (toProcess == null)
            {
                return "";
            }

            string result = toProcess;
            for (int i = 0; i < 10; i++)
            {
                result = result.Replace("  ", " ");
            }
            for (int i = 0; i < 10; i++)
            {
                result = result.Replace("\n ", "\n");
            }
            for (int i = 0; i < 10; i++)
            {
                result = result.Replace("\n\n", "\n");
            }
            result = result.Replace("&nbsp;", "");
            result = result.TrimStart('\n')
                .TrimEnd('\n')
                .TrimStart()
                .TrimEnd();
            return result;
        }
    }
}