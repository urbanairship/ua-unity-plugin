/* Copyright Urban Airship and Contributors */

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

        string Alias {
            get;
            set;
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

        void AssociateIdentifier (string key, string identifier);

        void DisplayMessageCenter ();

        int MessageCenterUnreadCount {
            get;
        }

        int MessageCenterCount {
            get;
        }

        void EditNamedUserTagGroups (string payload);

        void EditChannelTagGroups (string payload);
    }
}
