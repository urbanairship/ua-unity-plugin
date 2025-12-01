/* Copyright Airship and Contributors */

using System;
using System.Collections;
using UnityEngine;

namespace UrbanAirship {

    /// <summary>
    /// Helper class to run blocking operations in coroutines.
    /// AndroidJavaObject.Call() must run on Unity's main thread, so we call it directly
    /// from the coroutine (which runs on the main thread). We yield first to let Unity
    /// process a frame, then execute the blocking call. The blocking happens on the main
    /// thread, but Unity has had a chance to process input/rendering first.
    /// </summary>
    internal static class AirshipCoroutineHelper {
        
        /// <summary>
        /// Runs a blocking operation on the main thread. Yields first to let Unity process,
        /// then executes the blocking call. Since coroutines run on the main thread, this
        /// ensures AndroidJavaObject.Call() works correctly.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="operation">The blocking operation to run</param>
        /// <param name="onComplete">Callback invoked when the operation completes</param>
        /// <param name="onError">Optional callback invoked if an error occurs</param>
        /// <returns>A coroutine</returns>
        public static IEnumerator RunAsync<T>(Func<T> operation, Action<T> onComplete, Action<Exception> onError = null) {
            // Yield first to let Unity process a frame
            yield return null;
            
            T result = default(T);
            Exception exception = null;
            
            try {
                result = operation();
            } catch (Exception e) {
                exception = e;
                Debug.LogError($"[AirshipCoroutineHelper] Exception: {e.Message}\n{e.StackTrace}");
            }
            
            // Yield again before invoking callbacks to ensure we're still on main thread
            yield return null;

            // Handle result or error on main thread
            if (exception != null) {
                onError?.Invoke(exception);
            } else {
                onComplete?.Invoke(result);
            }
        }

        /// <summary>
        /// Runs a blocking operation on the main thread. Yields first to let Unity process,
        /// then executes the blocking call. Since coroutines run on the main thread, this
        /// ensures AndroidJavaObject.Call() works correctly.
        /// </summary>
        /// <param name="operation">The blocking operation to run</param>
        /// <param name="onComplete">Callback invoked when the operation completes</param>
        /// <param name="onError">Optional callback invoked if an error occurs</param>
        /// <returns>A coroutine</returns>
        public static IEnumerator RunAsync(Action operation, Action onComplete = null, Action<Exception> onError = null) {
            // Yield first to let Unity process a frame
            yield return null;
            
            Exception exception = null;
            
            try {
                operation();
            } catch (Exception e) {
                exception = e;
                Debug.LogError($"[AirshipCoroutineHelper] Exception: {e.Message}\n{e.StackTrace}");
            }
            
            // Yield again before invoking callbacks to ensure we're still on main thread
            yield return null;

            // Handle result or error on main thread
            if (exception != null) {
                onError?.Invoke(exception);
            } else {
                onComplete?.Invoke();
            }
        }
    }
}

