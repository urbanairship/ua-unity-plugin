/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable annotations

namespace AirshipSDK
{

    public class Airship
    {
        /// <summary>
        /// Push received event handler.
        /// </summary>
        public delegate void PushReceivedEventHandler(PushMessage message);

        /// <summary>
        /// Occurs when a push is received.
        /// </summary>
        public event PushReceivedEventHandler OnPushReceived;

        /// <summary>
        /// Push opened event handler.
        /// </summary>
        public delegate void PushOpenedEventHandler(PushMessage message);

        /// <summary>
        /// Occurs when a push is opened.
        /// </summary>
        public event PushOpenedEventHandler OnPushOpened;

        /// <summary>
        /// Deep link received event handler.
        /// </summary>
        public delegate void DeepLinkReceivedEventHandler(string deeplink);

        /// <summary>
        /// Occurs when a deep link is received.
        /// </summary>
        public event DeepLinkReceivedEventHandler OnDeepLinkReceived;

        /// <summary>
        /// Inbox update event handler.
        /// </summary>
        public delegate void InboxUpdatedEventHandler(uint messageUnreadCount, uint messageCount);

        /// <summary>
        /// Occurs when the inbox updates.
        /// </summary>
        public event InboxUpdatedEventHandler OnInboxUpdated;

        /// <summary>
        /// Show inbox event handler.
        /// </summary>
        public delegate void ShowInboxEventHandler(string messageId);

        /// <summary>
        /// Occurs when the app needs to show the inbox.
        /// </summary>
        public event ShowInboxEventHandler OnShowInbox;

        /// <summary>
        /// Channel create event handler.
        /// </summary>
        public delegate void ChannelCreateEventHandler(string channelId);

        /// <summary>
        /// Occurs when the channel creates.
        /// </summary>
        public event ChannelCreateEventHandler OnChannelCreated;

        /// <summary>
        /// Preference Center display event handler.
        /// </summary>
        public delegate void PreferenceCenterDisplayEventHandler(string preferenceCenterId);

        /// <summary>
        /// Occurs when the app displays the preference center.
        /// </summary>
        public event PreferenceCenterDisplayEventHandler OnPreferenceCenterDisplay;

        /// <summary>
        /// Authorized settings changed event handler.
        /// </summary>
        public delegate void AuthorizedSettingsChangedEventHandler(AuthorizedNotificationSetting[] authorizedSettings);

        /// <summary>
        /// Occurs when the authorized settings changed.
        /// </summary>
        public event AuthorizedSettingsChangedEventHandler OnAuthorizedSettingsChanged;

        /// <summary>
        /// Push token received event handler.
        /// </summary>
        public delegate void PushTokenReceivedEventHandler(string pushToken);

        /// <summary>
        /// Occurs when the push token is received.
        /// </summary>
        public event PushTokenReceivedEventHandler OnPushTokenReceived;

        /// <summary>
        /// Notification status changed event handler.
        /// </summary>
        public delegate void NotificationStatusChangedEventHandler(PushNotificationStatus status);

        /// <summary>
        /// Occurs when the notification status changed.
        /// </summary>
        public event NotificationStatusChangedEventHandler OnNotificationStatusChanged;

        public AirshipChannel channel;
        public AirshipContact contact;
        public AirshipAnalytics analytics;
        public AirshipInApp inApp;
        public AirshipPush push;
        public AirshipMessageCenter messageCenter;
        public AirshipPreferenceCenter preferenceCenter;
        public AirshipPrivacyManager privacyManager;
        public AirshipLocale locale;
        public AirshipAction actions;
        public AirshipFeatureFlagManager featureFlagManager;
        public IAirshipLiveUpdateManager liveUpdateManager;
        public IAirshipLiveActivityManager liveActivityManager;

        private IAirshipPlugin plugin;
        internal GameObject gameObject;

        internal static Airship sharedAirship = new Airship();

        /// <summary>
        /// Gets the shared Airship instance.
        /// </summary>
        /// <value>The shared Airship instance.</value>
        public static Airship Shared
        {
            get
            {
                return sharedAirship;
            }
        }

        /// <summary>
        /// Creates a Airship instance with a test plugin.
        /// Used only for testing.
        /// </summary>
        /// <param name="testPlugin">The test plugin.</param>
        internal Airship(object testPlugin)
        {
            plugin = (IAirshipPlugin)testPlugin;

            Init();
        }

