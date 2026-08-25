/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirshipSDK;

public class AirshipBehaviour : MonoBehaviour {

    void Awake () {
        Debug.Log("Taking off");
        bool takeOffResult = Airship.Shared.TakeOff(new AirshipConfig() {
            @default = new ConfigEnvironment() {
                appKey = "APP_KEY",
                appSecret = "APP_SECRET",
                logLevel = LogLevel.Verbose,
            },
            site = Site.US,
            inProduction = false,
            urlAllowList = new string[] { "*" },
        });
        Debug.Log("TakeOff returned: " + takeOffResult);
    }

    void Start () {
        Debug.Log("Airship is flying: " + Airship.Shared.IsFlying());

        Airship.Shared.push.SetUserNotificationsEnabled(true);

        Airship.Shared.OnPushReceived += OnPushReceived;
        Airship.Shared.OnPushOpened += OnPushOpened;
        Airship.Shared.OnChannelCreated += OnChannelCreated;
        Airship.Shared.OnDeepLinkReceived += OnDeepLinkReceived;
        Airship.Shared.OnInboxUpdated += OnInboxUpdated;
        Airship.Shared.OnShowInbox += OnShowInbox;
        Airship.Shared.OnPreferenceCenterDisplay += OnPreferenceCenterDisplay;
        Airship.Shared.OnPushTokenReceived += OnPushTokenReceived;
        Airship.Shared.OnNotificationStatusChanged += OnNotificationStatusChanged;
        Airship.Shared.OnAuthorizedSettingsChanged += OnAuthorizedSettingsChanged;

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
                if (subscriptionLists != null) {
                    Debug.Log("Channel Subscription lists: " + string.Join(", ", subscriptionLists));
                }
            },
            onError: (error) => {
                Debug.LogError("Error getting channel subscription lists: " + error.Message);
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
        StartCoroutine(Airship.Shared.contact.GetNamedUserId(
            onComplete: (namedUserId) => {
                Debug.Log("Named user ID: " + namedUserId);
            },
            onError: (error) => {
                Debug.LogError("Error getting named user ID: " + error.Message);
            }
        ));
        // Airship.Shared.contact.Reset();
        // StartCoroutine(Airship.Shared.contact.GetNamedUserId(
        //     onComplete: (namedUserId) => {
        //         Debug.Log("Named user ID after reset: " + namedUserId);
        //     },
        //     onError: (error) => {
        //         Debug.LogError("Error getting named user ID: " + error.Message);
        //     }
        // ));

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
                if (subscriptionLists != null) {
                    Debug.Log("Contact Subscription lists:");
                    foreach (var subscription in subscriptionLists) {
                        Debug.Log($"List: {subscription.Key}, Scopes: {string.Join(", ", subscription.Value)}");
                    }
                }
            },
            onError: (error) => {
                Debug.LogError("Error getting contact subscription lists: " + error.Message);
            }
        ));

        // InApp
        Airship.Shared.inApp.SetPaused(true);
        Debug.Log("InApp paused after true: " + Airship.Shared.inApp.IsPaused());
        Airship.Shared.inApp.SetPaused(false);
        Debug.Log("InApp paused after false: " + Airship.Shared.inApp.IsPaused());

        StartCoroutine(Airship.Shared.inApp.SetDisplayInterval(TimeSpan.FromSeconds(10),
            onComplete: () => {
                Debug.Log("InApp display interval: " + Airship.Shared.inApp.GetDisplayInterval());
            },
            onError: (error) => {
                Debug.LogError("Error setting display interval: " + error.Message);
            }
        ));

#if UNITY_ANDROID
        string localeOverride = "en-US";
#else
        string localeOverride = "en_US";
