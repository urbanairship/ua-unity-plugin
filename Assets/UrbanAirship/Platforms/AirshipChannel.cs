/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {

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
        /// Gets the channel ID associated with the device.
        /// </summary>
        /// <returns>The channel ID.</returns>
        public string? GetChannelId()
        {
            return plugin.Call<string?>("getChannelId");
        }

        /// <summary>
        /// Returns the channel ID. If the channel ID is not yet created the function it will wait for it before returning.
        /// After the channel ID is created, this method functions the same as `getChannelId()`.
        /// </summary>
        /// <returns>The channel ID.</returns>
        public string WaitForChannelId()
        {
            return plugin.Call<string>("waitForChannelId");
        }

        /// <summary>
        /// Gets the channel's subscription lists.
        /// </summary>
        /// <returns>The subscription lists.</returns>
        public IEnumerable<string> GetSubscriptionLists()
        {
            string subscriptionListsAsJson = plugin.Call<string>("getSubscriptionLists");
            JsonArray<string> jsonArray = JsonArray<string>.FromJson(subscriptionListsAsJson);
            return jsonArray.AsEnumerable();
        }

        /// <summary>
        /// Returns an editor for channel subscription lists.
        /// </summary>
        /// <returns>A SubscriptionListEditor.</returns>
        public SubscriptionListEditor EditSubscriptionLists()
        {
            return new SubscriptionListEditor((string payload) =>
            {
                plugin.Call("editSubscriptionLists", payload);
            });
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
                plugin.Call("editTagGroups", payload);
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
                plugin.Call("editAttributes", payload);
            });
        }
    }
}