/* Copyright Airship and Contributors */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AirshipSDK {
    /// <summary>
    /// A Custom Event model object.
    /// </summary>
    [System.Serializable]
    public class CustomEvent {
        [SerializeField]
        private string eventName;
        [SerializeField]
        private string eventValue;
        [SerializeField]
        private string transactionId;
        [SerializeField]
        private string interactionType;
        [SerializeField]
        private string interactionId;

        private List<Property> propertyList;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEvent"/> class.
        /// </summary>
        public CustomEvent () {
            this.propertyList = new List<Property> ();
        }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName {
            get { return eventName; }
            set { eventName = value; }
        }

        /// <summary>
        /// Gets or sets the event value.
        /// </summary>
        /// <remarks>
        /// Reads as 0 on an event that was never given a value. The getter used to parse the
        /// backing field unconditionally, which threw on such an event. An unset value and a
        /// value of 0 are indistinguishable here; <c>ToJson</c> tells them apart from the
        /// backing field and omits the key entirely when nothing was set.
        /// </remarks>
        /// <value>The event value, or 0 if the event carries none.</value>
        public decimal EventValue {
            get {
                if (string.IsNullOrEmpty (eventValue)) {
                    return 0m;
                }
                return Decimal.Parse (eventValue, CultureInfo.InvariantCulture);
            }
            set { eventValue = value.ToString (CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>The transaction identifier.</value>
        public string TransactionId {
            get { return transactionId; }
            set { transactionId = value; }
        }

        /// <summary>
        /// Gets or sets interaction type.
        /// </summary>
        /// <value>The interaction type.</value>
        public string InteractionType {
            get { return interactionType; }
            set { interactionType = value; }
        }

        /// <summary>
        /// Gets or sets the interaction identifier.
        /// </summary>
        /// <value>The interaction identifier.</value>
        public string InteractionId {
            get { return interactionId; }
            set { interactionId = value; }
        }

        /// <summary>
        /// Adds a string property.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        public void AddProperty (string name, string value) {
            this.propertyList.Add (new Property ("s", name, value));
        }

        /// <summary>
        /// Adds a double property.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        public void AddProperty (string name, double value) {
            if (double.IsNaN (value) || double.IsInfinity (value)) {
                throw new FormatException ("Infinity or NaN: " + value);
            }
            this.propertyList.Add (new Property ("d", name, value));
        }

        /// <summary>
        /// Adds a bool property.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        public void AddProperty (string name, bool value) {
            this.propertyList.Add (new Property ("b", name, value));
        }

        /// <summary>
        /// Adds a string array property.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        public void AddProperty (string name, ICollection<string> value) {
            this.propertyList.Add (new Property ("sa", name, value));
        }

        internal string ToJson () {
            // Built by hand rather than with JsonUtility: the framework proxy reads
            // `eventValue` only when it is a JSON number and `properties` only when it is a
            // JSON object, and JsonUtility can produce neither (it emits the value as a
            // string and has no dictionary support).
            var sb = new StringBuilder ();
            sb.Append ("{");
            sb.Append ("\"eventName\":").Append (AirshipUtils.ToJsonString (eventName));

            if (!string.IsNullOrEmpty (eventValue)) {
                // Always a round-trippable invariant number: the only way to set this is
                // the decimal EventValue property.
                sb.Append (",\"eventValue\":").Append (eventValue);
            }
            if (!string.IsNullOrEmpty (transactionId)) {
                sb.Append (",\"transactionId\":").Append (AirshipUtils.ToJsonString (transactionId));
            }
            if (!string.IsNullOrEmpty (interactionType)) {
                sb.Append (",\"interactionType\":").Append (AirshipUtils.ToJsonString (interactionType));
            }
            if (!string.IsNullOrEmpty (interactionId)) {
                sb.Append (",\"interactionId\":").Append (AirshipUtils.ToJsonString (interactionId));
            }

            if (propertyList != null && propertyList.Count > 0) {
                sb.Append (",\"properties\":{");
                for (int i = 0; i < propertyList.Count; i++) {
                    if (i > 0) {
                        sb.Append (",");
                    }
                    Property property = propertyList[i];
                    sb.Append (AirshipUtils.ToJsonString (property.name));
                    sb.Append (":");
                    sb.Append (property.ValueToJson ());
                }
                sb.Append ("}");
            }

            sb.Append ("}");
            return sb.ToString ();
        }

        [Serializable]
        class Property {
            public string type;
            public string name;
            public string stringValue;
            public double doubleValue;
            public bool boolValue;
            public string[] stringArrayValue;

            public Property (string type, string name, System.Object value) {
                this.type = type;
                this.name = name;

                if (type == "s") {
                    this.stringValue = (string) value;
                } else if (type == "d") {
                    this.doubleValue = (double) value;
                } else if (type == "b") {
                    this.boolValue = (bool) value;
                } else if (type == "sa") {
                    ICollection<string> collection = (ICollection<string>) value;
                    this.stringArrayValue = collection.ToArray ();
                }
            }

            /// Renders this property as its JSON value for the proxy's properties map.
            public string ValueToJson () {
                switch (type) {
                    case "s":
                        return AirshipUtils.ToJsonString (stringValue);
                    case "d":
                        return doubleValue.ToString ("R", CultureInfo.InvariantCulture);
                    case "b":
                        return boolValue ? "true" : "false";
                    case "sa":
                        var sb = new StringBuilder ();
                        sb.Append ("[");
                        if (stringArrayValue != null) {
                            for (int i = 0; i < stringArrayValue.Length; i++) {
                                if (i > 0) {
                                    sb.Append (",");
                                }
                                sb.Append (AirshipUtils.ToJsonString (stringArrayValue[i]));
                            }
                        }
                        sb.Append ("]");
                        return sb.ToString ();
                    default:
                        return "null";
                }
            }
        }
    }
}
