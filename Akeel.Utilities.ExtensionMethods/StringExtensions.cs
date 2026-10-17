using System.Linq;

namespace Akeel.Utilities.ExtensionMethods
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }


        public static int ToDigits(this string str)
        {
            var digitsStr = string.Join("", str.Where(char.IsDigit));
            return int.TryParse(digitsStr, out var result) ? result : 0;
        }
    }
}