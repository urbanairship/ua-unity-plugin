/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable annotations

namespace AirshipSDK {

    /// <summary>
    /// Airship channel.
    /// </summary>
    public class AirshipChannel
    {

        private IAirshipPlugin plugin;

        internal AirshipChannel(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Gets the channel ID associated with the device.
        /// </summary>
        /// <returns>The channel ID.</returns>
        public string? GetChannelId()
        {
            return plugin.Call<string?>("getChannelId");
        }

        /// <summary>
        /// Waits for the channel ID asynchronously using a coroutine.
        /// If the channel ID is not yet created, this method will wait for it before completing.
        /// After the channel ID is created, this method functions the same as `getChannelId()`.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="onComplete">Callback invoked with the channel ID when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator WaitForChannelId(Action<string> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => plugin.Call<string>("waitForChannelId"),
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Gets the tags currently set for the device.
        /// </summary>
        /// <returns>The tags.</returns>
        public IEnumerable<string> GetTags()
        {
            string tagsAsJson = plugin.Call<string>("getTags");
            JsonArray<string> jsonArray = JsonArray<string>.FromJson(tagsAsJson);
            return jsonArray.AsEnumerable();
        }

        /// <summary>
        /// Returns an editor for channel tags.
        /// </summary>
        /// <returns>A TagEditor for channel tags.</returns>
        public TagEditor EditTags()
        {
            return new TagEditor((string payload) =>
            {
                plugin.Call("editTags", payload);
            });
        }

        /// <summary>
        /// Returns an editor for channel tag groups.
        /// </summary>
        /// <returns>A TagGroupEditor for channel tag groups.</returns>
        public TagGroupEditor EditTagGroups()
        {
            return new TagGroupEditor((string payload) =>
            {
                plugin.Call("editChannelTagGroups", payload);
            });
        }

        /// <summary>
        /// Returns an editor for channel attributes.
        /// </summary>
        /// <returns>A AttributeEditor for channel attributes.</returns>
        public AttributeEditor EditAttributes()
        {
            return new AttributeEditor((string payload) =>
            {
                plugin.Call("editChannelAttributes", payload);
            });
        }

        /// <summary>
        /// Gets the channel's subscription lists asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="onComplete">Callback invoked with the subscription lists when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator GetSubscriptionLists(Action<IEnumerable<string>> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    string subscriptionListsAsJson = plugin.Call<string>("getChannelSubscriptionLists");
                    if (string.IsNullOrEmpty(subscriptionListsAsJson)) {
                        return Enumerable.Empty<string>();
                    }
                    JsonArray<string> jsonArray = JsonArray<string>.FromJson(subscriptionListsAsJson);
                    return jsonArray.AsEnumerable();
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Returns an editor for channel subscription lists.
        /// </summary>
        /// <returns>A SubscriptionListEditor.</returns>
        public SubscriptionListEditor EditSubscriptionLists()
        {
            return new SubscriptionListEditor((string payload) =>
            {
                plugin.Call("editChannelSubscriptionLists", payload);
            });
        }
    }
}