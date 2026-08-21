/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
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

        /// <summary>
        /// The activity state: <c>active</c>, <c>ended</c>, <c>dismissed</c>, <c>stale</c>,
        /// <c>pending</c> or <c>unknown</c>.
        /// </summary>
        public string state;

        public LiveActivityContentInfo content;

        // The proxy sends `attributes` as an arbitrary caller-defined JSON object. Unity's
        // JsonUtility has no dictionary support, so the native layer splits it into two
        // parallel arrays before sending it over. Same approach as PushMessage extras.
        [SerializeField]
        private List<string> attributesKeys;
        [SerializeField]
        private List<string> attributesValues;

        /// <summary>
        /// Gets the activity attributes.
        /// </summary>
        /// <remarks>Non-string values are encoded as JSON strings.</remarks>
        /// <value>The attributes, or <c>null</c> if the activity carried none.</value>
        public Dictionary<string, string> Attributes
        {
            get { return AirshipUtils.PairFlattenedObject(attributesKeys, attributesValues); }
        }
    }

    /// <summary>
    /// Live Activity content as reported by the platform.
    ///
    /// This is the inbound counterpart of <see cref="LiveActivityContent"/>: the outbound
    /// type takes a dictionary directly, while what comes back has its arbitrary
    /// <c>state</c> object flattened for Unity's JsonUtility.
    /// </summary>
    [Serializable]
    public record LiveActivityContentInfo
    {
        // See the note on LiveActivityInfo.attributesKeys.
        [SerializeField]
        private List<string> stateKeys;
        [SerializeField]
        private List<string> stateValues;

        public string staleDate;
        public double relevanceScore;

        /// <summary>
        /// Gets the content state.
        /// </summary>
        /// <remarks>Non-string values are encoded as JSON strings.</remarks>
        /// <value>The state, or <c>null</c> if the content carried none.</value>
        public Dictionary<string, string> State
        {
            get { return AirshipUtils.PairFlattenedObject(stateKeys, stateValues); }
        }
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
        public Dictionary<string, object> attributes;
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
