/* Copyright Urban Airship and Contributors */

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
        public GameObject Listener { set; private get; }
        public string GetDeepLink (bool clear) { return null; }
        public string GetIncomingPush (bool clear) { return null; }
        public void AddTag (string tag) { }
        public void RemoveTag (string tag) { }
        public void AddCustomEvent (string customEvent) { }
        public void AssociateIdentifier (string key, string identifier) { }
        public void DisplayMessageCenter () { }
        public int MessageCenterUnreadCount { get; private set; }
        public int MessageCenterCount { get; private set; }
        public void EditNamedUserTagGroups (string payload) { }
        public void EditChannelTagGroups (string payload) { }
    }
}
