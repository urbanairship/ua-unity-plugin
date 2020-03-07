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

        bool LocationEnabled {
            get;
            set;
        }

        bool BackgroundLocationAllowed {
            get;
            set;
        }

        string NamedUserId {
            get;
            set;
        }

        GameObject Listener {
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
