/* Copyright Airship and Contributors */

using System;
using System.Collections;
using UnityEngine;

namespace AirshipSDK {

    public interface IAirshipLiveActivityManager
    {
        IEnumerator List(LiveActivityListRequest request, Action<LiveActivityInfo[]> onComplete, Action<Exception> onError = null);
        IEnumerator ListAll(Action<LiveActivityInfo[]> onComplete, Action<Exception> onError = null);
        IEnumerator Start(LiveActivityStartRequest request, Action<LiveActivityInfo> onComplete, Action<Exception> onError = null);
        IEnumerator Update(LiveActivityUpdateRequest request, Action onComplete = null, Action<Exception> onError = null);
        IEnumerator End(LiveActivityEndRequest request, Action onComplete = null, Action<Exception> onError = null);
    }

    internal class StubbedAirshipLiveActivityManager : IAirshipLiveActivityManager
    {
        public IEnumerator List(LiveActivityListRequest request, Action<LiveActivityInfo[]> onComplete, Action<Exception> onError = null) { yield break; }
        public IEnumerator ListAll(Action<LiveActivityInfo[]> onComplete, Action<Exception> onError = null) { yield break; }
        public IEnumerator Start(LiveActivityStartRequest request, Action<LiveActivityInfo> onComplete, Action<Exception> onError = null) { yield break; }
        public IEnumerator Update(LiveActivityUpdateRequest request, Action onComplete = null, Action<Exception> onError = null) { yield break; }
        public IEnumerator End(LiveActivityEndRequest request, Action onComplete = null, Action<Exception> onError = null) { yield break; }
    }

    /// <summary>
    /// Airship Live Activity manager. iOS only.
    /// </summary>
    public class AirshipLiveActivityManager : IAirshipLiveActivityManager
    {
        private IAirshipPlugin plugin;

        internal AirshipLiveActivityManager(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Lists Live Activities matching the given attributes type.
        /// </summary>
        /// <param name="request">The list request containing the attributes type filter.</param>
        /// <param name="onComplete">Callback invoked with the matching Live Activities.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator List(LiveActivityListRequest request, Action<LiveActivityInfo[]> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    string json = plugin.Call<string>("liveActivityList", request);
                    return AirshipUtils.Deserialize<LiveActivityInfo[]>(json);
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Lists all Live Activities.
        /// </summary>
        /// <param name="onComplete">Callback invoked with all Live Activities.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator ListAll(Action<LiveActivityInfo[]> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    string json = plugin.Call<string>("liveActivityListAll");
                    return AirshipUtils.Deserialize<LiveActivityInfo[]>(json);
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Starts a new Live Activity.
        /// </summary>
        /// <param name="request">The start request.</param>
        /// <param name="onComplete">Callback invoked with the created Live Activity.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator Start(LiveActivityStartRequest request, Action<LiveActivityInfo> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    string json = plugin.Call<string>("liveActivityStart", request);
                    return AirshipUtils.Deserialize<LiveActivityInfo>(json);
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Updates a Live Activity.
        /// </summary>
        /// <param name="request">The update request.</param>
        /// <param name="onComplete">Optional callback invoked when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator Update(LiveActivityUpdateRequest request, Action onComplete = null, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => { plugin.Call("liveActivityUpdate", request); },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Ends a Live Activity.
        /// </summary>
        /// <param name="request">The end request.</param>
        /// <param name="onComplete">Optional callback invoked when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator End(LiveActivityEndRequest request, Action onComplete = null, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => { plugin.Call("liveActivityEnd", request); },
                onComplete,
                onError
            );
        }
    }

    [Serializable]
    public record LiveActivityInfo
    {
        public string id;
        public string attributesType;
        public string state;
        public LiveActivityContent content;
        public string attributes;
    }

    [Serializable]
    public record LiveActivityContent
    {
        public string state;
        public string staleDate;
        public double relevanceScore;
    }

    [Serializable]
    public record LiveActivityListRequest
    {
        public string attributesType;
    }

    [Serializable]
    public record LiveActivityStartRequest
    {
        public string attributesType;
        public LiveActivityContent content;
        public string attributes;
    }

    [Serializable]
    public record LiveActivityUpdateRequest
    {
        public string attributesType;
        public string activityId;
        public LiveActivityContent content;
    }

    [Serializable]
    public record LiveActivityEndRequest
    {
        public string attributesType;
        public string activityId;
        public LiveActivityContent content;
        public LiveActivityDismissalPolicy dismissalPolicy;
    }

    [Serializable]
    public record LiveActivityDismissalPolicy
    {
        public string type;
        public string date;
    }
}
