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

        [SerializeField]
        private string title;

        [SerializeField]
        private string notificationId;

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
        /// Gets the notification title.
        /// </summary>
        /// <value>The title, or <c>null</c> if the push carried none.</value>
        public string Title {
            get { return this.title; }
        }

        /// <summary>
        /// Gets the identifier of the notification this push posted.
        /// </summary>
        /// <remarks>
        /// This is the platform's notification identifier, not the Airship send ID. It is
        /// <c>null</c> for a push that posted no notification, such as a silent push.
        /// </remarks>
        /// <value>The notification identifier.</value>
        public string NotificationId {
            get { return this.notificationId; }
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

            // Only bail when nothing at all came through, which means the payload was not a
            // push. Checking the alert alone would drop a title-only or data-only push.
            if (pushMessage.Alert == null
                && pushMessage.Title == null
                && pushMessage.NotificationId == null
                && pushMessage.Extras == null) {
                return null;
            }
            return pushMessage;
        }
    }
}
