/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship
{

    /// <summary>
    /// Airship Privacy Manager.
    /// </summary>
    public class AirshipPrivacyManager
    {
        private IAirshipPlugin plugin;

        internal AirshipPrivacyManager(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Sets the current set of enabled features.
        /// </summary>
        /// <param name="enabledFeatures">The features to set.</param>
        public void SetEnabledFeatures(string[] enabledFeatures)
        {
            plugin.Call("setEnabledFeatures", enabledFeatures);
        }

        /// <summary>
        /// Gets the current enabled features.
        /// </summary>
        /// <returns>The current enabled features.</returns>
        public string[] GetEnabledFeatures()
        {
            return plugin.Call<string[]>("getEnabledFeatures");
        }

        /// <summary>
        /// Enables additional features.
        /// </summary>
        /// <param name="features">The features to enable.</param>
        public void EnableFeatures(string[] features)
        {
            plugin.Call("enableFeatures", features);
        }

        /// <summary>
        /// Disable features.
        /// </summary>
        /// <param name="features">The features to disable.</param>
        public void DisableFeatures(string[] features)
        {
            plugin.Call("disableFeatures", features);
        }

        /// <summary>
        /// Checks if the features are enabled or not.
        /// </summary>
        /// <param name="features">The features to check</param>
        /// <value><c>true</c> if the features are enabled, otherwise <c>false</c></value>
        public bool IsFeaturesEnabled(string[] features)
        {
            return plugin.Call<bool>("isFeaturesEnabled", features);
        }
    }
}