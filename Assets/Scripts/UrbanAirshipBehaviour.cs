/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UrbanAirship;

public class UrbanAirshipBehaviour : MonoBehaviour {
    public string addTagOnStart;

    void Awake () {
        Airship.Shared.TakeOff(new AirshipConfig() {
            defaultEnvironment = new ConfigEnvironment() {
                appKey = "APP_KEY",
                appSecret = "APP_SECRET",
                logLevel = LogLevel.Verbose,
            },
            site = Site.US,
            inProduction = false,
            urlAllowList = new string[] { "*" },
        });
    }

    void Start () {
        Debug.Log("Airship is flying: " + Airship.Shared.IsFlying());

        Airship.Shared.push.SetUserNotificationsEnabled(true);
        
        // if (!string.IsNullOrEmpty (addTagOnStart)) {
        //     UAirship.Shared.AddTag (addTagOnStart);
        // }

        Airship.Shared.OnPushReceived += OnPushReceived;
        Airship.Shared.OnPushOpened += OnPushOpened;
        Airship.Shared.OnChannelCreated += OnChannelCreated;
        Airship.Shared.OnDeepLinkReceived += OnDeepLinkReceived;
        Airship.Shared.OnInboxUpdated += OnInboxUpdated;
        Airship.Shared.OnShowInbox += OnShowInbox;
        Airship.Shared.OnPreferenceCenterDisplay += OnPreferenceCenterDisplay;

        // PrivacyManager
        Debug.Log("Set Enabled features to none");
        Airship.Shared.privacyManager.SetEnabledFeatures(new string[] { "none" });
        Debug.Log("Enabled features: " + string.Join(", ", Airship.Shared.privacyManager.GetEnabledFeatures()));
        
        Debug.Log("Enable push and analytics features");
        Airship.Shared.privacyManager.EnableFeatures(new string[] { "push", "analytics" });
        Debug.Log("Enabled features: " + string.Join(", ", Airship.Shared.privacyManager.GetEnabledFeatures()));

        Debug.Log("Disable analytics feature");
        Airship.Shared.privacyManager.DisableFeatures(new string[] { "analytics" });
        Debug.Log("Enabled features: " + string.Join(", ", Airship.Shared.privacyManager.GetEnabledFeatures()));

        Debug.Log("Is push feature enabled: " + Airship.Shared.privacyManager.IsFeaturesEnabled(new string[] { "push" }));
        Debug.Log("Is analytics feature enabled: " + Airship.Shared.privacyManager.IsFeaturesEnabled(new string[] { "analytics" }));

        Debug.Log("Set Enabled features to all");
        Airship.Shared.privacyManager.SetEnabledFeatures(new string[] { "all" });

        // Analytics
        Airship.Shared.analytics.TrackScreen("Main Camera");
        
        Airship.Shared.analytics.AssociateIdentifier("identifier", "my_identifier");
        
        CustomEvent customEvent = new CustomEvent();
        customEvent.EventName = "my_event";
        customEvent.EventValue = 123;
        Airship.Shared.analytics.AddCustomEvent(customEvent);

        Debug.Log("Session ID: " + Airship.Shared.analytics.GetSessionId());

        // Channel
        Debug.Log("Channel ID: " + Airship.Shared.channel.GetChannelId());

        StartCoroutine(Airship.Shared.channel.WaitForChannelId(
            onComplete: (channelId) => {
                Debug.Log($"Channel ID received: {channelId}");
            },
            onError: (error) => {
                Debug.LogError($"Error getting channel ID: {error.Message}");
            }
        ));

        Airship.Shared.channel.EditTags().AddTag("unity_tag").Apply();
        Airship.Shared.channel.EditTags().AddTag("tag_to_remove_1").Apply();
        Airship.Shared.channel.EditTags().AddTags(new string[] { "tag_to_remove_2", "tag_to_remove_3" }).Apply();
        Debug.Log("Tags: " + string.Join(", ", Airship.Shared.channel.GetTags()));
        Airship.Shared.channel.EditTags().RemoveTag("tag_to_remove_1").Apply();
        Airship.Shared.channel.EditTags().RemoveTags(new string[] { "tag_to_remove_2", "tag_to_remove_3" }).Apply();
        Debug.Log("Tags: " + string.Join(", ", Airship.Shared.channel.GetTags()));

        Airship.Shared.channel.EditTagGroups().AddTag("unity_tag_group", "tag_1").Apply();
        Airship.Shared.channel.EditTagGroups().AddTags("unity_tag_group", new string[] { "tag_2", "tag_3" }).Apply();
        Airship.Shared.channel.EditTagGroups().RemoveTag("unity_tag_group", "tag_2").Apply();
        Airship.Shared.channel.EditTagGroups().RemoveTags("unity_tag_group", new string[] { "tag_3" }).Apply();

        Airship.Shared.channel.EditSubscriptionLists().Subscribe("unity_subscription_list").Apply();
        Airship.Shared.channel.EditSubscriptionLists().Subscribe("unity_subscription_list_to_remove").Apply();
        StartCoroutine(Airship.Shared.channel.GetSubscriptionLists(
            onComplete: (subscriptionLists) => {
                Debug.Log("Channel Subscription lists: " + string.Join(", ", subscriptionLists));
            },
            onError: (error) => {
                Debug.LogError("Error getting subscription lists: " + error.Message);
            }
        ));
        Airship.Shared.channel.EditSubscriptionLists().Unsubscribe("unity_subscription_list_to_remove").Apply();

        Airship.Shared.channel.EditAttributes().SetAttribute("teststring", "a_string").Apply();
        Airship.Shared.channel.EditAttributes().SetAttribute("testint", (int) 1).Apply();
        Airship.Shared.channel.EditAttributes().SetAttribute("testlong", (long) 1000).Apply();
        Airship.Shared.channel.EditAttributes().SetAttribute("testfloat", (float)5.99).Apply();
        Airship.Shared.channel.EditAttributes().SetAttribute("testdouble", (double)5555.999).Apply();
        Airship.Shared.channel.EditAttributes().SetAttribute("testdate", DateTime.UtcNow).Apply();
        
        Airship.Shared.channel.EditAttributes().RemoveAttribute("teststring").Apply();
        Airship.Shared.channel.EditAttributes().RemoveAttribute("testint").Apply();

        // Contact
        Airship.Shared.contact.Identify("my_named_user");
        Debug.Log("Named user ID: " + Airship.Shared.contact.GetNamedUserId());
        Airship.Shared.contact.Reset();
        Debug.Log("Named user ID after reset: " + Airship.Shared.contact.GetNamedUserId());

        Airship.Shared.contact.EditTagGroups().AddTag("unity_tag_group", "tag_1").Apply();
        Airship.Shared.contact.EditTagGroups().AddTags("unity_tag_group", new string[] { "tag_2", "tag_3" }).Apply();
        Airship.Shared.contact.EditTagGroups().RemoveTag("unity_tag_group", "tag_2").Apply();
        Airship.Shared.contact.EditTagGroups().RemoveTags("unity_tag_group", new string[] { "tag_3" }).Apply();

        Airship.Shared.contact.EditAttributes().SetAttribute("teststring", "a_string").Apply();
        Airship.Shared.contact.EditAttributes().SetAttribute("testint", (int) 1).Apply();
        Airship.Shared.contact.EditAttributes().SetAttribute("testlong", (long) 1000).Apply();
        Airship.Shared.contact.EditAttributes().SetAttribute("testfloat", (float)5.99).Apply();
        Airship.Shared.contact.EditAttributes().SetAttribute("testdouble", (double)5555.999).Apply();
        Airship.Shared.contact.EditAttributes().SetAttribute("testdate", DateTime.UtcNow).Apply();
        Airship.Shared.contact.EditAttributes().RemoveAttribute("teststring").Apply();
        Airship.Shared.contact.EditAttributes().RemoveAttribute("testint").Apply();

        Airship.Shared.contact.EditSubscriptionLists().Subscribe("unity_subscription_list", SubscriptionScope.APP).Apply();
        Airship.Shared.contact.EditSubscriptionLists().Subscribe("unity_subscription_list_to_remove", SubscriptionScope.APP).Apply();
        Airship.Shared.contact.EditSubscriptionLists().Unsubscribe("unity_subscription_list_to_remove", SubscriptionScope.APP).Apply();
        StartCoroutine(Airship.Shared.contact.GetSubscriptionLists(
            onComplete: (subscriptionLists) => {
                Debug.Log("Contact Subscription lists:");
                foreach (var subscription in subscriptionLists) {
                    Debug.Log($"List: {subscription.Key}, Scopes: {string.Join(", ", subscription.Value)}");
                }
            },
            onError: (error) => {
                Debug.LogError("Error getting subscription lists: " + error.Message);
            }
        ));

        // InApp
        Airship.Shared.inApp.SetPaused(true);
        Debug.Log("InApp paused after true: " + Airship.Shared.inApp.IsPaused());
        Airship.Shared.inApp.SetPaused(false);
        Debug.Log("InApp paused after false: " + Airship.Shared.inApp.IsPaused());

        Airship.Shared.inApp.SetDisplayInterval(TimeSpan.FromSeconds(10));
        Debug.Log("InApp display interval: " + Airship.Shared.inApp.GetDisplayInterval());

        // Locale
        Airship.Shared.locale.SetLocaleOverride("en_US");
        Airship.Shared.locale.ClearLocaleOverride();
        // Debug.Log("Locale: " + Airship.Shared.locale.GetLocale());

        // Message Center
        StartCoroutine(Airship.Shared.messageCenter.RefreshInbox(
            onComplete: () => {
                Debug.Log("Refresh inbox complete");
            },
            onError: (error) => {
                Debug.LogError("Error refreshing inbox: " + error.Message);
            }
        ));

        Airship.Shared.messageCenter.SetAutoLaunchDefaultMessageCenter(true);

        StartCoroutine(Airship.Shared.messageCenter.GetUnReadCount(
            onComplete: (unreadCount) => {
                Debug.Log("Unread count: " + unreadCount);
            },
            onError: (error) => {
                Debug.LogError("Error getting unread count: " + error.Message);
            }
        ));

        StartCoroutine(Airship.Shared.messageCenter.GetMessages(
            onComplete: (messages) => {
                Debug.Log("Messages: " + string.Join(", ", messages));
            },
            onError: (error) => {
                Debug.LogError("Error getting messages: " + error.Message);
            }
        ));

        Airship.Shared.messageCenter.Display(null);
        // Airship.Shared.messageCenter.ShowMessageCenter(null);
        Airship.Shared.messageCenter.Dismiss();

        // Preference Center
        Airship.Shared.preferenceCenter.SetAutoLaunchDefaultPreferenceCenter("neat", true);
        // Airship.Shared.preferenceCenter.Display("neat");
        // StartCoroutine(Airship.Shared.preferenceCenter.GetConfig("neat",
        //     onComplete: (config) => {
        //         Debug.Log("Config: " + JsonUtility.ToJson(config));
        //     },
        //     onError: (error) => {
        //         Debug.LogError("Error getting config: " + error.Message);
        //     }
        // ));

        // Push
        
        Airship.Shared.push.SetUserNotificationsEnabled(false);
        Debug.Log("User notifications enabled after set to false: " + Airship.Shared.push.IsUserNotificationEnabled());
        
        StartCoroutine(Airship.Shared.push.EnableUserNotifications(
            new EnabledUserPushNotificationsArgs() {
                fallback = PromptPermissionFallback.SystemSettings
            },
            onComplete: (result) => {
                Debug.Log("User notifications enabled: " + result);
            },
            onError: (error) => {
                Debug.LogError("Error enabling user notifications: " + error.Message);
            }
        ));

        StartCoroutine(Airship.Shared.push.GetNotificationStatus(
            onComplete: (status) => {
                Debug.Log("Notification status: " + status);
            },
            onError: (error) => {
                Debug.LogError("Error getting notification status: " + error.Message);
            }
        ));

        Debug.Log("Push token: " + Airship.Shared.push.GetPushToken());

        Debug.Log("Active notifications: " + string.Join(", ", Airship.Shared.push.GetActiveNotifications()));

        // Airship.Shared.push.ClearNotifications();   
        // Debug.Log("Notifications cleared");

        Debug.Log("Is notification channel enabled: " + Airship.Shared.push.android.IsNotificationChannelEnabled("test_channel"));
        
        Airship.Shared.push.android.SetNotificationConfig(new AndroidNotificationConfig() {
            icon = "ic_notification",
            largeIcon = "ic_notification_large",
            defaultChannelId = "test_channel",
            accentColor = "#FF0000",
        });

        Airship.Shared.push.android.SetForegroundNotificationsEnabled(false);
        Debug.Log("Foreground notifications enabled: " + Airship.Shared.push.android.IsForegroundNotificationsEnabled());
        Airship.Shared.push.android.SetForegroundNotificationsEnabled(true);
        Debug.Log("Foreground notifications enabled after true: " + Airship.Shared.push.android.IsForegroundNotificationsEnabled());
    }

