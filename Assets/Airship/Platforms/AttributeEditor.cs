/* Copyright Airship and Contributors */

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AirshipSDK {

    /// <summary>
    /// An editor for channel attributes
    /// </summary>
    public class AttributeEditor {
        private Action<string> onApply;
        private IList<AttributeMutation> operations = new List<AttributeMutation> ();

        internal AttributeEditor (Action<string> onApply) {
            this.onApply = onApply;
        }

        /// <summary>
        /// Sets a string attribute.
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        /// <param name="value">The attribute string greater than one character and less than 1024 characters in length.</param>
        public AttributeEditor SetAttribute (string key, string value) {
            if (IsInvalidField (key) || IsInvalidField(value)) {
                return this;
            }
            operations.Add (new AttributeMutation (AttributeAction.Set, key, value, AttributeType.String));
            return this;
        }

        /// <summary>
        /// Sets an integer number attribute.
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        /// <param name="value">The number attribute.</param>
        public AttributeEditor SetAttribute (string key, int value) {
            if (IsInvalidField (key)) {
                return this;
            }
            operations.Add (new AttributeMutation (AttributeAction.Set, key, value.ToString(CultureInfo.InvariantCulture), AttributeType.Number));
            return this;
        }

        /// <summary>
        /// Sets an long number attribute.
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        /// <param name="value">The number attribute.</param>
        public AttributeEditor SetAttribute (string key, long value) {
            if (IsInvalidField (key)) {
                return this;
            }
            operations.Add (new AttributeMutation (AttributeAction.Set, key, value.ToString(CultureInfo.InvariantCulture), AttributeType.Number));
            return this;
        }

        /// <summary>
        /// Sets a float number attribute.
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        /// <param name="value">The number attribute.</param>
        public AttributeEditor SetAttribute (string key, float value) {
            if (IsInvalidField (key)) {
                return this;
            }
            if (float.IsNaN (value) || float.IsInfinity (value)) {
                throw new FormatException ("Infinity or NaN: " + value);
            }
            operations.Add (new AttributeMutation (AttributeAction.Set, key, value.ToString(CultureInfo.InvariantCulture), AttributeType.Number));
            return this;
        }

        /// <summary>
        /// Sets a double number attribute.
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        /// <param name="value">The number attribute.</param>
        public AttributeEditor SetAttribute (string key, double value) {
            if (IsInvalidField (key)) {
                return this;
            }
            if (double.IsNaN (value) || double.IsInfinity (value)) {
                throw new FormatException ("Infinity or NaN: " + value);
            }
            operations.Add (new AttributeMutation (AttributeAction.Set, key, value.ToString(CultureInfo.InvariantCulture), AttributeType.Number));
            return this;
        }

        /// <summary>
        /// Sets a date attribute.
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        /// <param name="value">The date attribute value.</param>
        public AttributeEditor SetAttribute(string key, DateTime value)
        {
            if (IsInvalidField(key))
            {
                return this;
            }

            // Pass date to the plugin as seconds since the epoch
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            string valueInMillisecondsSinceEpoch = (value - epochStart).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);

            operations.Add(new AttributeMutation(AttributeAction.Set, key, valueInMillisecondsSinceEpoch, AttributeType.Date));
            return this;
        }

        /// <summary>
        /// Removes an attribute.
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        public AttributeEditor RemoveAttribute (string key) {
            if (IsInvalidField (key)) {
                return this;
            }
            operations.Add (new AttributeMutation (AttributeAction.Remove, key, null, null));
            return this;
        }

        private bool IsInvalidField (string key) {
            if (key == null || key.Length == 0) {
                return true;
            }

            if (key.Length > 1024) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Applies pending changes.
        /// </summary>
        public void Apply () {
            if (onApply != null) {
                var sb = new StringBuilder();
                sb.Append("[");
                for (int i = 0; i < operations.Count; i++) {
                    if (i > 0) sb.Append(",");
                    sb.Append(operations[i].ToJson());
                }
                sb.Append("]");
                onApply(sb.ToString());
            }
        }

        internal enum AttributeType {
            None,
            Integer,
            Long,
            Float,
            Double,
            String,
            Date,
            Number
        }

        internal enum AttributeAction {
            Set,
            Remove
        }

        [Serializable]
        internal class AttributeMutation {

#pragma warning disable
            // Used for JSON encoding/decoding

            [SerializeField]
            private string action;

            [SerializeField]
            private string key;

            [SerializeField]
            private string value;

            [SerializeField]
            private string? type;
#pragma warning restore

            public AttributeMutation (AttributeAction action, string key, string value, AttributeType? type) {
                this.action = action.ToString().ToLower();
                this.key = key;
                this.value = value;
                this.type = type?.ToString().ToLower();
            }

            public string ToJson() {
                // Keys and values are caller-supplied, so they must be escaped. A raw quote
                // or backslash would otherwise produce malformed JSON, and the native parsers
                // reject the whole batch rather than just the offending operation.
                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"action\":").Append(AirshipUtils.ToJsonString(action));
                sb.Append(",\"key\":").Append(AirshipUtils.ToJsonString(key));
                if (value != null) {
                    // Number and date values must reach the proxy as JSON numbers.
                    bool isNumericType = type == "number" || type == "date";
                    if (isNumericType) {
                        sb.Append(",\"value\":").Append(value);
                    } else {
                        sb.Append(",\"value\":").Append(AirshipUtils.ToJsonString(value));
                    }
                }
                if (type != null) {
                    sb.Append(",\"type\":").Append(AirshipUtils.ToJsonString(type));
                }
                sb.Append("}");
                return sb.ToString();
            }
        }
    }
}