#endif
        Airship.Shared.locale.SetLocaleOverride(localeOverride);
        Debug.Log("Locale: " + Airship.Shared.locale.GetLocale());
        Airship.Shared.locale.ClearLocaleOverride();
        Debug.Log("Locale: " + Airship.Shared.locale.GetLocale());

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
                foreach (var message in messages) {
                    Debug.Log(message.title);
                }
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
        StartCoroutine(Airship.Shared.preferenceCenter.GetConfig("neat",
            onComplete: (config) => {
                Debug.Log("Config: " + JsonUtility.ToJson(config));
            },
            onError: (error) => {
                Debug.LogError("Error getting config: " + error.Message);
            }
        ));
        // Airship.Shared.preferenceCenter.Display("neat");

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

        StartCoroutine(Airship.Shared.push.GetActiveNotifications(
            onComplete: (notifications) => {
                Debug.Log("Active notifications: " + string.Join(", ", notifications));
            },
            onError: (error) => {
                Debug.LogError("Error getting active notifications: " + error.Message);
            }
        ));

        Airship.Shared.push.ClearNotifications();   
        Debug.Log("Notifications cleared");


        // Android Push methods
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

        // iOS Push methods
        Airship.Shared.push.iOS.SetForegroundPresentationOptions(new ForegroundPresentationOption[] {
            ForegroundPresentationOption.Sound,
            ForegroundPresentationOption.Badge,
            ForegroundPresentationOption.Banner,
            ForegroundPresentationOption.List,
        });

        // SetBadgeNumber is a coroutine, so it has to be started -- calling it plainly
        // builds the enumerator and never runs it -- and the badge has only been applied
        // by the time onComplete fires.
        StartCoroutine(Airship.Shared.push.iOS.SetBadgeNumber(1, onComplete: () => {
            Debug.Log("Badge number: " + Airship.Shared.push.iOS.GetBadgeNumber());

            StartCoroutine(Airship.Shared.push.iOS.SetBadgeNumber(0, onComplete: () => {
                Debug.Log("Badge number: " + Airship.Shared.push.iOS.GetBadgeNumber());
            }));
        }));
        
        Airship.Shared.push.iOS.SetQuietTimeEnabled(true);
        Debug.Log("Quiet time enabled: " + Airship.Shared.push.iOS.IsQuietTimeEnabled());
        Airship.Shared.push.iOS.SetQuietTimeEnabled(false);
        Debug.Log("Quiet time enabled: " + Airship.Shared.push.iOS.IsQuietTimeEnabled());
        Airship.Shared.push.iOS.SetQuietTimeEnabled(true);
        Airship.Shared.push.iOS.SetQuietTime(new QuietTime() {
            startHour = 10,
            startMinute = 0,
            endHour = 18,
            endMinute = 0,
        });
        Debug.Log("Quiet time: " + Airship.Shared.push.iOS.GetQuietTime());

        Debug.Log("Authorized notification settings: " + string.Join(", ", Airship.Shared.push.iOS.GetAuthorizedNotificationSettings()));
        Debug.Log("Authorized notification status: " + Airship.Shared.push.iOS.GetAuthorizedNotificationStatus());


        StartCoroutine(Airship.Shared.actions.RunAction("test_action", "test_value",
            onComplete: (result) => {
                Debug.Log("Action result: " + result);
            },
            onError: (error) => {
                Debug.LogError("Error running action: " + error.Message);
            }
        ));

        StartCoroutine(Airship.Shared.featureFlagManager.Flag("feature_flag",
            onComplete: (flag) => {
                Debug.Log("Feature flag: " + flag);

                Airship.Shared.featureFlagManager.TrackInteraction(flag);
            },
            onError: (error) => {
                Debug.LogError("Error getting feature flag: " + error.Message);
            }
        ));

        // Live Update (Android only)
        // Debug.Log("Start Live Update");
        // Airship.Shared.liveUpdateManager.Start(new LiveUpdateStartRequest() {
        //     name = "Emoji-example",
        //     type = "Example",
        //     content = new Dictionary<string, object> {
        //         ["status_update"] = "Unity test started!",
        //         ["emoji"] = "🏆"
        //     }
        // });

        // Debug.Log("List Live Updates");
        // StartCoroutine(Airship.Shared.liveUpdateManager.List(
        //     new LiveUpdateListRequest() { type = "Example" },
        //     onComplete: (liveUpdates) => {
        //         Debug.Log("Live Updates count: " + liveUpdates.Length);
        //         foreach (var lu in liveUpdates) {
        //             Debug.Log($"Live Update: name={lu.name}, type={lu.type}, content={lu.Content}");
        //         }
        //     },
        //     onError: (error) => {
        //         Debug.LogError("Error listing live updates: " + error.Message);
        //     }
        // ));

        // Live Activity (iOS only)
        // Debug.Log("Start Live Activity");
        // StartCoroutine(Airship.Shared.liveActivityManager.Start(
        //     new LiveActivityStartRequest() {
        //         attributesType = "LiveActivityExampleAttributes",
        //         content = new LiveActivityContent() {
        //             state = new Dictionary<string, object> {
        //                 ["emoji"] = "🏆"
        //             },
        //             relevanceScore = 100
        //         },
        //         attributes = new Dictionary<string, object> {
        //             ["name"] = "Unity Test"
        //         }
        //     },
        //     onComplete: (activity) => {
        //         Debug.Log($"Live Activity started: id={activity.id}, state={activity.state}");
        //     },
        //     onError: (error) => {
        //         Debug.LogError("Error starting live activity: " + error.Message);
        //     }
        // ));
    }

    void OnDestroy () {
        Airship.Shared.OnPushReceived -= OnPushReceived;
        Airship.Shared.OnPushOpened -= OnPushOpened;
        Airship.Shared.OnChannelCreated -= OnChannelCreated;
        Airship.Shared.OnDeepLinkReceived -= OnDeepLinkReceived;
        Airship.Shared.OnInboxUpdated -= OnInboxUpdated;
        Airship.Shared.OnShowInbox -= OnShowInbox;
        Airship.Shared.OnPreferenceCenterDisplay -= OnPreferenceCenterDisplay;
        Airship.Shared.OnPushTokenReceived -= OnPushTokenReceived;
        Airship.Shared.OnNotificationStatusChanged -= OnNotificationStatusChanged;
        Airship.Shared.OnAuthorizedSettingsChanged -= OnAuthorizedSettingsChanged;
    }

    void OnPushReceived (PushMessage message) {
        Debug.Log ("Listener: Received push! " + message.Alert);

        if (message.Extras != null) {
            foreach (KeyValuePair<string, string> kvp in message.Extras) {
                Debug.Log (string.Format ("Extras Key = {0}, Value = {1}", kvp.Key, kvp.Value));
            }
        }
    }

    void OnPushOpened (PushMessage message) {
        Debug.Log ("Listener: Opened Push! " + message.Alert);

        if (message.Extras != null) {
            foreach (KeyValuePair<string, string> kvp in message.Extras) {
                Debug.Log (string.Format ("Extras Key = {0}, Value = {1}", kvp.Key, kvp.Value));
            }
        }
    }

    void OnChannelCreated (string channelId) {
        Debug.Log ("Listener: Channel created: " + channelId);
    }

    void OnDeepLinkReceived (string deeplink) {
        Debug.Log ("Listener: Received deep link: " + deeplink);
    }

    void OnInboxUpdated (uint messageUnreadCount, uint messageCount)
    {
        Debug.Log("Listener: Inbox updated - unread messages: " + messageUnreadCount + " total messages: " + messageCount);
    }

    void OnShowInbox (string messageId)
    {
        if (messageId == null)
        {
            Debug.Log("Listener: OnShowInbox - show inbox");
        }
        else
        {
            Debug.Log("Listener: OnShowInbox - show message: messageId = " + messageId);
        }
    }

    void OnPreferenceCenterDisplay (string preferenceCenterId) {
        Debug.Log ("Listener: Preference Center display - preferenceCenterId: " + preferenceCenterId);
    }

    void OnPushTokenReceived (string pushToken) {
        Debug.Log ("Listener: Push token received: " + pushToken);
    }

    void OnNotificationStatusChanged (PushNotificationStatus status) {
        Debug.Log ("Listener: Notification status changed: " + status);
    }

    void OnAuthorizedSettingsChanged (AuthorizedNotificationSetting[] authorizedSettings) {
        Debug.Log ("Listener: Authorized settings changed: " + JsonUtility.ToJson(authorizedSettings));
    }
}
