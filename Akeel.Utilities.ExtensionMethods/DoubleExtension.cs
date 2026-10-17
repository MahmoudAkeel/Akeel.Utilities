using System;

namespace Akeel.Utilities.ExtensionMethods
{
    public static class DoubleExtension
    {
        public static double Negative(this double number) => number.Positive() * -1;
        public static double Positive(this double number) => Math.Abs(number);
    }
}