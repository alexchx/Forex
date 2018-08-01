using System;
using System.Collections.Generic;

namespace Forex
{
    public static class ValueConverter
    {
        public static T Convert<T>(object value)
        {
            if (typeof(T).IsEnum)
            {
                string strValue = value?.ToString();
                if (!string.IsNullOrEmpty(strValue))
                {
                    return (T)Enum.Parse(typeof(T), strValue);
                }

                return default(T);
            }
            else
            {
                return (T)System.Convert.ChangeType(value, typeof(T));
            }
        }

        public static string[] Split(string values)
        {
            return Split<string>(values);
        }

        public static string[] Split(string values, string separator)
        {
            return Split<string>(values, separator);
        }

        public static T[] Split<T>(string values)
        {
            return Split<T>(values, ",");
        }

        public static T[] Split<T>(string values, string separator)
        {
            if (string.IsNullOrEmpty(values))
            {
                return new T[0];
            }

            string[] segments = values.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            T[] results = new T[segments.Length];
            for (int i = 0; i < segments.Length; i++)
            {
                results[i] = Convert<T>(segments[i]);
            }

            return results;
        }

        public static string Join<T>(IEnumerable<T> values)
        {
            return Join<T>(values, ",");
        }

        public static string Join<T>(IEnumerable<T> values, string separator)
        {
            return values == null ? string.Empty : string.Join(separator, values);
        }
    }
}
