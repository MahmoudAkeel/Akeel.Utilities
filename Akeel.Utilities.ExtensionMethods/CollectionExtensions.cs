using System.Collections.Generic;
using System.Linq;

namespace Akeel.Utilities.ExtensionMethods
{
    public static class CollectionExtensions
    {
        public static bool AnyGuard<T>(this ICollection<T> collection)
            => collection.IsNotNull() && collection.Any();

        public static bool EmptyOrNull<T>(this ICollection<T> collection)
            => AnyGuard(collection).IsFalse();
    }
}
