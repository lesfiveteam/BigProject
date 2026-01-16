using System;
using UnityEngine;

namespace BigProject.Utilities
{
    public static class StringUtilities
    {
        /// <returns>Название поля в перечислении.</returns>
        public static string GetEnumValueName<T>(object value) => Enum.GetName(typeof(T), value);
    }
}
