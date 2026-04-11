using BigProject.Systems;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

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

        public static void ThrowIfNullFormat<T>(T target, string msg = "")
        {
            StackFrame frame = new(1);
            MethodBase method = frame.GetMethod();
            string callerClass = method?.DeclaringType?.Name ?? "UnknownClass";
            string callerMethod = method?.Name ?? "UnknownMethod";
            string caller = $"{callerClass}.{callerMethod}";

            string typeName = target?.GetType().Name ?? typeof(T).Name;

            if (target == null)
            {
                throw new NullReferenceException($"{string.Format(LogStr.CRITICAL_NULL_REFERENCE, caller, typeName)} {msg}");
            }

            if (target is UnityEngine.Object unityObj && !unityObj)
            {
                throw new NullReferenceException($"{string.Format(LogStr.CRITICAL_NULL_REFERENCE, caller, typeName)} {msg}");
            }
        }

        public static void ThrowIfEmptyCollection(ICollection collection, string name)
        {
            if (collection.Count == 0)
                throw new InvalidOperationException(string.Format(AUTHOR_EXCEPTION_MSG, name, LogStr.CRITICAL_EMPTY_COLLECTION));
        }
    }
}