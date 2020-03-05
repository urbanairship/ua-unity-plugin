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
        	if (IsInvalidField(key)) {
        		return this;
        	}
            operations.Add(AttributeMutation.NewSetAttributeMutation(key, value));
            return this;
        }

        /// <summary>
        /// Sets an integer number attribute.    
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        /// <param name="value">The number attribute.</param>
        public AttributeEditor SetAttribute (string key, int value) {
        	if (IsInvalidField(key)) {
        		return this;
        	}
            operations.Add(AttributeMutation.NewSetAttributeMutation(key, value));
            return this;
        }

        /// <summary>
        /// Sets an long number attribute.    
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        /// <param name="value">The number attribute.</param>
        public AttributeEditor SetAttribute (string key, long value) {
        	if (IsInvalidField(key)) {
        		return this;
        	}
            operations.Add(AttributeMutation.NewSetAttributeMutation(key, value));
            return this;
        }

        /// <summary>
        /// Sets a float number attribute.    
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        /// <param name="value">The number attribute.</param>
        public AttributeEditor SetAttribute (string key, float value) {
        	if (IsInvalidField(key)) {
        		return this;
        	}
            operations.Add(AttributeMutation.NewSetAttributeMutation(key, value));
            return this;
        }

        /// <summary>
        /// Sets a double number attribute.    
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        /// <param name="value">The number attribute.</param>
        public AttributeEditor SetAttribute (string key, double value) {
        	if (IsInvalidField(key)) {
        		return this;
        	}
            operations.Add(AttributeMutation.NewSetAttributeMutation(key, value));
            return this;
        }

        /// <summary>
        /// Removes an attribute.   
        /// </summary>
        /// <returns>The AttributeEditor</returns>
        /// <param name="key">The attribute key greater than one character and less than 1024 characters in length.</param>
        public AttributeEditor RemoveAttribute(string key)
        {
            if (IsInvalidField(key))
            {
                return this;
            }
            operations.Add(AttributeMutation.newRemoveAttributeMutation(key));
            return this;
        }

        private bool IsInvalidField(string key) {
        	if (key == null || key.Length == 0) {
        		//Logger.error("Attribute fields cannot be empty.");
            	return true;
        	}

        	if (key.Length > 1024) {
        		//Logger.error("Attribute field inputs cannot be greater than %s characters in length", 1024);
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

        [Serializable]
        internal class AttributeMutation {
        	private static string ATTRIBUTE_ACTION_REMOVE = "remove";
    		private static string ATTRIBUTE_ACTION_SET = "set";

#pragma warning disable
            // Used for JSON encoding/decoding

            [SerializeField]
            private string action;

            [SerializeField]
            private string key;

            [SerializeField]
            private object value;
#pragma warning restore

            public AttributeMutation (string action, string key, object value) {
                this.action = action;
                this.key = key;
                this.value = value;
            }

            public static AttributeMutation NewSetAttributeMutation(string key, string stringValue) {
        		return new AttributeMutation(ATTRIBUTE_ACTION_SET, key, stringValue);
        	}

        	public static AttributeMutation NewSetAttributeMutation(string key, int number) {
        		return new AttributeMutation(ATTRIBUTE_ACTION_SET, key, number);
        	}

        	public static AttributeMutation NewSetAttributeMutation(string key, long number) {
        		return new AttributeMutation(ATTRIBUTE_ACTION_SET, key, number);
        	}

        	public static AttributeMutation NewSetAttributeMutation(string key, float number) {
				if (float.IsNaN(number) || float.IsInfinity(number)) {
            		throw new FormatException("Infinity or NaN: " + number);
        		}

        		return new AttributeMutation(ATTRIBUTE_ACTION_SET, key, number);
        	}

        	public static AttributeMutation NewSetAttributeMutation(string key, double number) {
        		if (double.IsNaN(number) || double.IsInfinity(number)) {
            		throw new FormatException("Infinity or NaN: " + number);
        		}

        		return new AttributeMutation(ATTRIBUTE_ACTION_SET, key, number);
        	}

            public static AttributeMutation newRemoveAttributeMutation(string key) {
                return new AttributeMutation(ATTRIBUTE_ACTION_REMOVE, key, null);
            }
        }
    }
}