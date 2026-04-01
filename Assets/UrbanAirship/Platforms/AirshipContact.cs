/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {

    /// <summary>
    /// Airship contact.
    /// </summary>
    public class AirshipContact
    {

        private IAirshipPlugin plugin;

        internal AirshipContact(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Identifies the contact with a named user Id.
        /// </summary>
        /// <param name="namedUserId">The named user Id.</param>
        public void Identify(string namedUserId)
        {
            plugin.Call("identify", namedUserId);
        }

        /// <summary>
        /// Resets the contact.
        /// </summary>
        public void Reset()
        {
            plugin.Call("reset");
        }

        /// <summary>
        /// Gets the named user Id asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="onComplete">Callback invoked with the named user Id when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator GetNamedUserId(Action<string> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => plugin.Call<string>("getNamedUserId"),
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Returns an editor for contact tag groups.
        /// </summary>
        /// <returns>A TagGroupEditor for contact tag groups.</returns>
        public TagGroupEditor EditTagGroups()
        {
            return new TagGroupEditor((string payload) =>
            {
                plugin.Call("editContactTagGroups", payload);
            });
        }

        /// <summary>
        /// Returns an editor for contact attributes.
        /// </summary>
        /// <returns>A AttributeEditor for contact attributes.</returns>
        public AttributeEditor EditAttributes()
        {
            return new AttributeEditor((string payload) =>
            {
                plugin.Call("editContactAttributes", payload);
            });
        }

        /// <summary>
        /// Gets the contact's subscription lists asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="onComplete">Callback invoked with the subscription lists when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator GetSubscriptionLists(Action<Dictionary<string, IEnumerable<string>>> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    Dictionary<string, IEnumerable<string>> scopedSubscriptionLists = new Dictionary<string, IEnumerable<string>>();

                    string subscriptionListsAsJson = plugin.Call<string>("getContactSubscriptionLists");
                    ScopedSubscriptionList[] _scopedSubscriptionLists = JsonArray<ScopedSubscriptionList>.FromJson(subscriptionListsAsJson).values;

                    foreach (ScopedSubscriptionList subscriptionList in _scopedSubscriptionLists)
                    {
                        scopedSubscriptionLists.Add(subscriptionList.listId, subscriptionList.scopes.AsEnumerable());
                    }

                    return scopedSubscriptionLists;
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Returns an editor for contact subscription lists.
        /// </summary>
        /// <returns>A SubscriptionListEditor.</returns>
        public ScopedSubscriptionListEditor EditSubscriptionLists()
        {
            return new ScopedSubscriptionListEditor((string payload) =>
            {
                plugin.Call("editContactSubscriptionLists", payload);
            });
        }
    }
}