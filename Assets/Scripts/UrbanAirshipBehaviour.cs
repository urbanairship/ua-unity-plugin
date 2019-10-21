/* Copyright Airship and Contributors */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UrbanAirship;

public class UrbanAirshipBehaviour : MonoBehaviour {
    public string addTagOnStart;

    void Awake () {
        UAirship.Shared.UserNotificationsEnabled = true;
    }

    void Start () {
        if (!string.IsNullOrEmpty (addTagOnStart)) {
            UAirship.Shared.AddTag (addTagOnStart);
        }

        UAirship.Shared.OnPushReceived += OnPushReceived;
        UAirship.Shared.OnChannelUpdated += OnChannelUpdated;
        UAirship.Shared.OnDeepLinkReceived += OnDeepLinkReceived;
        UAirship.Shared.OnPushOpened += OnPushOpened;
        UAirship.Shared.OnInboxUpdated += OnInboxUpdated;
        UAirship.Shared.OnShowInbox += OnShowInbox;

        UAirship.Shared.RefreshInbox();
    }

    void OnDestroy () {
        UAirship.Shared.OnPushReceived -= OnPushReceived;
        UAirship.Shared.OnChannelUpdated -= OnChannelUpdated;
        UAirship.Shared.OnDeepLinkReceived -= OnDeepLinkReceived;
        UAirship.Shared.OnPushOpened -= OnPushOpened;
    }

    void OnPushReceived (PushMessage message) {
        Debug.Log ("Received push! " + message.Alert);

        if (message.Extras != null) {
            foreach (KeyValuePair<string, string> kvp in message.Extras) {
                Debug.Log (string.Format ("Extras Key = {0}, Value = {1}", kvp.Key, kvp.Value));
            }
        }
    }

    void OnPushOpened (PushMessage message) {
        Debug.Log ("Opened Push! " + message.Alert);

        if (message.Extras != null) {
            foreach (KeyValuePair<string, string> kvp in message.Extras) {
                Debug.Log (string.Format ("Extras Key = {0}, Value = {1}", kvp.Key, kvp.Value));
            }
        }
    }

    void OnChannelUpdated (string channelId) {
        Debug.Log ("Channel updated: " + channelId);
    }

    void OnDeepLinkReceived (string deeplink) {
        Debug.Log ("Received deep link: " + deeplink);
    }

    void OnInboxUpdated (uint messageUnreadCount, uint messageCount)
    {
        Debug.Log("Inbox updated - unread messages: " + messageUnreadCount + " total messages: " + messageCount);

        IEnumerable<InboxMessage>inboxMessages = UAirship.Shared.InboxMessages();
        foreach (InboxMessage inboxMessage in inboxMessages)
        {
            Debug.Log("Message id: " + inboxMessage.id + ", title: " + inboxMessage.title + ", sentDate: " + inboxMessage.sentDate + ", isRead: " + inboxMessage.isRead + ", isDeleted: " + inboxMessage.isDeleted);
            if (inboxMessage.extras == null)
            {
                Debug.Log("Extras is null");
            }
            else
            {
                foreach (KeyValuePair<string, string> entry in inboxMessage.extras)
                {
                    Debug.Log("Message extras [" + entry.Key + "] = " + entry.Value);
                }
            }
        }
    }

    void OnShowInbox (string messageId)
    {
        if (messageId == null)
        {
            Debug.Log("OnShowInbox - show inbox");
        }
        else
        {
            Debug.Log("OnShowInbox - show message: messageId = " + messageId);
        }
    }
}
