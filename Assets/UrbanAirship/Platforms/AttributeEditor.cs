/* Copyright Airship and Contributors */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {

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
            operations.Add (new AttributeMutation (AttributeAction.Set, key, value, AttributeType.Integer));
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
            operations.Add (new AttributeMutation (AttributeAction.Set, key, value, AttributeType.Long));
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
            operations.Add (new AttributeMutation (AttributeAction.Set, key, value, AttributeType.Float));
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
            operations.Add (new AttributeMutation (AttributeAction.Set, key, value, AttributeType.Double));
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
            operations.Add (new AttributeMutation (AttributeAction.Remove, key, null, AttributeType.None));
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
                JsonArray<AttributeMutation> jsonArray = new JsonArray<AttributeMutation> ();
                jsonArray.values = operations.ToArray ();
                onApply (jsonArray.ToJson ());
            }
        }

        internal enum AttributeType {
            None,
            Integer,
            Long,
            Float,
            Double,
            String
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
            private string type;
#pragma warning restore

            public AttributeMutation (AttributeAction action, string key, object value, AttributeType type) {
                this.action = action.ToString ();
                this.key = key;
                this.value = value == null ? null : value.ToString ();
                this.type = type.ToString ();
            }
        }
    }
}
