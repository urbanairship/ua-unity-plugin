/* Copyright Airship and Contributors */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {
    /// <summary>
    /// An editor for subscription lists.
    /// </summary>
    public class SubscriptionListEditor {
        private Action<string> onApply;
        private IList<SubscriptionListOperation> operations = new List<SubscriptionListOperation> ();

        internal SubscriptionListEditor (Action<string> onApply) {
            this.onApply = onApply;
        }

        /// <summary>
        /// Subscribes to a list.
        /// </summary>
        /// <returns>The subscription list editor.</returns>
        /// <param name="subscriptionListId">The subscription list identifier.</param>
        public SubscriptionListEditor subscribe (string subscriptionListId) {
            operations.Add (new SubscriptionListOperation ("subscribe", subscriptionListId));
            return this;
        }

        /// <summary>
        /// Unsubscribes from a list.
        /// </summary>
        /// <returns>The subscription list editor.</returns>
        /// <param name="subscriptionListId">The subscription list identifier.</param>
        public SubscriptionListEditor unsubscribe (string subscriptionListId) {
            operations.Add (new SubscriptionListOperation ("unsubscribe", subscriptionListId));
            return this;
        }

        /// <summary>
        /// Applies pending changes.
        /// </summary>
        public void Apply () {
            if (onApply != null) {
                JsonArray<SubscriptionListOperation> jsonArray = new JsonArray<SubscriptionListOperation> ();
                jsonArray.values = operations.ToArray ();
                onApply (jsonArray.ToJson ());
            }
        }

        [Serializable]
        internal class SubscriptionListOperation {
#pragma warning disable
            // Used for JSON encoding/decoding

            [SerializeField]
            private string action;

            [SerializeField]
            private string listId;
#pragma warning restore

            public SubscriptionListOperation (string action, string listId) {
                this.action = action;
                this.listId = listId;
            }
        }
    }
}