    void OnDestroy () {
        Airship.Shared.OnPushReceived -= OnPushReceived;
        Airship.Shared.OnPushOpened -= OnPushOpened;
        Airship.Shared.OnChannelCreated -= OnChannelCreated;
        Airship.Shared.OnDeepLinkReceived -= OnDeepLinkReceived;
        Airship.Shared.OnInboxUpdated -= OnInboxUpdated;
        Airship.Shared.OnShowInbox -= OnShowInbox;
        Airship.Shared.OnPreferenceCenterDisplay -= OnPreferenceCenterDisplay;
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

    void OnChannelCreated (string channelId) {
        Debug.Log ("Channel created: " + channelId);
    }

    void OnDeepLinkReceived (string deeplink) {
        Debug.Log ("Received deep link: " + deeplink);
    }

    void OnInboxUpdated (uint messageUnreadCount, uint messageCount)
    {
        Debug.Log("Inbox updated - unread messages: " + messageUnreadCount + " total messages: " + messageCount);

        StartCoroutine(Airship.Shared.messageCenter.GetMessages(
            onComplete: (messages) => {
                foreach (InboxMessage inboxMessage in messages)
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
            },
            onError: (error) => {
                Debug.LogError("Error getting messages: " + error.Message);
            }
        ));
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

    void OnPreferenceCenterDisplay (string preferenceCenterId) {
        Debug.Log ("Preference Center display - preferenceCenterId: " + preferenceCenterId);
    }
}
