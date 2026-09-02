/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable annotations

namespace AirshipSDK
{

    /// <summary>
    /// Airship Preference Center.
    /// </summary>
    public class AirshipPreferenceCenter
    {
        private IAirshipPlugin plugin;

        internal AirshipPreferenceCenter(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Requests to display a preference center.
        /// 
        /// Will either emit an event to display the Preference Center if auto launch is disabled, or display the OOTB UI.
        /// </summary>
        /// <param name="preferenceCenterId">The preference center Id.</param>
        public void Display(string preferenceCenterId)
        {
            plugin.Call("displayPreferenceCenter", preferenceCenterId);
        }

        /// <summary>
        /// Gets the preference center config asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="preferenceCenterId">The preference center Id.</param>
        /// <param name="onComplete">Callback invoked with the config when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator GetConfig(string preferenceCenterId, Action<PreferenceCenterConfig> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    string configJson = plugin.Call<string>("getPreferenceCenterConfig", preferenceCenterId);
                    if (string.IsNullOrEmpty(configJson))
                    {
                        throw new Exception("Airship: empty response from getPreferenceCenterConfig");
                    }
                    return JsonUtility.FromJson<PreferenceCenterConfig>(configJson);
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Enables or disables showing the OOTB UI when requested to display by either 
        /// `Display` or by a notification with some other action.
        /// </summary>
        /// <param name="preferenceCenterId">The preference center Id.</param>
        /// <param name="autoLaunch">true to show OOTB UI, false to emit events.</param>
        public void SetAutoLaunchDefaultPreferenceCenter(string preferenceCenterId, bool autoLaunch)
        {
            plugin.Call("setAutoLaunchDefaultPreferenceCenter", preferenceCenterId, autoLaunch);
        }
    }

    /// <summary>
    /// Preference center configuration.
    ///
    /// Field names mirror the Airship preference center form JSON, which the framework
    /// proxy passes through verbatim. Members that would otherwise be snake_case on the
    /// wire are exposed through a property; the `type` discriminators arrive as strings
    /// and are surfaced as enums that degrade to `Unknown` for values added server-side
    /// after this plugin was built.
    ///
    /// Everything here is deliberately concrete with no dictionaries: Unity's JsonUtility
    /// cannot instantiate abstract types and has no dictionary support, so a polymorphic
    /// model deserializes to nulls.
    /// </summary>
    [Serializable]
    public class PreferenceCenterConfig
    {
        public string id;
        public PreferenceCenterCommonDisplay display;
        public List<PreferenceCenterSection> sections;
    }

    /// <summary>
    /// Display information for a section, item or config.
    /// </summary>
    [Serializable]
    public class PreferenceCenterCommonDisplay
    {
        /// <summary>The display name.</summary>
        public string name;

        /// <summary>Optional description.</summary>
        public string description;

        /// <summary>Optional icon URL. Only set on alert items.</summary>
        public string icon;
    }

    /// <summary>
    /// Section types. `Unknown` covers values this plugin version does not model.
    /// </summary>
    public enum PreferenceCenterSectionType
    {
        [AirshipEnumStringValue("unknown")]
        Unknown,
        [AirshipEnumStringValue("section")]
        Section,
        [AirshipEnumStringValue("labeled_section_break")]
        LabeledSectionBreak
    }

    /// <summary>
    /// A preference center section. `items` is empty for a labeled section break.
    /// </summary>
    [Serializable]
    public class PreferenceCenterSection
    {
        [SerializeField]
        private string type;

        public string id;
        public PreferenceCenterCommonDisplay display;
        public List<PreferenceCenterItem> items;
        public List<PreferenceCenterCondition> conditions;

        /// <summary>The section type.</summary>
        public PreferenceCenterSectionType Type
        {
            get { return AirshipUtils.ParseEnum(type, PreferenceCenterSectionType.Unknown); }
        }

        /// <summary>The raw section type string, for types this version does not model.</summary>
        public string RawType
        {
            get { return type; }
        }
    }

