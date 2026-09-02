/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable annotations

namespace AirshipSDK {

    /// <summary>
    /// Airship action.
    /// </summary>
    public class AirshipAction
    {

        private IAirshipPlugin plugin;

        internal AirshipAction(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Runs an Airship action asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="name">The name of the action to run.</param>
        /// <param name="value">The action's value.</param>
        /// <param name="onComplete">Callback invoked with the action result when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator RunAction(string name, string? value, Action<string> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => plugin.Call<string>("runAction", name, value),
                onComplete,
                onError
            );
        }
    }
}