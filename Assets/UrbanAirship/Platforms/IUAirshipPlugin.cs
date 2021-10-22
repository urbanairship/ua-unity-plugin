/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanAirship {

    interface IUAirshipPlugin {
        bool UserNotificationsEnabled {
            get;
            set;
        }

        string Tags {
            get;
        }

        string ChannelId {
            get;
        }

        string NamedUserId {
            get;
            set;
        }

        GameObject Listener {
            set;
        }

        bool DataCollectionEnabled {
            get;
            set;
        }

        bool PushTokenRegistrationEnabled {
            get;
            set;
        }

        string GetDeepLink (bool clear);

        string GetIncomingPush (bool clear);

        void AddTag (string tag);

        void RemoveTag (string tag);

        void AddCustomEvent (string customEvent);

        void TrackScreen (string screenName);

        void AssociateIdentifier (string key, string identifier);

        void DisplayMessageCenter ();

        void DisplayInboxMessage (string messageId);

        void RefreshInbox ();

        string InboxMessages ();

        void MarkInboxMessageRead (string messageId);

        void DeleteInboxMessage (string messageId);

        void SetAutoLaunchDefaultMessageCenter (bool enabled);

        int MessageCenterUnreadCount {
            get;
        }

        int MessageCenterCount {
            get;
        }

        void EditNamedUserTagGroups (string payload);

        void EditChannelTagGroups (string payload);

        void EditChannelAttributes (string payload);

        void EditNamedUserAttributes (string payload);

        void OpenPreferenceCenter (string preferenceCenterId);

        void SetEnabledFeatures (string[] enabledFeatures);

        void EnableFeatures (string[] enabledFeatures);

        void DisableFeatures (string[] disabledFeatures);

        bool IsFeatureEnabled (string feature);

        string[] GetEnabledFeatures ();

        TimeSpan InAppAutomationDisplayInterval {
            get;
            set;
        }

        bool InAppAutomationPaused {
            get;
            set;
        }

    }
}
