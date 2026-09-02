/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

#nullable enable annotations

namespace AirshipSDK {

    /// <summary>
    /// Airship feature flag manager.
    /// </summary>
    public class AirshipFeatureFlagManager
    {

        private IAirshipPlugin plugin;

        internal AirshipFeatureFlagManager(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Retrieve a given flag's status and associated data by its name asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="name">The name of the flag.</param>
        /// <param name="onComplete">Callback invoked with the feature flag when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator Flag(string name, Action<FeatureFlag> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                        string flagJson = plugin.Call<string>("flag", name);
                        if (string.IsNullOrEmpty(flagJson))
                        {
                            throw new Exception("Airship: empty response from flag");
                        }
                        FeatureFlag flag = JsonUtility.FromJson<FeatureFlag>(flagJson);
                        return flag;
                    },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Tracks a feature flag interaction event.
        /// </summary>
        /// <param name="flag">The flag.</param>
        public void TrackInteraction(FeatureFlag flag)
        {
            plugin.Call("trackInteraction", flag.ToJson());
        }
    }

    [Serializable]
    public record FeatureFlag
    {
        public bool isEligible;
        public bool exists;
        
        // Stored as JSON strings (serialized on Kotlin/Swift side)
        public string? variables;
        public string _internal;

        public string ToJson()
        {
            //return $"{{ \"isEligible\":{isEligible}, \"exists\":{exists}, \"variables\":{(variables == null ? "\"\"" : variables)}, \"_internal\":{_internal} }}";
            var sb = new StringBuilder();
            sb.Append("{");

            sb.Append($"\"isEligible\":{(isEligible ? "true" : "false")}");
            sb.Append($",\"exists\":{(exists ? "true" : "false")}");

            if (!string.IsNullOrEmpty(variables))
            {
                sb.Append($",\"variables\":{variables}");
            }
            if (!string.IsNullOrEmpty(_internal))
            {
                sb.Append($",\"_internal\":{_internal}");
            }
            
            sb.Append("}");
            return sb.ToString();
        }

    }
}