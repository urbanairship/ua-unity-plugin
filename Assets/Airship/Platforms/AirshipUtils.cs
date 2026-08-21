/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;

#nullable enable annotations

namespace AirshipSDK
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

            // Handle dictionaries -> JSON objects (checked before IEnumerable,
            // since IDictionary is also IEnumerable)
            if (value is IDictionary dictionary)
            {
                var dictBuilder = new StringBuilder();
                dictBuilder.Append("{");

                bool firstEntry = true;
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (!firstEntry) dictBuilder.Append(",");
                    firstEntry = false;

                    dictBuilder.Append($"\"{EscapeJsonString(entry.Key.ToString())}\":");
                    dictBuilder.Append(entry.Value == null ? "null" : SerializeValue(entry.Value));
                }

                dictBuilder.Append("}");
                return dictBuilder.ToString();
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

            // Handle numbers (including nullable).
            if (actualType.IsPrimitive || actualType == typeof(decimal) || actualType == typeof(float) || valueType == typeof(double))
            {
                return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            }

            // Handle nested objects/records - recursively serialize them
            // This handles ConfigEnvironment, IOSConfig, AndroidConfig, etc.
            return Serialize(value);
        }

        /// <summary>
        /// Parses a string value into an enum by matching [AirshipEnumStringValue] attributes.
        /// Falls back to Enum.Parse if no attribute match is found.
        /// </summary>
        internal static object ParseEnumFromStringValue(Type enumType, string stringValue)
        {
            foreach (FieldInfo field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                AirshipEnumStringValueAttribute attr = field.GetCustomAttribute<AirshipEnumStringValueAttribute>();
                if (attr != null && attr.StringValue == stringValue)
                {
                    return field.GetValue(null);
                }
            }
            return Enum.Parse(enumType, stringValue, true);
        }

        /// <summary>
        /// Parses a string value into an enum by matching [AirshipEnumStringValue] attributes,
        /// returning <paramref name="fallback"/> when the value is missing or unrecognized.
        ///
        /// Unity's JsonUtility only maps enums from integers, so string-valued enums that
        /// arrive inside a [Serializable] payload have to be parsed through here instead.
        /// </summary>
        internal static T ParseEnum<T>(string stringValue, T fallback) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                return fallback;
            }

            try
            {
                return (T)ParseEnumFromStringValue(typeof(T), stringValue);
            }
            catch (Exception)
            {
                Debug.LogWarning("Airship: unrecognized " + typeof(T).Name + " value '" + stringValue + "'");
                return fallback;
            }
        }

        /// <summary>
        /// Escapes a string for embedding in JSON. Handles the required escapes plus the
        /// C0 control range, which a plain Replace chain misses and which would otherwise
        /// produce a payload the native JSON parsers reject.
        /// </summary>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        internal static string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var builder = new StringBuilder(str.Length + 8);
            foreach (char c in str)
            {
                switch (c)
                {
                    case '\\': builder.Append("\\\\"); break;
                    case '"': builder.Append("\\\""); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    case '\b': builder.Append("\\b"); break;
                    case '\f': builder.Append("\\f"); break;
                    default:
                        if (c < ' ')
                        {
                            builder.Append("\\u").Append(((int)c).ToString("x4", System.Globalization.CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(c);
                        }
                        break;
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Wraps a string as a quoted, escaped JSON string literal.
        /// </summary>
        internal static string ToJsonString(string str)
        {
            return str == null ? "null" : "\"" + EscapeJsonString(str) + "\"";
        }

        public static T Deserialize<T>(string json)
        {
            Type type = typeof(T);
            Type underlyingType = Nullable.GetUnderlyingType(type);
            Type actualType = underlyingType ?? type;

            if (string.IsNullOrEmpty(json) || json == "{}")
            {
                // Arrays are returned empty rather than null so callers can enumerate
                // the result without a null check.
                if (actualType.IsArray)
                {
                    return (T)(object)Array.CreateInstance(actualType.GetElementType(), 0);
                }
                return default(T);
            }
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
            // Enums with [AirshipEnumStringValue] — parse from string value
            if (actualType.IsEnum)
            {
                string enumStr = json;
                if (enumStr.StartsWith("\"") && enumStr.EndsWith("\""))
                    enumStr = enumStr.Substring(1, enumStr.Length - 2);
                return (T)(object)ParseEnumFromStringValue(actualType, enumStr);
            }
            // string[] — top-level JSON array of strings like ["a","b"]
            if (actualType == typeof(string[]))
            {
                return (T)(object)(JsonArray<string>.FromJson(json).values ?? new string[0]);
            }
            // Enum arrays — parse JSON array of strings into enum values
            if (actualType.IsArray && actualType.GetElementType().IsEnum)
            {
                Type elementType = actualType.GetElementType();
                string[] stringValues = JsonArray<string>.FromJson(json).values;
                if (stringValues == null)
                    return (T)(object)Array.CreateInstance(elementType, 0);
                Array enumArray = Array.CreateInstance(elementType, stringValues.Length);
                for (int i = 0; i < stringValues.Length; i++)
                {
                    enumArray.SetValue(ParseEnumFromStringValue(elementType, stringValues[i]), i);
                }
                return (T)(object)enumArray;
            }
            // For other arrays of serializable objects, use the JsonArray wrapper
            if (actualType.IsArray)
            {
                Type elementType = actualType.GetElementType();
                Type jsonArrayType = typeof(JsonArray<>).MakeGenericType(elementType);
                var method = jsonArrayType.GetMethod("FromJson", BindingFlags.Public | BindingFlags.Static);
                var wrapper = method.Invoke(null, new object[] { json });
                var valuesField = jsonArrayType.GetField("values");
                object values = valuesField.GetValue(wrapper);
                return (T)(values ?? Array.CreateInstance(elementType, 0));
            }

            // Serializable objects — unwrap nullable and use JsonUtility
            return JsonUtility.FromJson<T>(json);
        }
    }
}
