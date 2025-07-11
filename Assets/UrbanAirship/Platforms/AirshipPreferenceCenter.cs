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
        /// Gets the preference center config.
        /// </summary>
        /// <param name="preferenceCenterId">The preference center Id.</param>
        /// <returns>The preference center config.</returns>
        public PreferenceCenterConfig GetConfig(string preferenceCenterId)
        {
            // TODO parse this from a Json into a PreferenceCenterConfig and return that
            return plugin.Call<string>("getPreferenceCenterConfig", preferenceCenterId);
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
        public readonly override PreferenceCenterConditionType type = PreferenceCenterConditionType.notificationOptIn;
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
        public override string title;
        public override string subtitle;
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
        public readonly override PreferenceCenterSectionType type = PreferenceCenterSectionType.CommonSection;
        public readonly override PreferenceCenterCommonDisplay? display;
        public readonly override List<PreferenceCenterItem>? items;
        public readonly override List<PreferenceCenterCondition>? conditions;
    }

    [Serializable]
    public class PreferenceCenterLabeledSectionBreak : PreferenceCenterSection
    {
        public readonly override PreferenceCenterSectionType type = PreferenceCenterSectionType.LabeledSectionBreak;
        public readonly override PreferenceCenterCommonDisplay? display;
        public readonly override List<PreferenceCenterItem>? items = null;
        public readonly override List<PreferenceCenterCondition>? conditions;
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
        public Map<string, dynamic> actions;
    }

    [Serializable]
    public class PreferenceCenterAlertItem : PreferenceCenterItem
    {
        public readonly override PreferenceCenterItemType type = PreferenceCenterItemType.Alert;
        public readonly override PreferenceCenterCommonDisplay display;
        public readonly override List<PreferenceCenterCondition>? conditions;
        public readonly PreferenceCenterAlertItemButton? button;
    }

    [Serializable]
    public class PreferenceCenterChannelSubscriptionItem : PreferenceCenterItem
    {
        public readonly override PreferenceCenterItemType type = PreferenceCenterItemType.ChannelSubscription;
        public readonly override PreferenceCenterCommonDisplay display;
        public readonly override List<PreferenceCenterCondition>? conditions;
        public readonly string subscriptionId;
    }

    [Serializable]
    public class PreferenceCenterContactSubscriptionItem : PreferenceCenterItem
    {
        public readonly override PreferenceCenterItemType type = PreferenceCenterItemType.ContactSubscription;
        public readonly override PreferenceCenterCommonDisplay display;
        public readonly override List<PreferenceCenterCondition>? conditions;
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
        public readonly override PreferenceCenterItemType type = PreferenceCenterItemType.ContactSubscriptionGroup;
        public readonly override PreferenceCenterCommonDisplay display;
        public readonly override List<PreferenceCenterCondition>? conditions;
        public readonly string subscriptionId;
        public readonly List<PreferenceCenterContactSubscriptionGroupItemComponent> components;
    }
}