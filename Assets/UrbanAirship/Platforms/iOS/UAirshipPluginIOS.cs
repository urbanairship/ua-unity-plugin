/* Copyright Airship and Contributors */

#if UNITY_IOS

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UrbanAirship {

    class UAirshipPluginIOS : IUAirshipPlugin {

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_setListener (string listener);

        [DllImport ("__Internal")]
        private static extern string UAUnityPlugin_getDeepLink (bool clear);

        [DllImport ("__Internal")]
        private static extern string UAUnityPlugin_getIncomingPush (bool clear);

        [DllImport ("__Internal")]
        private static extern bool UAUnityPlugin_getUserNotificationsEnabled ();

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_setUserNotificationsEnabled (bool enabled);

        [DllImport ("__Internal")]
        private static extern string UAUnityPlugin_getTags ();

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_addTag (string tag);

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_removeTag (string tag);

        [DllImport ("__Internal")]
        private static extern string UAUnityPlugin_getChannelId ();

        // Analytics Function Imports
        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_addCustomEvent (string customEvent);

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_trackScreen (string screenName);

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_associateIdentifier (string key, string identifier);

        [DllImport ("__Internal")]
        private static extern string UAUnityPlugin_getNamedUserID ();

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_setNamedUserID (string namedUserID);

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_displayMessageCenter ();

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_displayInboxMessage (string messageId);

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_refreshInbox ();

        [DllImport ("__Internal")]
        private static extern string UAUnityPlugin_getInboxMessages ();

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_markInboxMessageRead (string messageId);

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_deleteInboxMessage (string messageId);

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_setAutoLaunchDefaultMessageCenter (bool enabled);

        [DllImport ("__Internal")]
        private static extern int UAUnityPlugin_getMessageCenterUnreadCount ();

        [DllImport ("__Internal")]
        private static extern int UAUnityPlugin_getMessageCenterCount ();

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_editNamedUserTagGroups (string payload);

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_editChannelTagGroups (string payload);

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_editChannelAttributes (string payload);

        [DllImport("__Internal")]
        private static extern void UAUnityPlugin_editNamedUserAttributes (string payload);

        [DllImport("__Internal")]
        private static extern void UAUnityPlugin_openPreferenceCenter (string preferenceCenterId);

        [DllImport("__Internal")]
        private static extern void UAUnityPlugin_setEnabledFeatures (string enabledFeatures);

        [DllImport("__Internal")]
        private static extern void UAUnityPlugin_enableFeatures (string enabledFeatures);

        [DllImport("__Internal")]
        private static extern void UAUnityPlugin_disableFeatures (string disabledFeatures);

        [DllImport("__Internal")]
        private static extern bool UAUnityPlugin_isFeatureEnabled (string features);
        
        [DllImport("__Internal")]
        private static extern bool UAUnityPlugin_isAnyFeatureEnabled ();

        [DllImport("__Internal")]
        private static extern string UAUnityPlugin_getEnabledFeatures ();

        [DllImport ("__Internal")]
        private static extern double UAUnityPlugin_getInAppAutomationDisplayInterval ();

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_setInAppAutomationDisplayInterval (double seconds);

        [DllImport ("__Internal")]
        private static extern bool UAUnityPlugin_isInAppAutomationPaused ();

        [DllImport ("__Internal")]
        private static extern void UAUnityPlugin_setInAppAutomationPaused (bool paused);

        public bool UserNotificationsEnabled {
            get {
                return UAUnityPlugin_getUserNotificationsEnabled ();
            }
            set {
                UAUnityPlugin_setUserNotificationsEnabled (value);
            }
        }

        public string Tags {
            get {
                return UAUnityPlugin_getTags ();
            }
        }

        public string ChannelId {
            get {
                return UAUnityPlugin_getChannelId ();
            }
        }

        public string NamedUserId {
            get {
                return UAUnityPlugin_getNamedUserID ();
            }

            set {
                UAUnityPlugin_setNamedUserID (value);
            }
        }

        public TimeSpan InAppAutomationDisplayInterval {
            get {
                return TimeSpan.FromSeconds (UAUnityPlugin_getInAppAutomationDisplayInterval ());
            }
            set {
                UAUnityPlugin_setInAppAutomationDisplayInterval (value.TotalSeconds);
            }
        }

        public bool InAppAutomationPaused {
            get {
                return UAUnityPlugin_isInAppAutomationPaused ();
            }
            set {
                UAUnityPlugin_setInAppAutomationPaused (value);
            }
        }

        public GameObject Listener {
            set {
                UAUnityPlugin_setListener (value.name);
            }
        }

        public string GetDeepLink (bool clear) {
            return UAUnityPlugin_getDeepLink (clear);
        }

        public string GetIncomingPush (bool clear) {
            return UAUnityPlugin_getIncomingPush (clear);
        }

        public void AddTag (string tag) {
            UAUnityPlugin_addTag (tag);
        }

        public void RemoveTag (string tag) {
            UAUnityPlugin_removeTag (tag);
        }

        public void AddCustomEvent (string customEvent) {
            UAUnityPlugin_addCustomEvent (customEvent);
        }

        public void TrackScreen (string screenName) {
            UAUnityPlugin_trackScreen (screenName);
        }

        public void AssociateIdentifier (string key, string identifier) {
            UAUnityPlugin_associateIdentifier (key, identifier);
        }

        public void DisplayMessageCenter () {
            UAUnityPlugin_displayMessageCenter ();
        }

        public void DisplayInboxMessage (string messageId) {
            UAUnityPlugin_displayInboxMessage (messageId);
        }

        public void RefreshInbox () {
            UAUnityPlugin_refreshInbox ();
        }

        public string InboxMessages () {
            return UAUnityPlugin_getInboxMessages ();
        }

        public void MarkInboxMessageRead (string messageId) {
            UAUnityPlugin_markInboxMessageRead (messageId);
        }

        public void DeleteInboxMessage (string messageId) {
            UAUnityPlugin_deleteInboxMessage (messageId);
        }

        public void SetAutoLaunchDefaultMessageCenter (bool enabled) {
            UAUnityPlugin_setAutoLaunchDefaultMessageCenter (enabled);
        }

        public int MessageCenterUnreadCount {
            get {
                return UAUnityPlugin_getMessageCenterUnreadCount ();
            }
        }

        public int MessageCenterCount {
            get {
                return UAUnityPlugin_getMessageCenterCount ();
            }
        }

        public void EditNamedUserTagGroups (string payload) {
            UAUnityPlugin_editNamedUserTagGroups (payload);
        }

        public void EditChannelTagGroups (string payload) {
            UAUnityPlugin_editChannelTagGroups (payload);
        }

        public void EditChannelAttributes (string payload) {
            UAUnityPlugin_editChannelAttributes (payload);
        }

        public void EditNamedUserAttributes (string payload)
        {
            UAUnityPlugin_editNamedUserAttributes (payload);
        }

        public void OpenPreferenceCenter (string preferenceCenterId)
        {
            UAUnityPlugin_openPreferenceCenter (preferenceCenterId);
        }

        public void SetEnabledFeatures (string[] enabledFeatures)
        {            
            UAUnityPlugin_setEnabledFeatures (String.Join(",", enabledFeatures));
        }

        public void EnableFeatures (string[] enabledFeatures)
        {
            UAUnityPlugin_enableFeatures (String.Join(",", enabledFeatures));
        }

        public void DisableFeatures (string[] disabledFeatures)
        {
            UAUnityPlugin_disableFeatures (String.Join(",", disabledFeatures));
        }

        public bool IsFeatureEnabled (string[] features) {
            return UAUnityPlugin_isFeatureEnabled (String.Join(",", features));
        }

        public bool IsAnyFeatureEnabled (string[] features) {
            //iOS method doesn't take any parameter
            return UAUnityPlugin_isAnyFeatureEnabled ();
        }

        public string[] GetEnabledFeatures () {
            return UAUnityPlugin_getEnabledFeatures().Split(',');
        }
    }
}

#endif
