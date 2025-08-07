/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {

    /// <summary>
    /// Airship analytics.
    /// </summary>
    public class AirshipAnalytics
    {

        private IAirshipPlugin plugin;

        internal AirshipAnalytics(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Associate a custom identifier.
        /// Previous identifiers will be replaced by the new identifiers each time AssociateIdentifier is called.
        /// It is a set operation.
        /// </summary>
        /// <param name="key">The custom key for the identifier.</param>
        /// <param name="identifier">The value of the identifier, or `null` to remove the identifier.</param>
        public void AssociateIdentifier(string key, string? identifier)
        {
            plugin.Call("associateIdentifier", key, identifier);
        }

        /// <summary>
        /// Tracks a screen.
        /// </summary>
        /// <param name="screenName">The screen name. `null` to stop tracking.</param>
        public void TrackScreen(string screenName)
        {
            plugin.Call("trackScreen", screenName);
        }

        /// <summary>
        /// Adds a custom event.
        /// </summary>
        /// <param name="customEvent">The custom event.</param>
        public void AddCustomEvent(CustomEvent customEvent)
        {
            plugin.Call("addCustomEvent", customEvent.ToJson());
        }

        /// <summary>
        /// Gets the Airship session ID. The session ID is a UUID that updates on foreground and background.
        /// </summary>
        /// <returns>The session ID.</returns>
        public string GetSessionId()
        {
            return plugin.Call<string>("getSessionId");
        }
    }
}