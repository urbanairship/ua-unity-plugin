/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace AirshipSDK {

    /// <summary>
    /// Helper class to run blocking operations asynchronously without blocking Unity's main thread.
    /// On Android, JNI calls that use runBlocking in Kotlin will block the calling thread.
    /// This helper runs those operations on a worker thread owned by the plugin,
    /// then returns results to the main thread via callbacks.
    /// </summary>
    internal static class AirshipCoroutineHelper {

        // Maximum time an operation may spend running before surfacing a timeout. The clock
        // starts when a worker picks the operation up, not when it is queued: queue time is
        // budgeted separately below, so a call waiting behind others does not spend the
        // budget meant for its own execution and report a timeout for work that never ran.
        private const float TimeoutSeconds = 60f;

        // Maximum time an operation may sit in the dispatcher queue before a worker takes it.
        // Every queued operation is itself bounded by TimeoutSeconds, so reaching this means
        // the pool is saturated or wedged rather than merely busy.
        private const float QueueTimeoutSeconds = 60f;

        /// <summary>
        /// Runs a blocking operation on a worker thread to avoid ANRs.
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
            bool started = false;

            try {
                AirshipNativeDispatcher.Post(() => {
                    Volatile.Write(ref started, true);
                    try {
                        result = operation();
                    } catch (Exception e) {
                        exception = e;
                    } finally {
                        // Volatile write so the main-thread loop is guaranteed to observe completion.
                        Volatile.Write(ref completed, true);
                    }
                });
            } catch (Exception e) {
                exception = e;
                Volatile.Write(ref completed, true);
            }

            // Wait for completion without blocking the main thread. Two phases: first for a
            // worker to pick the operation up, then for the operation itself to finish.
            float queuedAt = Time.realtimeSinceStartup;
            while (!Volatile.Read(ref started) && !Volatile.Read(ref completed)) {
                if (Time.realtimeSinceStartup - queuedAt > QueueTimeoutSeconds) {
                    onError?.Invoke(new TimeoutException(
                        $"Airship async operation waited {QueueTimeoutSeconds}s for a worker and never started"));
                    yield break;
                }
                yield return null;
            }

            float startedAt = Time.realtimeSinceStartup;
            while (!Volatile.Read(ref completed)) {
                if (Time.realtimeSinceStartup - startedAt > TimeoutSeconds) {
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
        /// Runs a blocking operation on a worker thread to avoid ANRs.
        /// </summary>
        /// <param name="operation">The blocking operation to run</param>
        /// <param name="onComplete">Callback invoked when the operation completes</param>
        /// <param name="onError">Optional callback invoked if an error occurs</param>
        /// <returns>A coroutine</returns>
        public static IEnumerator RunAsync(Action operation, Action onComplete = null, Action<Exception> onError = null) {
            Exception exception = null;
            bool completed = false;
            bool started = false;

            try {
                AirshipNativeDispatcher.Post(() => {
                    Volatile.Write(ref started, true);
                    try {
                        operation();
                    } catch (Exception e) {
                        exception = e;
                    } finally {
                        // Volatile write so the main-thread loop is guaranteed to observe completion.
                        Volatile.Write(ref completed, true);
                    }
                });
            } catch (Exception e) {
                exception = e;
                Volatile.Write(ref completed, true);
            }

            // Wait for completion without blocking the main thread. Two phases: first for a
            // worker to pick the operation up, then for the operation itself to finish.
            float queuedAt = Time.realtimeSinceStartup;
            while (!Volatile.Read(ref started) && !Volatile.Read(ref completed)) {
                if (Time.realtimeSinceStartup - queuedAt > QueueTimeoutSeconds) {
                    onError?.Invoke(new TimeoutException(
                        $"Airship async operation waited {QueueTimeoutSeconds}s for a worker and never started"));
                    yield break;
                }
                yield return null;
            }

            float startedAt = Time.realtimeSinceStartup;
            while (!Volatile.Read(ref completed)) {
                if (Time.realtimeSinceStartup - startedAt > TimeoutSeconds) {
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

    /// <summary>
    /// A small pool of threads owned by the plugin, used to run blocking native calls.
    /// </summary>
    /// <remarks>
    /// These are dedicated threads rather than the shared .NET thread pool because of how
    /// JNI attachment works on Android. Every JNI call must come from a thread attached to
    /// the JVM, and attach/detach is not reference counted, so attaching and detaching a
    /// pool thread around each call means detaching a thread the rest of the app also uses
    /// and may have attached for its own reasons. Each worker here attaches once when it
    /// starts and detaches only when it exits, and no thread outside this class is ever
    /// touched.
    ///
    /// Threads are created on demand up to <see cref="MaxWorkers"/> and then reused. The cap
    /// stops a native call that never returns from spawning threads without bound, while
    /// leaving room for the handful of concurrent calls an app realistically makes.
    /// Concurrent calls are safe: <c>AndroidJavaObject</c> holds a JNI global reference and
    /// resolves method IDs per call, and the framework proxy is itself thread-safe.
    /// </remarks>
    internal static class AirshipNativeDispatcher {

        private const int MaxWorkers = 4;

        private static readonly BlockingCollection<Action> queue = new BlockingCollection<Action>();
        private static readonly object gate = new object();

        private static int workerCount;
        private static int idleWorkers;
        private static bool stopped;

        /// <summary>
        /// Queues an operation to run on a worker thread.
        /// </summary>
        internal static void Post(Action work) {
            lock (gate) {
                if (stopped) {
                    throw new InvalidOperationException("Airship: the native dispatcher has shut down");
                }

                // Compared against the backlog rather than tested for zero. idleWorkers
                // counts workers parked in Take(), so it does not account for work already
                // queued and unclaimed: two Post calls in a row -- the normal case, since
                // StartCoroutine runs the iterator up to its first yield -- would both see
                // the same idle worker and neither would spawn, leaving the second operation
                // queued behind the first even with the pool under its cap.
                //
                // This can still race the other way and start one thread more than strictly
                // needed, which is harmless, and the alternative is holding the lock across
                // the handoff.
                if (queue.Count >= Volatile.Read(ref idleWorkers) && workerCount < MaxWorkers) {
                    workerCount++;
                    new Thread(Work) {
                        Name = "Airship Native Worker " + workerCount,
                        IsBackground = true
                    }.Start();
                }
            }

            queue.Add(work);
        }

#if !UNITY_EDITOR
        // Only registered in a player. In the editor nothing is attached to a JVM, and a
        // static shutdown flag would survive "enter play mode without domain reload" and
        // leave the dispatcher permanently stopped.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterShutdown() {
            Application.quitting += Stop;
        }
#endif

        private static void Stop() {
            lock (gate) {
                if (stopped) {
                    return;
                }
                stopped = true;
            }

            // Lets idle workers fall out of Take() and detach. Workers are background
            // threads, so one still inside a native call cannot hold up process exit.
            queue.CompleteAdding();
        }

        private static void Work() {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (AndroidJNI.AttachCurrentThread() != 0) {
                // Making JNI calls from an unattached thread aborts the process, so give up
                // this worker instead. Post will start a replacement for the next operation.
                Debug.LogError("Airship: failed to attach a worker thread to the JVM");
                lock (gate) {
                    workerCount--;
                }
                return;
            }
#endif
            try {
                while (true) {
                    Action work;
                    Interlocked.Increment(ref idleWorkers);
                    try {
                        work = queue.Take();
                    } catch (InvalidOperationException) {
                        // The queue was completed and drained.
                        break;
                    } finally {
                        Interlocked.Decrement(ref idleWorkers);
                    }

                    try {
                        work();
                    } catch (Exception e) {
                        // Operations handle their own errors; this is only reached if a
                        // callback itself threw, and losing the worker would be worse.
                        Debug.LogException(e);
                    }
                }
            } finally {
#if UNITY_ANDROID && !UNITY_EDITOR
                // ART aborts a thread that exits while still attached.
                AndroidJNI.DetachCurrentThread();
#endif
            }
        }
    }
}
