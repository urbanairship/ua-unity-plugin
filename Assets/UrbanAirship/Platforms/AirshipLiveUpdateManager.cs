/* Copyright Airship and Contributors */

using System;
using System.Collections;
using UnityEngine;

namespace AirshipSDK {

    /// <summary>
    /// Airship Live Update manager.
    /// </summary>
    public class AirshipLiveUpdateManager
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
                    string json = plugin.Call<string>("liveUpdateList", AirshipUtils.Serialize(request));
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
            plugin.Call("liveUpdateStart", AirshipUtils.Serialize(request));
        }

        /// <summary>
        /// Updates an existing Live Update.
        /// </summary>
        /// <param name="request">The update request.</param>
        public void Update(LiveUpdateUpdateRequest request)
        {
            plugin.Call("liveUpdateUpdate", AirshipUtils.Serialize(request));
        }

        /// <summary>
        /// Ends a Live Update.
        /// </summary>
        /// <param name="request">The end request.</param>
        public void End(LiveUpdateEndRequest request)
        {
            plugin.Call("liveUpdateEnd", AirshipUtils.Serialize(request));
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
        public string content;
        public string lastContentUpdateTimestamp;
        public string lastStateChangeTimestamp;
        public string dismissTimestamp;
    }

    [Serializable]
    public record LiveUpdateListRequest
    {
        public string type;
    }

    [Serializable]
    public record LiveUpdateStartRequest
    {
        public string name;
        public string type;
        public string content;
        public string timestamp;
        public string dismissTimestamp;
    }

    [Serializable]
    public record LiveUpdateUpdateRequest
    {
        public string name;
        public string content;
        public string timestamp;
        public string dismissTimestamp;
    }

    [Serializable]
    public record LiveUpdateEndRequest
    {
        public string name;
        public string content;
        public string timestamp;
        public string dismissTimestamp;
    }
}
