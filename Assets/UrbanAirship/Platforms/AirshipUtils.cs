/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UrbanAirship
{
    /// <summary>
    /// Utility class for Airship serialization and helper methods.
    /// </summary>
    public static class AirshipUtils
    {
        /// <summary>
        /// Serializes an object to JSON, handling enums with [AirshipEnumStringValue] attributes,
        /// nullable types, arrays, and nested objects/records.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>JSON string representation of the object.</returns>
        public static string Serialize(object obj)
        {
            if (obj == null)
            {
                return "{}";
            }

            var jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{");

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            bool first = true;

            // Process properties
            foreach (PropertyInfo prop in properties)
            {
                if (!prop.CanRead) continue;

                object? value = prop.GetValue(obj);
                if (value == null) continue;

                if (!first) jsonBuilder.Append(",");
                first = false;

                jsonBuilder.Append($"\"{prop.Name}\":");
                jsonBuilder.Append(SerializeValue(value));
            }

            // Process fields
            foreach (FieldInfo field in fields)
            {
                object? value = field.GetValue(obj);
                if (value == null) continue;

                if (!first) jsonBuilder.Append(",");
                first = false;

                jsonBuilder.Append($"\"{field.Name}\":");
                jsonBuilder.Append(SerializeValue(value));
            }

            jsonBuilder.Append("}");
            return jsonBuilder.ToString();
        }

        /// <summary>
        /// Serializes a single value, handling enums with [AirshipEnumStringValue] attributes,
        /// nullable types, arrays, and nested objects/records.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>JSON string representation of the value.</returns>
        public static string SerializeValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            Type valueType = value.GetType();

            // Handle nullable types - get the underlying type
            Type? underlyingType = Nullable.GetUnderlyingType(valueType);
            Type actualType = underlyingType ?? valueType;

            // Handle nullable enums
            if (underlyingType != null && underlyingType.IsEnum)
            {
                Enum enumValue = (Enum)value;
                string stringValue = enumValue.ToStringValue();
                return $"\"{EscapeJsonString(stringValue)}\"";
            }

            // Handle non-nullable enums
            if (actualType.IsEnum)
            {
                Enum enumValue = (Enum)value;
                string stringValue = enumValue.ToStringValue();
                return $"\"{EscapeJsonString(stringValue)}\"";
            }

            // Handle arrays (including nullable arrays)
            if (actualType.IsArray)
            {
                Array array = (Array)value;
                var arrayBuilder = new StringBuilder();
                arrayBuilder.Append("[");
                
                for (int i = 0; i < array.Length; i++)
                {
                    if (i > 0) arrayBuilder.Append(",");
                    arrayBuilder.Append(SerializeValue(array.GetValue(i)));
                }
                
                arrayBuilder.Append("]");
                return arrayBuilder.ToString();
            }

            // Handle IList/ICollection (for generic collections)
            if (value is IEnumerable enumerable && !(value is string))
            {
                var listBuilder = new StringBuilder();
                listBuilder.Append("[");
                
                bool first = true;
                foreach (object item in enumerable)
                {
                    if (!first) listBuilder.Append(",");
                    first = false;
                    listBuilder.Append(SerializeValue(item));
                }
                
                listBuilder.Append("]");
                return listBuilder.ToString();
            }

            // Handle strings
            if (actualType == typeof(string))
            {
                return $"\"{EscapeJsonString((string)value)}\"";
            }

            // Handle booleans (including nullable)
            if (actualType == typeof(bool))
            {
                return value.ToString().ToLower();
            }

            // Handle numbers (including nullable)
            if (actualType.IsPrimitive || actualType == typeof(decimal) || actualType == typeof(float) || valueType == typeof(double))
            {
                return value.ToString();
            }

            // Handle nested objects/records - recursively serialize them
            // This handles ConfigEnvironment, IOSConfig, AndroidConfig, etc.
            return Serialize(value);
        }

        /// <summary>
        /// Escapes special characters in JSON strings.
        /// </summary>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        private static string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "{}")
            {
                return default(T);
            }
            Type type = typeof(T);
            Type underlyingType = Nullable.GetUnderlyingType(type);
            Type actualType = underlyingType ?? type;
            // Primitives
            if (actualType == typeof(bool))
                return (T)(object)bool.Parse(json);
            if (actualType == typeof(int))
                return (T)(object)int.Parse(json);
            if (actualType == typeof(long))
                return (T)(object)long.Parse(json);
            if (actualType == typeof(float))
                return (T)(object)float.Parse(json, System.Globalization.CultureInfo.InvariantCulture);
            if (actualType == typeof(double))
                return (T)(object)double.Parse(json, System.Globalization.CultureInfo.InvariantCulture);
            // String
            if (actualType == typeof(string))
            {
                if (json.StartsWith("\"") && json.EndsWith("\""))
                    return (T)(object)json.Substring(1, json.Length - 2);
                return (T)(object)json;
            }
            // string[] — top-level JSON array of strings like ["a","b"]
            if (actualType == typeof(string[]))
            {
                return (T)(object)JsonArray<string>.FromJson(json).values;
            }
            // For other arrays of serializable objects, use the JsonArray wrapper
            if (actualType.IsArray)
            {
                Type elementType = actualType.GetElementType();
                Type jsonArrayType = typeof(JsonArray<>).MakeGenericType(elementType);
                var method = jsonArrayType.GetMethod("FromJson", BindingFlags.Public | BindingFlags.Static);
                var wrapper = method.Invoke(null, new object[] { json });
                var valuesField = jsonArrayType.GetField("values");
                return (T)valuesField.GetValue(wrapper);
            }

            // Let's check if I can fix QuietTime? result differently
            // if (underlyingType != null)
            // {
            //     var method = typeof(JsonUtility).GetMethod("FromJson", new[] { typeof(string) })
            //         .MakeGenericMethod(underlyingType);
            //     return (T)method.Invoke(null, new object[] { json });
            // }

            // Serializable objects — unwrap nullable and use JsonUtility
            return JsonUtility.FromJson<T>(json);
        }
    }
}
