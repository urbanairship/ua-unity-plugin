/* Copyright Airship and Contributors */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AirshipSDK {
    /// <summary>
    /// A push message model object.
    /// </summary>
    [Serializable]
    public class PushMessage {
        [SerializeField]
        private string alert;

        // The Airship send ID is no longer available: the framework proxy exposes no
        // send-ID field, and on iOS ProxyPushPayload strips the "_" key from extras.
        // Left unpopulated pending a decision to remove or redefine this property.
        [SerializeField]
        private string identifier;

        // Unity's JsonUtility has no dictionary support, so the native layers split the
        // proxy's `extras` object into two parallel arrays before sending it over. Same
        // approach as InternalInboxMessage.
        [SerializeField]
        private List<string> extrasKeys;
        [SerializeField]
        private List<string> extrasValues;

        private Dictionary<string, string> extrasDictionary;

        /// <summary>
        /// Gets the alert text.
        /// </summary>
        /// <value>The alert text.</value>
        public string Alert {
            get { return this.alert; }
        }

        /// <summary>
        /// Gets the push identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Identifier {
            get { return this.identifier; }
        }

        /// <summary>
        /// Gets the key value extras sent with the push.
        /// </summary>
        /// <remarks>Non-string extra values are encoded as JSON strings.</remarks>
        /// <value>The extras, or <c>null</c> if the push carried none.</value>
        public Dictionary<string, string> Extras {
            get {
                if (extrasKeys == null || extrasKeys.Count == 0) {
                    return null;
                }

                if (this.extrasDictionary == null) {
                    this.extrasDictionary = new Dictionary<string, string> ();
                    int count = extrasValues == null
                        ? 0
                        : Math.Min (extrasKeys.Count, extrasValues.Count);
                    for (int i = 0; i < count; i++) {
                        string key = extrasKeys[i];
                        if (key != null) {
                            this.extrasDictionary[key] = extrasValues[i];
                        }
                    }
                }

                return this.extrasDictionary;
            }
        }

        internal static PushMessage FromJson (string jsonString) {
            if (string.IsNullOrEmpty (jsonString) || jsonString == "null") {
                return null;
            }

            PushMessage pushMessage;
            try {
                pushMessage = JsonUtility.FromJson<PushMessage> (jsonString);
            } catch (Exception e) {
                Debug.LogError ("Airship: unable to parse push message: " + e.Message);
                return null;
            }

            if (pushMessage == null) {
                return null;
            }

            if (pushMessage.Alert == null && pushMessage.Identifier == null && pushMessage.Extras == null) {
                return null;
            }
            return pushMessage;
        }
    }
}
