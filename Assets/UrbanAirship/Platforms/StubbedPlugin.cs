/* Copyright Airship and Contributors */

using System;
using UnityEngine;

namespace UrbanAirship {
    class StubbedPlugin : IUAirshipPlugin {
        public bool UserNotificationsEnabled { get; set; }
        public string Tags { get { return null; } }
        public string ChannelId { get { return null; } }
        public bool LocationEnabled { get; set; }
        public bool BackgroundLocationAllowed { get; set; }
        public string NamedUserId { get; set; }
        public TimeSpan InAppAutomationDisplayInterval { get; set; }
        public bool InAppAutomationPaused { get; set; }
        public GameObject Listener { set; private get; }
        public string GetDeepLink (bool clear) { return null; }
        public string GetIncomingPush (bool clear) { return null; }
        public void AddTag (string tag) { }
        public void RemoveTag (string tag) { }
        public void AddCustomEvent (string customEvent) { }
        public void TrackScreen (string screenName) { }
        public void AssociateIdentifier (string key, string identifier) { }
        public void DisplayMessageCenter () { }
        public void DisplayInboxMessage (string messageId) { }
        public void RefreshInbox () { }
        public string InboxMessages () { return null; }
        public void MarkInboxMessageRead (string messageId) { }
        public void DeleteInboxMessage (string messageId) { }
        public void SetAutoLaunchDefaultMessageCenter (bool enabled) { }
        public int MessageCenterUnreadCount { get; private set; }
        public int MessageCenterCount { get; private set; }
        public void EditNamedUserTagGroups (string payload) { }
        public void EditChannelTagGroups (string payload) { }
        public void EditChannelAttributes (string payload) { }
        public bool DataCollectionEnabled { get; set; }
        public bool PushTokenRegistrationEnabled { get; set; }
    }
}
