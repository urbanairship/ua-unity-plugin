/* Copyright Airship and Contributors */

#if UNITY_ANDROID

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {

    class UAirshipPluginAndroid : IUAirshipPlugin {

        private AndroidJavaObject androidPlugin;

        public UAirshipPluginAndroid () {
            try {
                using (AndroidJavaClass pluginClass = new AndroidJavaClass ("com.urbanairship.unityplugin.UnityPlugin")) {
                    androidPlugin = pluginClass.CallStatic<AndroidJavaObject> ("shared");
                }
            } catch (Exception) {
                Debug.LogError ("UAirship plugin not found");
            }
        }

        public bool UserNotificationsEnabled {
            get {
                return Call<bool> ("getUserNotificationsEnabled");
            }
            set {
                Call ("setUserNotificationsEnabled", value);
            }
        }

        public string Tags {
            get {
                return Call<string> ("getTags");
            }
        }

        public string ChannelId {
            get {
                return Call<string> ("getChannelId");
            }
        }

        public string NamedUserId {
            get {
                return Call<string> ("getNamedUserId");
            }

            set {
                Call ("setNamedUserId", value);
            }
        }

        public TimeSpan InAppAutomationDisplayInterval {
            get {
                return TimeSpan.FromSeconds (Call<double> ("getInAppAutomationDisplayInterval"));
            }
            set {
                Call ("setInAppAutomationDisplayInterval", value.TotalSeconds);
            }
        }

        public bool InAppAutomationPaused {
             get {
                return Call<bool> ("isInAppAutomationPaused");
            }
            set {
                Call ("setInAppAutomationPaused", value);
            }
        }

        public GameObject Listener {
            set {
                Call ("setListener", value.name);
            }
        }

        public string GetIncomingPush (bool clear) {
            return Call<string> ("getIncomingPush", clear);
        }

        public string GetDeepLink (bool clear) {
            return Call<string> ("getDeepLink", clear);
        }

        public void AddTag (string tag) {
            Call ("addTag", tag);
        }

        public void RemoveTag (string tag) {
            Call ("removeTag", tag);
        }

        public void AddCustomEvent (string customEvent) {
            Call ("addCustomEvent", customEvent);
        }

        public void TrackScreen (string screenName) {
            Call ("trackScreen", screenName);
        }

        public void AssociateIdentifier (string key, string identifier) {
            Call ("associateIdentifier", key, identifier);
        }

        public void DisplayMessageCenter () {
            Call ("displayMessageCenter");
        }

        public void DisplayInboxMessage (string messageId) {
            Call ("displayInboxMessage", messageId);
        }

        public void RefreshInbox () {
            Call ("refreshInbox");
        }

        public string InboxMessages () {
            return Call<string> ("getInboxMessages");
        }

        public void MarkInboxMessageRead (string messageId) {
            Call ("markInboxMessageRead", messageId);
        }

        public void DeleteInboxMessage (string messageId) {
            Call ("deleteInboxMessage", messageId);
        }

        public void SetAutoLaunchDefaultMessageCenter (bool enabled) {
            Call ("setAutoLaunchDefaultMessageCenter", enabled);
        }

        public int MessageCenterUnreadCount {
            get {
                return Call<int> ("getMessageCenterUnreadCount");
            }
        }

        public int MessageCenterCount {
            get {
                return Call<int> ("getMessageCenterCount");
            }
        }

        public void EditNamedUserTagGroups (string payload) {
            Call ("editNamedUserTagGroups", payload);
        }

        public void EditChannelTagGroups (string payload) {
            Call ("editChannelTagGroups", payload);
        }

        public void EditChannelAttributes (string payload) {
            Call ("editChannelAttributes", payload);
        }

        public void EditNamedUserAttributes (string payload) {
            Call ("editNamedUserAttributes", payload);
        }

        public void OpenPreferenceCenter (string preferenceCenterId) {
            Call ("openPreferenceCenter", preferenceCenterId);
        }

        public void SetEnabledFeatures (string[] enabledFeatures) {
            Call ("setEnabledFeatures", MakeJavaArray(enabledFeatures));
        }

        public void EnableFeatures (string[] enabledFeatures) {
            Call ("enableFeatures", MakeJavaArray(enabledFeatures));
        }

        public void DisableFeatures (string[] disabledFeatures) {
            Call ("disableFeatures", MakeJavaArray(disabledFeatures));
        }

        public bool IsFeatureEnabled (string[] features) {
            return Call<bool> ("isFeatureEnabled", MakeJavaArray(features));
        }

        public bool IsAnyFeatureEnabled (string[] features) {
            return Call<bool> ("isAnyFeatureEnabled", MakeJavaArray(features));
        }

        public string[] GetEnabledFeatures () {
            return Call<string[]> ("getEnabledFeatures");
        }

        /// Internal method to make a Java Array with an array of String values, to be used with the 
        /// "setEnabledFeatures" method.
        private AndroidJavaObject MakeJavaArray(string [] values) {
            AndroidJavaClass arrayClass  = new AndroidJavaClass("java.lang.reflect.Array");
            AndroidJavaObject arrayObject = arrayClass.CallStatic<AndroidJavaObject>("newInstance", new AndroidJavaClass("java.lang.String"), values.Count());
            for (int i=0; i<values.Count(); ++i) {
                arrayClass.CallStatic("set", arrayObject, i, new AndroidJavaObject("java.lang.String", values[i]));
            }

            return arrayObject;
        }

        private void Call (string method, params object[] args) {
            if (androidPlugin != null) {
                androidPlugin.Call (method, args);
            }
        }

        private T Call<T> (string method, params object[] args) {
            if (androidPlugin != null) {
                return androidPlugin.Call<T> (method, args);
            }
            return default (T);
        }
    }
}

#endif
