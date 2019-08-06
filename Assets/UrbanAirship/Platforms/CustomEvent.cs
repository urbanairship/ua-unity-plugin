/* Copyright Airship and Contributors */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {
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

#pragma warning disable
        // Used for JSON encoding/decoding.
        [SerializeField]
        private Property[] properties;
#pragma warning restore

        private List<Property> propertyList;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrbanAirship.CustomEvent"/> class.
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
        /// <value>The event value.</value>
        public decimal EventValue {
            get { return Decimal.Parse (eventValue); }
            set { eventValue = value.ToString (); }
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
            this.properties = this.propertyList.ToArray ();
            return JsonUtility.ToJson (this);
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
        }
    }
}
