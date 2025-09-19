/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship
{

    /// <summary>
    /// Airship Locale. Manages locale used by Airship messaging.
    /// </summary>
    public class AirshipLocale
    {
        private IAirshipPlugin plugin;

        internal AirshipLocale(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Sets the locale override.
        /// </summary>
        /// <param name="localeIdentifier">The locale identifier.</param>
        public void SetLocaleOverride(string localeIdentifier)
        {
            plugin.Call("setLocaleOverride", localeIdentifier);
        }

        /// <summary>
        /// Clears the locale override.
        /// </summary>
        public void ClearLocaleOverride()
        {
            plugin.Call("clearLocaleOverride");
        }

        /// <summary>
        /// Gets the current locale.
        /// </summary>
        /// <returns>The current locale.</returns>
        public string GetLocale()
        {
            return plugin.Call<string>("getLocale");
        }
    }
}