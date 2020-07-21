namespace Ranka.Utils
{
    public class StringUtils
    {
        public static string DiscordStringFormat(string input)
        {
            string output = input.Trim().Replace("<", "").Replace(">", "");
            return output;
        }
    }
}