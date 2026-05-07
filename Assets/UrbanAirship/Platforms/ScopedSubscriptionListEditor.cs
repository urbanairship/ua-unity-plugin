/* Copyright Airship and Contributors */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AirshipSDK {
    /// <summary>
    /// An editor for subscription lists.
    /// </summary>
    public class ScopedSubscriptionListEditor {
        private Action<string> onApply;
        private IList<ScopedSubscriptionListOperation> operations = new List<ScopedSubscriptionListOperation> ();

        internal ScopedSubscriptionListEditor (Action<string> onApply) {
            this.onApply = onApply;
        }

        /// <summary>
        /// Subscribes to a list.
        /// </summary>
        /// <returns>The subscription list editor.</returns>
        /// <param name="subscriptionListId">The subscription list identifier.</param>
        /// <param name="subscriptionScope">The subscription scope to unsubscribe.</param>
        public ScopedSubscriptionListEditor Subscribe (string subscriptionListId, string subscriptionScope) {
            operations.Add (new ScopedSubscriptionListOperation ("subscribe", subscriptionListId, subscriptionScope));
            return this;
        }

        /// <summary>
        /// Unsubscribes from a list.
        /// </summary>
        /// <returns>The subscription list editor.</returns>
        /// <param name="subscriptionListId">The subscription list identifier.</param>
        /// <param name="subscriptionScope">The subscription scope to unsubscribe.</param>
        public ScopedSubscriptionListEditor Unsubscribe (string subscriptionListId, string subscriptionScope) {
            operations.Add (new ScopedSubscriptionListOperation ("unsubscribe", subscriptionListId, subscriptionScope));
            return this;
        }

        /// <summary>
        /// Applies pending changes.
        /// </summary>
        public void Apply () {
            if (onApply != null) {
                JsonArray<ScopedSubscriptionListOperation> jsonArray = new JsonArray<ScopedSubscriptionListOperation> ();
                jsonArray.values = operations.ToArray ();
                onApply (jsonArray.ToJson ());
            }
        }

        [Serializable]
        internal class ScopedSubscriptionListOperation {
#pragma warning disable
            // Used for JSON encoding/decoding

            [SerializeField]
            private string action;

            [SerializeField]
            private string listId;

            [SerializeField]
            private string scope;
#pragma warning restore

            public ScopedSubscriptionListOperation (string action, string listId, string scope) {
                this.action = action;
                this.listId = listId;
                this.scope = scope;
            }
        }
    }

    /// <summary>
    /// Subscription Scope types.
    /// </summary>
    public static class SubscriptionScope {
        public const string APP = "app";
        public const string WEB = "web";
        public const string SMS = "sms";
        public const string EMAIL = "email";
    }

    /// <summary>
    /// Scoped Subscription list.
    /// </summary>
    [Serializable]
    internal class ScopedSubscriptionList {
        public string listId;
        public List<string> scopes;
    }
}