    /// <summary>
    /// Item types. `Unknown` covers values this plugin version does not model.
    /// </summary>
    public enum PreferenceCenterItemType
    {
        [AirshipEnumStringValue("unknown")]
        Unknown,
        [AirshipEnumStringValue("channel_subscription")]
        ChannelSubscription,
        [AirshipEnumStringValue("contact_subscription")]
        ContactSubscription,
        [AirshipEnumStringValue("contact_subscription_group")]
        ContactSubscriptionGroup,
        [AirshipEnumStringValue("alert")]
        Alert
    }

    /// <summary>
    /// A preference center item. Which of the optional members are populated depends on
    /// <see cref="Type"/>: `SubscriptionId` for the subscription types, `scopes` for
    /// contact subscriptions, `components` for a contact subscription group, and
    /// `button` for an alert.
    /// </summary>
    [Serializable]
    public class PreferenceCenterItem
    {
        [SerializeField]
        private string type;

        [SerializeField]
        private string subscription_id;

        public string id;
        public PreferenceCenterCommonDisplay display;
        public List<PreferenceCenterCondition> conditions;

        /// <summary>Subscription scopes. Contact subscription items only.</summary>
        public List<string> scopes;

        /// <summary>Group components. Contact subscription group items only.</summary>
        public List<PreferenceCenterContactSubscriptionGroupItemComponent> components;

        /// <summary>The alert button. Alert items only.</summary>
        public PreferenceCenterAlertItemButton button;

        /// <summary>The item type.</summary>
        public PreferenceCenterItemType Type
        {
            get { return AirshipUtils.ParseEnum(type, PreferenceCenterItemType.Unknown); }
        }

        /// <summary>The raw item type string, for types this version does not model.</summary>
        public string RawType
        {
            get { return type; }
        }

        /// <summary>The subscription list ID. Subscription items only.</summary>
        public string SubscriptionId
        {
            get { return subscription_id; }
        }
    }

    /// <summary>
    /// A component of a contact subscription group item.
    /// </summary>
    [Serializable]
    public class PreferenceCenterContactSubscriptionGroupItemComponent
    {
        public List<string> scopes;
        public PreferenceCenterCommonDisplay display;
    }

    /// <summary>
    /// An alert item's button.
    /// </summary>
    /// <remarks>
    /// The wire payload also carries an `actions` object. Unity's JsonUtility cannot
    /// represent arbitrary JSON, so it is not surfaced; exposing it needs the native
    /// layers to stringify the object first, the way the feature flag `_internal` and
    /// `variables` fields already do.
    /// </remarks>
    [Serializable]
    public class PreferenceCenterAlertItemButton
    {
        public string text;

        [SerializeField]
        private string content_description;

        /// <summary>Accessibility description for the button.</summary>
        public string ContentDescription
        {
            get { return content_description; }
        }
    }

    /// <summary>
    /// Condition types. `Unknown` covers values this plugin version does not model.
    /// </summary>
    public enum PreferenceCenterConditionType
    {
        [AirshipEnumStringValue("unknown")]
        Unknown,
        [AirshipEnumStringValue("notification_opt_in")]
        NotificationOptIn
    }

    /// <summary>
    /// Opt-in status a condition matches on.
    /// </summary>
    public enum PreferenceCenterOptInStatus
    {
        [AirshipEnumStringValue("unknown")]
        Unknown,
        [AirshipEnumStringValue("opt_in")]
        OptIn,
        [AirshipEnumStringValue("opt_out")]
        OptOut
    }

    /// <summary>
    /// A display condition on a section or item.
    /// </summary>
    [Serializable]
    public class PreferenceCenterCondition
    {
        [SerializeField]
        private string type;

        [SerializeField]
        private string when_status;

        /// <summary>The condition type.</summary>
        public PreferenceCenterConditionType Type
        {
            get { return AirshipUtils.ParseEnum(type, PreferenceCenterConditionType.Unknown); }
        }

        /// <summary>The opt-in status this condition matches on.</summary>
        public PreferenceCenterOptInStatus WhenStatus
        {
            get { return AirshipUtils.ParseEnum(when_status, PreferenceCenterOptInStatus.Unknown); }
        }
    }
}
