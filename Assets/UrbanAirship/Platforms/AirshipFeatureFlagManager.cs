/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {

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
            return $"{{ \"isEligible\":{isEligible}, \"exists\":{exists}, \"variables\":{(variables == null ? "\"\"" : variables)}, \"_internal\":{_internal} }}";
        }

        // public FeatureFlag(InternalFeatureFlag internalFeatureFlag)
        // {
        //     isEligible = internalFeatureFlag.isEligible;
        //     exists = internalFeatureFlag.exists;

        //     if (internalFeatureFlag.variableKeys != null && internalFeatureFlag.variableKeys.Count > 0)
        //     {
        //         // Unity's JsonUtility doesn't support embedded dictionaries - create the extras dictionary manually
        //         variables = new Dictionary<string, string>();
        //         for (int index = 0; index < internalFeatureFlag.variableKeys.Count; index++)
        //         {
        //             variables[internalFeatureFlag.variableKeys[index]] = internalFeatureFlag.variableValues[index];
        //         }
        //     }

        //     if (internalFeatureFlag._internalKeys != null && internalFeatureFlag._internalKeys.Count > 0)
        //     {
        //         // Unity's JsonUtility doesn't support embedded dictionaries - create the extras dictionary manually
        //         _internal = new Dictionary<string, string>();
        //         for (int index = 0; index < internalFeatureFlag._internalKeys.Count; index++)
        //         {
        //             _internal[internalFeatureFlag._internalKeys[index]] = internalFeatureFlag._internalValues[index];
        //         }
        //     }
        // }
    }

    // [Serializable]
    // public class InternalFeatureFlag
    // {
    //     public bool isEligible;
    //     public bool exists;
    //     public List<string>? variableKeys;
    //     public List<string>? variableValues;
    //     public List<string> _internalKeys;
    //     public List<string> _internalValues;
    // }
}