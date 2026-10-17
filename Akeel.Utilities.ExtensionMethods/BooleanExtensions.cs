namespace Akeel.Utilities.ExtensionMethods
{
    public static class BooleanExtensions
    {
        public static bool IsTrue(this bool value) => value;
        public static bool IsFalse(this bool value) => !value;
    }
}