/* Copyright Airship and Contributors */

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace AirshipSDK.Tests {

    /// <summary>
    /// Covers the dispatcher that runs every blocking native call.
    /// </summary>
    /// <remarks>
    /// These drive <see cref="AirshipNativeDispatcher"/> directly rather than through
    /// AirshipCoroutineHelper, because the helper's waiting is a coroutine and its budget is
    /// measured in <c>Time.realtimeSinceStartup</c>, neither of which advances inside an
    /// EditMode test. The dispatcher itself touches no Unity API in the editor, so it can be
    /// exercised on its own.
    ///
    /// The dispatcher is process-wide and its worker threads outlive any single test, which
    /// is why nothing here asserts on a worker count -- only on work actually completing.
    /// </remarks>
    [TestFixture]
    public class NativeDispatcherTests {

        // Generous: these only ever wait on work that completes immediately, so the timeout
        // is a guard against a hang rather than a measurement.
        private const int WaitMilliseconds = 10000;

        [Test]
        public void PostRunsTheOperation () {
            using (ManualResetEventSlim ran = new ManualResetEventSlim (false)) {
                AirshipNativeDispatcher.Post (() => ran.Set ());

                Assert.IsTrue (ran.Wait (WaitMilliseconds), "the posted operation never ran");
            }
        }

        [Test]
        public void PostRunsOffTheCallingThread () {
            int callingThread = Thread.CurrentThread.ManagedThreadId;
            int workerThread = callingThread;

            using (ManualResetEventSlim ran = new ManualResetEventSlim (false)) {
                AirshipNativeDispatcher.Post (() => {
                    workerThread = Thread.CurrentThread.ManagedThreadId;
                    ran.Set ();
                });

                Assert.IsTrue (ran.Wait (WaitMilliseconds), "the posted operation never ran");
                Assert.AreNotEqual (callingThread, workerThread,
                    "the operation ran on the calling thread, which is what would block Unity's main thread");
            }
        }

        /// Every queued operation has to reach a worker. Spawning only when no worker is
        /// parked in Take() left back-to-back posts queued behind a single worker, so more
        /// operations than the pool has threads is the case worth pinning down.
        [Test]
        public void EveryPostedOperationRuns () {
            const int count = 12;
            using (CountdownEvent remaining = new CountdownEvent (count)) {
                for (int i = 0; i < count; i++) {
                    AirshipNativeDispatcher.Post (() => remaining.Signal ());
                }

                Assert.IsTrue (remaining.Wait (WaitMilliseconds),
                    $"{remaining.CurrentCount} of {count} operations never ran");
            }
        }

        /// The pool is capped, so a slow operation necessarily delays something. What must
        /// not happen is one slow operation stranding work while the pool is under its cap.
        [Test]
        public void ASlowOperationDoesNotStrandTheNextOne () {
            using (ManualResetEventSlim release = new ManualResetEventSlim (false))
            using (ManualResetEventSlim fastRan = new ManualResetEventSlim (false)) {
                AirshipNativeDispatcher.Post (() => release.Wait (WaitMilliseconds));
                AirshipNativeDispatcher.Post (() => fastRan.Set ());

                try {
                    Assert.IsTrue (fastRan.Wait (WaitMilliseconds),
                        "the second operation was stranded behind the first");
                } finally {
                    release.Set ();
                }
            }
        }

        [Test]
        public void ConcurrentPostersAllGetTheirWork () {
            const int posters = 4;
            const int perPoster = 5;

            using (CountdownEvent remaining = new CountdownEvent (posters * perPoster)) {
                List<Thread> threads = new List<Thread> ();

                for (int p = 0; p < posters; p++) {
                    Thread thread = new Thread (() => {
                        for (int i = 0; i < perPoster; i++) {
                            AirshipNativeDispatcher.Post (() => remaining.Signal ());
                        }
                    });
                    threads.Add (thread);
                    thread.Start ();
                }

                foreach (Thread thread in threads) {
                    thread.Join (WaitMilliseconds);
                }

                Assert.IsTrue (remaining.Wait (WaitMilliseconds),
                    $"{remaining.CurrentCount} operations never ran");
            }
        }

        /// An operation that throws is the dispatcher's own business: it logs and keeps the
        /// worker, because losing a thread from a four-thread pool is worse than the error.
        [Test]
        public void AThrowingOperationDoesNotKillTheWorker () {
            UnityEngine.TestTools.LogAssert.Expect (UnityEngine.LogType.Exception,
                new System.Text.RegularExpressions.Regex ("deliberate"));

            AirshipNativeDispatcher.Post (() => throw new InvalidOperationException ("deliberate"));

            using (ManualResetEventSlim ran = new ManualResetEventSlim (false)) {
                AirshipNativeDispatcher.Post (() => ran.Set ());

                Assert.IsTrue (ran.Wait (WaitMilliseconds),
                    "no worker picked up the next operation after one threw");
            }
        }
    }
}
