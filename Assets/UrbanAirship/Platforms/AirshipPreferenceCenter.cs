/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship
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
                () => JsonUtility.FromJson<PreferenceCenterConfig>(plugin.Call<string>("getPreferenceCenterConfig", preferenceCenterId)),
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

    [Serializable]
    public class PreferenceCenterConfig
    {
        public string id;
        public List<PreferenceCenterSection> sections;
        [SerializeField]
        public PreferenceCenterCommonDisplay? display;
    }

    [Serializable]
    public class PreferenceCenterConditionState
    {
        public bool notificationOptIn;
    }

    [Serializable]
    public enum PreferenceCenterConditionType
    {
        notificationOptIn
    }

    [Serializable]
    public abstract class PreferenceCenterCondition
    {
        public PreferenceCenterConditionType? type = null;
    }

    [Serializable]
    enum PreferenceCenterConditionOptIn
    {
        optIn,
        optOut
    }

    [Serializable]
    class PreferenceCenterNotificationOptInCondition : PreferenceCenterCondition
    {
        public readonly PreferenceCenterConditionType type = PreferenceCenterConditionType.notificationOptIn;
        public PreferenceCenterConditionOptIn whenStatus;
    }

    [Serializable]
    public class PreferenceCenterCommonDisplay
    {
        public string title;
        public string subtitle;
    }

    [Serializable]
    public class PreferenceCenterIconDisplay : PreferenceCenterCommonDisplay
    {
        public string title;
        public string subtitle;
        public string icon;
    }

    [Serializable]
    public enum PreferenceCenterSectionType
    {
        CommonSection,
        LabeledSectionBreak
    }

    [Serializable]
    public abstract class PreferenceCenterSection
    {
        public PreferenceCenterSectionType type;
        public PreferenceCenterCommonDisplay? display;
        public List<PreferenceCenterItem>? items;
        public List<PreferenceCenterCondition>? conditions;
    }

    [Serializable]
    public class PreferenceCenterCommonSection : PreferenceCenterSection
    {
        public readonly PreferenceCenterSectionType type = PreferenceCenterSectionType.CommonSection;
        public readonly PreferenceCenterCommonDisplay? display;
        public readonly List<PreferenceCenterItem>? items;
        public readonly List<PreferenceCenterCondition>? conditions;
    }

    [Serializable]
    public class PreferenceCenterLabeledSectionBreak : PreferenceCenterSection
    {
        public readonly PreferenceCenterSectionType type = PreferenceCenterSectionType.LabeledSectionBreak;
        public readonly PreferenceCenterCommonDisplay? display;
        public readonly List<PreferenceCenterItem>? items = null;
        public readonly List<PreferenceCenterCondition>? conditions;
    }

    [Serializable]
    public enum PreferenceCenterItemType
    {
        ChannelSubscription,
        ContactSubscription,
        ContactSubscriptionGroup,
        Alert
    }

    [Serializable]
    public abstract class PreferenceCenterItem
    {
        public PreferenceCenterItemType type;
        public PreferenceCenterCommonDisplay display;
        public List<PreferenceCenterCondition>? conditions;
    }

    [Serializable]
    public class PreferenceCenterAlertItemButton
    {
        public string text;
        public string contentDescription;
        public Dictionary<string, dynamic> actions;
    }

    [Serializable]
    public class PreferenceCenterAlertItem : PreferenceCenterItem
    {
        public readonly PreferenceCenterItemType type = PreferenceCenterItemType.Alert;
        public readonly PreferenceCenterCommonDisplay display;
        public readonly List<PreferenceCenterCondition>? conditions;
        public readonly PreferenceCenterAlertItemButton? button;
    }

    [Serializable]
    public class PreferenceCenterChannelSubscriptionItem : PreferenceCenterItem
    {
        public readonly PreferenceCenterItemType type = PreferenceCenterItemType.ChannelSubscription;
        public readonly PreferenceCenterCommonDisplay display;
        public readonly List<PreferenceCenterCondition>? conditions;
        public readonly string subscriptionId;
    }

    [Serializable]
    public class PreferenceCenterContactSubscriptionItem : PreferenceCenterItem
    {
        public readonly PreferenceCenterItemType type = PreferenceCenterItemType.ContactSubscription;
        public readonly PreferenceCenterCommonDisplay display;
        public readonly List<PreferenceCenterCondition>? conditions;
        public readonly string subscriptionId;
        public readonly List<string> scopes;
    }

    [Serializable]
    public class PreferenceCenterContactSubscriptionGroupItemComponent
    {
        public List<string> scopes;
        public PreferenceCenterCommonDisplay display;
    }

    [Serializable]
    public class PreferenceCenterContactSubscriptionGroupItem : PreferenceCenterItem
    {
        public readonly PreferenceCenterItemType type = PreferenceCenterItemType.ContactSubscriptionGroup;
        public readonly PreferenceCenterCommonDisplay display;
        public readonly List<PreferenceCenterCondition>? conditions;
        public readonly string subscriptionId;
        public readonly List<PreferenceCenterContactSubscriptionGroupItemComponent> components;
    }
}