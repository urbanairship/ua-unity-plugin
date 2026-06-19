/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AirshipSDK {

    /// <summary>
    /// Helper class to run blocking operations asynchronously without blocking Unity's main thread.
    /// On Android, JNI calls that use runBlocking in Kotlin will block the calling thread.
    /// This helper runs those operations on a background thread with proper JNI thread attachment,
    /// then returns results to the main thread via callbacks.
    /// </summary>
    internal static class AirshipCoroutineHelper {

        // Maximum time to wait for a background operation before surfacing a timeout.
        private const float TimeoutSeconds = 60f;
        
        /// <summary>
        /// Runs a blocking operation on a background thread to avoid ANRs.
        /// On Android, attaches/detaches the thread to/from the JVM.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="operation">The blocking operation to run</param>
        /// <param name="onComplete">Callback invoked when the operation completes</param>
        /// <param name="onError">Optional callback invoked if an error occurs</param>
        /// <returns>A coroutine</returns>
        public static IEnumerator RunAsync<T>(Func<T> operation, Action<T> onComplete, Action<Exception> onError = null) {
            T result = default(T);
            Exception exception = null;
            bool completed = false;
            
            // Run the blocking operation on a background thread
            Task.Run(() => {
                try {
#if UNITY_ANDROID && !UNITY_EDITOR
                    // Attach this thread to the JVM for JNI calls
                    AndroidJNI.AttachCurrentThread();
                    try {
                        result = operation();
                    } finally {
                        AndroidJNI.DetachCurrentThread();
                    }
#else
                    result = operation();
#endif
                } catch (Exception e) {
                    exception = e;
                } finally {
                    // Volatile write so the main-thread loop is guaranteed to observe completion.
                    Volatile.Write(ref completed, true);
                }
            });
            
            // Wait for completion without blocking the main thread
            float startTime = Time.realtimeSinceStartup;
            while (!Volatile.Read(ref completed)) {
                if (Time.realtimeSinceStartup - startTime > TimeoutSeconds) {
                    onError?.Invoke(new TimeoutException($"Airship async operation timed out after {TimeoutSeconds}s"));
                    yield break;
                }
                yield return null;
            }

            // Handle result or error on main thread
            if (exception != null) {
                onError?.Invoke(exception);
            } else {
                onComplete?.Invoke(result);
            }
        }

        /// <summary>
        /// Runs a blocking operation on a background thread to avoid ANRs.
        /// On Android, attaches/detaches the thread to/from the JVM.
        /// </summary>
        /// <param name="operation">The blocking operation to run</param>
        /// <param name="onComplete">Callback invoked when the operation completes</param>
        /// <param name="onError">Optional callback invoked if an error occurs</param>
        /// <returns>A coroutine</returns>
        public static IEnumerator RunAsync(Action operation, Action onComplete = null, Action<Exception> onError = null) {
            Exception exception = null;
            bool completed = false;
            
            // Run the blocking operation on a background thread
            Task.Run(() => {
                try {
#if UNITY_ANDROID && !UNITY_EDITOR
                    // Attach this thread to the JVM for JNI calls
                    AndroidJNI.AttachCurrentThread();
                    try {
                        operation();
                    } finally {
                        AndroidJNI.DetachCurrentThread();
                    }
#else
                    operation();
#endif
                } catch (Exception e) {
                    exception = e;
                } finally {
                    // Volatile write so the main-thread loop is guaranteed to observe completion.
                    Volatile.Write(ref completed, true);
                }
            });
            
            // Wait for completion without blocking the main thread
            float startTime = Time.realtimeSinceStartup;
            while (!Volatile.Read(ref completed)) {
                if (Time.realtimeSinceStartup - startTime > TimeoutSeconds) {
                    onError?.Invoke(new TimeoutException($"Airship async operation timed out after {TimeoutSeconds}s"));
                    yield break;
                }
                yield return null;
            }

            // Handle result or error on main thread
            if (exception != null) {
                onError?.Invoke(exception);
            } else {
                onComplete?.Invoke();
            }
        }
    }
}
