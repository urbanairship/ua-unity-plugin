/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AirshipSDK {

    public interface IAirshipLiveUpdateManager
    {
        IEnumerator List(LiveUpdateListRequest request, Action<LiveUpdate[]> onComplete, Action<Exception> onError = null);
        IEnumerator ListAll(Action<LiveUpdate[]> onComplete, Action<Exception> onError = null);
        void Start(LiveUpdateStartRequest request);
        void Update(LiveUpdateUpdateRequest request);
        void End(LiveUpdateEndRequest request);
        void ClearAll();
    }

    internal class StubbedAirshipLiveUpdateManager : IAirshipLiveUpdateManager
    {
        public IEnumerator List(LiveUpdateListRequest request, Action<LiveUpdate[]> onComplete, Action<Exception> onError = null) { yield break; }
        public IEnumerator ListAll(Action<LiveUpdate[]> onComplete, Action<Exception> onError = null) { yield break; }
        public void Start(LiveUpdateStartRequest request) {}
        public void Update(LiveUpdateUpdateRequest request) {}
        public void End(LiveUpdateEndRequest request) {}
        public void ClearAll() {}
    }

    /// <summary>
    /// Airship Live Update manager. Android only.
    /// </summary>
    public class AirshipLiveUpdateManager : IAirshipLiveUpdateManager
    {
        private IAirshipPlugin plugin;

        internal AirshipLiveUpdateManager(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Lists Live Updates matching the given type.
        /// </summary>
        /// <param name="request">The list request containing the type filter.</param>
        /// <param name="onComplete">Callback invoked with the matching Live Updates.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator List(LiveUpdateListRequest request, Action<LiveUpdate[]> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    string json = plugin.Call<string>("liveUpdateList", request);
                    return AirshipUtils.Deserialize<LiveUpdate[]>(json);
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Lists all Live Updates.
        /// </summary>
        /// <param name="onComplete">Callback invoked with all Live Updates.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator ListAll(Action<LiveUpdate[]> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    string json = plugin.Call<string>("liveUpdateListAll");
                    return AirshipUtils.Deserialize<LiveUpdate[]>(json);
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Starts a new Live Update.
        /// </summary>
        /// <param name="request">The start request.</param>
        public void Start(LiveUpdateStartRequest request)
        {
            plugin.Call("liveUpdateStart", request);
        }

        /// <summary>
        /// Updates an existing Live Update.
        /// </summary>
        /// <param name="request">The update request.</param>
        public void Update(LiveUpdateUpdateRequest request)
        {
            plugin.Call("liveUpdateUpdate", request);
        }

        /// <summary>
        /// Ends a Live Update.
        /// </summary>
        /// <param name="request">The end request.</param>
        public void End(LiveUpdateEndRequest request)
        {
            plugin.Call("liveUpdateEnd", request);
        }

        /// <summary>
        /// Clears all Live Updates.
        /// </summary>
        public void ClearAll()
        {
            plugin.Call("liveUpdateClearAll");
        }
    }

    [Serializable]
    public record LiveUpdate
    {
        public string name;
        public string type;

        // The proxy sends `content` as an arbitrary caller-defined JSON object. Unity's
        // JsonUtility has no dictionary support, so the native layer splits it into two
        // parallel arrays before sending it over. Same approach as PushMessage extras.
        [SerializeField]
        private List<string> contentKeys;
        [SerializeField]
        private List<string> contentValues;

        /// <summary>ISO-8601 timestamp of the last content update.</summary>
        public string lastContentUpdateTimestamp;

        /// <summary>ISO-8601 timestamp of the last state change.</summary>
        public string lastStateChangeTimestamp;

        /// <summary>ISO-8601 dismissal timestamp, or <c>null</c> if none is set.</summary>
        public string dismissTimestamp;

        /// <summary>
        /// Gets the live update content.
        /// </summary>
        /// <remarks>Non-string values are encoded as JSON strings.</remarks>
        /// <value>The content, or <c>null</c> if the update carried none.</value>
        public Dictionary<string, string> Content
        {
            get { return AirshipUtils.PairFlattenedObject(contentKeys, contentValues); }
        }
    }

    [Serializable]
    public record LiveUpdateListRequest
    {
        public string type;
    }

    // Outbound requests spell the dismissal timestamp `dismissalTimestamp`: that is the key
    // LiveUpdateRequest.fromJson reads on the proxy side. Inbound LiveUpdate keeps
    // `dismissTimestamp`, which is what LiveUpdateProxy emits. The asymmetry is the proxy's,
    // not a typo -- matching it is what makes the value survive the round trip.

    [Serializable]
    public record LiveUpdateStartRequest
    {
        public string name;
        public string type;
        public Dictionary<string, object> content;
        public string timestamp;
        public string dismissalTimestamp;
    }

    [Serializable]
    public record LiveUpdateUpdateRequest
    {
        public string name;
        public Dictionary<string, object> content;
        public string timestamp;
        public string dismissalTimestamp;
    }

    [Serializable]
    public record LiveUpdateEndRequest
    {
        public string name;
        public Dictionary<string, object> content;
        public string timestamp;
        public string dismissalTimestamp;
    }
}
