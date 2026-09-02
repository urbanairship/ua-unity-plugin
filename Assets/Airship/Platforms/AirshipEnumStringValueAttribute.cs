/* Copyright Airship and Contributors */

using System;
using System.Reflection;

#nullable enable annotations

// Namespaced deliberately: `EnumExtensions` is a common name, and as a global-namespace
// static class carrying a `this Enum` extension it collided with identically named helpers
// in customer projects -- turning a plugin upgrade into a compile error in their code.
namespace AirshipSDK
{
    public class AirshipEnumStringValueAttribute : Attribute
    {
        public string StringValue { get; }

        public AirshipEnumStringValueAttribute(string stringValue)
        {
            StringValue = stringValue;
        }
    }

    public static class EnumExtensions
    {
        public static string ToStringValue(this Enum value)
        {
            // Get the FieldInfo for the enum member.
            FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null)
            {
                return value.ToString(); // Fallback to default name
            }

            // Check if the custom attribute exists.
            AirshipEnumStringValueAttribute? attribute = fieldInfo.GetCustomAttribute<AirshipEnumStringValueAttribute>();

            // Return the string value from the attribute, or the default name if none exists.
            return attribute?.StringValue ?? value.ToString();
        }
    }
}