        /// <summary>
        /// Creates a Airship instance.
        /// </summary>]
        private Airship()
        {
            if (Application.isEditor)
            {
                plugin = new StubbedAirshipPlugin ();
            }
            else
            {
#if UNITY_ANDROID
                plugin = new AirshipPluginAndroid ();
#elif UNITY_IOS
                plugin = new AirshipPluginiOS ();
#else
                plugin = new StubbedAirshipPlugin ();
#endif
            }

            Init();
        }

        /// <summary>
        /// Initialize an Airship instance.
        /// </summary>]
        private void Init()
        {
            channel = new AirshipChannel(plugin);
            contact = new AirshipContact(plugin);
            analytics = new AirshipAnalytics(plugin);
            inApp = new AirshipInApp(plugin);
            push = new AirshipPush(plugin);
            messageCenter = new AirshipMessageCenter(plugin);
            preferenceCenter = new AirshipPreferenceCenter(plugin);
            privacyManager = new AirshipPrivacyManager(plugin);
            locale = new AirshipLocale(plugin);
            actions = new AirshipAction(plugin);
            featureFlagManager = new AirshipFeatureFlagManager(plugin);
#if UNITY_ANDROID
            liveUpdateManager = new AirshipLiveUpdateManager(plugin);
#else
            liveUpdateManager = new StubbedAirshipLiveUpdateManager();
#endif
#if UNITY_IOS
            liveActivityManager = new AirshipLiveActivityManager(plugin);
#else
            liveActivityManager = new StubbedAirshipLiveActivityManager();
#endif

            gameObject = new GameObject("[AirshipListener]");
            gameObject.AddComponent<AirshipListener>();

            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            plugin.Listener = gameObject;
        }

        /// <summary>
        /// Calls takeOff. If Airship is already configured for the app session,
        /// the new config will be applied on the next app init.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns>true if airship is ready.</returns>
        public bool TakeOff(AirshipConfig config)
        {
            return plugin.Call<bool>("takeOff", config);
        }

        /// <summary>
        /// Checks if Airship is ready.
        /// </summary>
        /// <returns>true is Airship is ready, otherwise false.</returns>
        public bool IsFlying()
        {
            return plugin.Call<bool>("isFlying");
        }

        internal class AirshipListener : MonoBehaviour
        {
            void OnPushReceived(string payload)
            {
                PushReceivedEventHandler handler = Airship.Shared.OnPushReceived;

                if (handler == null)
                {
                    return;
                }

                PushMessage pushMessage = PushMessage.FromJson(payload);
                if (pushMessage != null)
                {
                    handler(pushMessage);
                }
            }

            void OnPushOpened(string payload)
            {
                PushOpenedEventHandler handler = Airship.Shared.OnPushOpened;

                if (handler == null)
                {
                    return;
                }

                PushMessage pushMessage = PushMessage.FromJson(payload);
                if (pushMessage != null)
                {
                    handler(pushMessage);
                }
            }

            void OnDeepLinkReceived(string deeplink)
            {
                DeepLinkReceivedEventHandler handler = Airship.Shared.OnDeepLinkReceived;

                if (handler != null)
                {
                    handler(deeplink);
                }
            }

            void OnChannelCreated(string channelId)
            {
                ChannelCreateEventHandler handler = Airship.Shared.OnChannelCreated;

                if (handler != null)
                {
                    handler(channelId);
                }
            }

            void OnInboxUpdated(string counts)
            {
                InboxUpdatedEventHandler handler = Airship.Shared.OnInboxUpdated;

                MessageCounts messageCounts = JsonUtility.FromJson<MessageCounts>(counts);

                if (handler != null)
                {
                    handler(messageCounts.unread, messageCounts.total);
                }

            }

            void OnShowInbox(string messageId)
            {
                ShowInboxEventHandler handler = Airship.Shared.OnShowInbox;

                if (handler != null)
                {
                    if ((messageId == null) || (messageId.Length == 0))
                    {
                        handler(null);
                    }
                    else
                    {
                        handler(messageId);
                    }
                }
            }

            void OnPreferenceCenterDisplay(string preferenceCenterId)
            {
                PreferenceCenterDisplayEventHandler handler = Airship.Shared.OnPreferenceCenterDisplay;

                if (handler != null)
                {
                    handler(preferenceCenterId);
                }
            }

            void OnAuthorizedSettingsChanged(string authorizedSettings)
            {
                AuthorizedSettingsChangedEventHandler handler = Airship.Shared.OnAuthorizedSettingsChanged;

                if (handler != null)
                {
                    AuthorizedNotificationSetting[] authorizedSettingsArray = AirshipUtils.Deserialize<AuthorizedNotificationSetting[]>(authorizedSettings);
                    if (authorizedSettingsArray != null)
                    {
                        handler(authorizedSettingsArray);
                    }
                }
            }

