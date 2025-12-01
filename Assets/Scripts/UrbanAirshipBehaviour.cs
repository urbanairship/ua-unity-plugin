/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UrbanAirship;

public class UrbanAirshipBehaviour : MonoBehaviour {
    public string addTagOnStart;

    void Awake () {
        //Airship.Shared.push.SetUserNotificationsEnabled(true);
        //Airship.Shared.push.UserNotificationsEnabled = true;
    }

    void Start () {
        // if (!string.IsNullOrEmpty (addTagOnStart)) {
        //     UAirship.Shared.AddTag (addTagOnStart);
        // }

        string[] allenable = new string[] { "FEATURE_ALL" };
        //Airship.Shared.privacyManager.SetEnabledFeatures(allenable);

        // UAirship.Shared.OnPushReceived += OnPushReceived;
        // UAirship.Shared.OnChannelUpdated += OnChannelUpdated;
        // UAirship.Shared.OnDeepLinkReceived += OnDeepLinkReceived;
        // UAirship.Shared.OnPushOpened += OnPushOpened;
        // UAirship.Shared.OnInboxUpdated += OnInboxUpdated;
        // UAirship.Shared.OnShowInbox += OnShowInbox;

        Airship.Shared.analytics.TrackScreen("Main Camera");
        
        CustomEvent customEvent = new CustomEvent();
        customEvent.EventName = "event_name";
        customEvent.EventValue = 123;
        Airship.Shared.analytics.AddCustomEvent(customEvent);

        Airship.Shared.channel.EditTags().AddTag("ulrich").Apply();

        Airship.Shared.channel.EditAttributes().SetAttribute("teststring", "a_string").Apply();
        Airship.Shared.channel.EditAttributes().SetAttribute("testint", (int) 1).Apply();
        Airship.Shared.channel.EditAttributes().SetAttribute("testlong", (long) 1000).Apply();
        Airship.Shared.channel.EditAttributes().SetAttribute("testfloat", (float)5.99).Apply();
        Airship.Shared.channel.EditAttributes().SetAttribute("testdouble", (double)5555.999).Apply();
        Airship.Shared.channel.EditAttributes().SetAttribute("testdate", DateTime.UtcNow).Apply();

        Airship.Shared.channel.EditAttributes().RemoveAttribute("teststring").Apply();
        Airship.Shared.channel.EditAttributes().RemoveAttribute("testint").Apply();

        StartCoroutine(Airship.Shared.messageCenter.RefreshInbox(
            onComplete: () => {
                Debug.Log("Refresh inbox complete");
            },
            onError: (error) => {
                Debug.LogError("Error refreshing inbox: " + error.Message);
            }
        ));

        StartCoroutine(Airship.Shared.channel.WaitForChannelId(
            onComplete: (channelId) => {
                Debug.Log($"Channel ID received: {channelId}");
            },
            onError: (error) => {
                Debug.LogError($"Error getting channel ID: {error.Message}");
            }
        ));
    }

    // void OnDestroy () {
    //     UAirship.Shared.OnPushReceived -= OnPushReceived;
    //     UAirship.Shared.OnChannelUpdated -= OnChannelUpdated;
    //     UAirship.Shared.OnDeepLinkReceived -= OnDeepLinkReceived;
    //     UAirship.Shared.OnPushOpened -= OnPushOpened;
    // }

    // void OnPushReceived (PushMessage message) {
    //     Debug.Log ("Received push! " + message.Alert);

    //     if (message.Extras != null) {
    //         foreach (KeyValuePair<string, string> kvp in message.Extras) {
    //             Debug.Log (string.Format ("Extras Key = {0}, Value = {1}", kvp.Key, kvp.Value));
    //         }
    //     }
    // }

    // void OnPushOpened (PushMessage message) {
    //     Debug.Log ("Opened Push! " + message.Alert);

    //     if (message.Extras != null) {
    //         foreach (KeyValuePair<string, string> kvp in message.Extras) {
    //             Debug.Log (string.Format ("Extras Key = {0}, Value = {1}", kvp.Key, kvp.Value));
    //         }
    //     }
    // }

    // void OnChannelUpdated (string channelId) {
    //     Debug.Log ("Channel updated: " + channelId);
    // }

    // void OnDeepLinkReceived (string deeplink) {
    //     Debug.Log ("Received deep link: " + deeplink);
    // }

    // void OnInboxUpdated (uint messageUnreadCount, uint messageCount)
    // {
    //     Debug.Log("Inbox updated - unread messages: " + messageUnreadCount + " total messages: " + messageCount);

    //     IEnumerable<InboxMessage>inboxMessages = UAirship.Shared.InboxMessages();
    //     foreach (InboxMessage inboxMessage in inboxMessages)
    //     {
    //         Debug.Log("Message id: " + inboxMessage.id + ", title: " + inboxMessage.title + ", sentDate: " + inboxMessage.sentDate + ", isRead: " + inboxMessage.isRead + ", isDeleted: " + inboxMessage.isDeleted);
    //         if (inboxMessage.extras == null)
    //         {
    //             Debug.Log("Extras is null");
    //         }
    //         else
    //         {
    //             foreach (KeyValuePair<string, string> entry in inboxMessage.extras)
    //             {
    //                 Debug.Log("Message extras [" + entry.Key + "] = " + entry.Value);
    //             }
    //         }
    //     }
    // }

    // void OnShowInbox (string messageId)
    // {
    //     if (messageId == null)
    //     {
    //         Debug.Log("OnShowInbox - show inbox");
    //     }
    //     else
    //     {
    //         Debug.Log("OnShowInbox - show message: messageId = " + messageId);
    //     }
    // }
}
