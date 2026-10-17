using System;

namespace Akeel.Utilities.ExtensionMethods
{
    public static class EnumExtensions
    {
        public static int ToNumber<T>(this T source) where T : IConvertible//enum
        {
            //if (!source.GetType().IsEnum)
            //    throw new ArgumentException("T must be an enumerated type");

            return (int)(IConvertible)source;
        }

        public static string ToNumberString<T>(this T source) where T : IConvertible//enum
        {
            //if (!source.GetType().IsEnum)
            //    throw new ArgumentException("T must be an enumerated type");

            return ((int)(IConvertible)source).ToString();
        }

        public static byte ToByte(this Enum source)
        {
            return Convert.ToByte(source);
        }
    }
}
    