            void OnPushTokenReceived(string pushToken)
            {
                PushTokenReceivedEventHandler handler = Airship.Shared.OnPushTokenReceived;

                if (handler != null)
                {
                    handler(pushToken);
                }
            }

            void OnNotificationStatusChanged(string status)
            {
                NotificationStatusChangedEventHandler handler = Airship.Shared.OnNotificationStatusChanged;

                if (handler != null)
                {
                    PushNotificationStatus pushStatus = JsonUtility.FromJson<PushNotificationStatus>(status);
                    if (pushStatus != null)
                    {
                        handler(pushStatus);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Airship config environment
    /// </summary>
    [Serializable]
    public record ConfigEnvironment
    {
        // App key.
        public string appKey;

        // App secret.
        public string appSecret;

        // Optional log level.
        public LogLevel? logLevel;

        // Optional iOS config
        public IOSEnvironmentConfig? iOS;
    }

    [Serializable]
    public enum LogLevel
    {
        [AirshipEnumStringValue("verbose")]
        Verbose,
        [AirshipEnumStringValue("debug")]
        Debug,
        [AirshipEnumStringValue("info")]
        Info,
        [AirshipEnumStringValue("warning")]
        Warning,
        [AirshipEnumStringValue("error")]
        Error,
        [AirshipEnumStringValue("none")]
        None
    }

    [Serializable]
    public record IOSEnvironmentConfig
    {
        /// <summary>
        /// Log privacy level. By default it logs at `private`, not logging anything lower than info to the console
        /// and redacting logs with string interpolation. `public` will log all configured log levels to the console
        /// without redacting any of the log lines.
        /// </summary>
        public LogPrivacyLevel? logPrivacyLevel;
    }

    [Serializable]
    public enum LogPrivacyLevel
    {
        [AirshipEnumStringValue("private")]
        Private,
        [AirshipEnumStringValue("public")]
        Public
    }

    [Serializable]
    public enum Site
    {
        [AirshipEnumStringValue("us")]
        US,
        [AirshipEnumStringValue("eu")]
        EU
    }

    [Serializable]
    public record IOSConfig
    {
        // itunesId for rate app and app store deep links.
        public string? itunesId;
    }

    [Serializable]
    public record AndroidConfig
    {
        // App store URI
        public string? appStoreUri;

        // Fcm app name if using multiple FCM projects.
        public string? fcmFirebaseAppName;

        // Notification config.
        public AndroidNotificationConfig? notificationConfig;

        // Log privacy level. By default it logs at `private`, not logging anything lower than info to the console 
        // and redacting logs with string interpolation. `public` will log all configured log levels to the console 
        // without redacting any of the log lines.
        public LogPrivacyLevel? logPrivacyLevel;
    }

    [Serializable]
    public record AndroidNotificationConfig
    {
        // The icon resource name.
        public string? icon;

        // The large icon resource name.
        public string? largeIcon;

        // The default android notification channel ID.
        public string? defaultChannelId;

        // The accent color. Must be a hex value #AARRGGBB.
        public string? accentColor;
    }

    [Serializable]
    public record AirshipConfig
    {
        // Default environment.
        public ConfigEnvironment? @default;

        // Development environment. Overrides default environment if inProduction is false.
        public ConfigEnvironment? development;

        // Production environment. Overrides default environment if inProduction is true.
        public ConfigEnvironment? production;

        // Cloud site.
        public Site? site;

        // Switches the environment from development or production.
        // If the value is not set, Airship will determine the value at runtime.
        public bool? inProduction;

        // URL allow list.
        public string[]? urlAllowList;

        // URL allow list for open URL scope.
        public string[]? urlAllowListScopeOpenUrl;

        // URL allow list for JS bridge injection.
        public string[]? urlAllowListScopeJavaScriptInterface;

        // Initial config URL for custom Airship domains.
        // The URL should also be added to the urlAllowList.
        public string? initialConfigUrl;

        // Enabled features. Defaults to all.
        public string[]? enabledFeatures;

        // Enables channel capture feature. This config is enabled by default.
        public bool? isChannelCaptureEnabled;

        // Whether to suppress console error messages about missing allow list entries during takeOff.
        // This config is disabled by default.
        public bool? suppressAllowListError;

        // Pauses In-App Automation on launch.
        public bool? autoPauseInAppAutomationOnLaunch;

        // iOS config.
        public IOSConfig? ios;

        // Android config.
        public AndroidConfig? android;
    }
}