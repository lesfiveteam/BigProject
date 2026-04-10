using BigProject.Systems;
using System;
using System.Collections;

namespace BigProject.Utilities
{
    public static class ExceptionUtilities
    {
        private const string AUTHOR_EXCEPTION_MSG = "{0}: {1}.";

        public static void ThrowIfNull(object arg, string msg = "Null reference exception.")
        {
            if (arg == null)
            {
                throw new NullReferenceException(msg);
            }

            if (arg is UnityEngine.Object unityObj)
            {
                if (!unityObj)
                {
                    throw new NullReferenceException(msg);
                }
            }
        }

        public static void ThrowIfNull(object arg, string author, string msg = "Null reference exception.")
        {
            if (arg == null)
            {
                throw new NullReferenceException(string.Format(AUTHOR_EXCEPTION_MSG, author, msg));
            }
        }

        public static void ThrowIfEmptyCollection(ICollection collection, string name)
        {
            if (collection.Count == 0)
                throw new InvalidOperationException(string.Format(AUTHOR_EXCEPTION_MSG, name, LogStr.CRITICAL_EMPTY_COLLECTION));
        }
    }
}