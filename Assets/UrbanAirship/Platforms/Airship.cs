/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanAirship
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

        public AirshipChannel channel;
        public AirshipContact contact;
        public AirshipAnalytics analytics;
        public AirshipInApp inApp;
        public AirshipPush push;
        public AirshipMessageCenter messageCenter;
        public AirshipPreferenceCenter preferenceCenter;
        public AirshipPrivacyManager privacyManager;
        public AirshipLocale locale;

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
                plugin = new StubbedAirshipPlugin();
            }
            else
            {
#if UNITY_ANDROID
                plugin = new AirshipPluginAndroid ();
#elif UNITY_IOS
                plugin = new AirshipPluginiOS ();
#else
                plugin = new StubbedAirshipPlugin();
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
            // TODO finish the rest of the modules

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

        // TODO don't forget live activity and live update.

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
                DeepLinkReceivedEventHandler handler = Shared.OnDeepLinkReceived;

                if (handler != null)
                {
                    handler(deeplink);
                }
            }

            void OnChannelCreated(string channelId)
            {
                ChannelCreateEventHandler handler = Shared.OnChannelCreated;

                if (handler != null)
                {
                    handler(channelId);
                }
            }

            void OnInboxUpdated(string counts)
            {
                InboxUpdatedEventHandler handler = Shared.OnInboxUpdated;

                MessageCounts messageCounts = JsonUtility.FromJson<MessageCounts>(counts);

                if (handler != null)
                {
                    handler(messageCounts.unread, messageCounts.total);
                }

            }

            void OnShowInbox(string messageId)
            {
                ShowInboxEventHandler handler = Shared.OnShowInbox;

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
                PreferenceCenterDisplayEventHandler handler = Shared.OnPreferenceCenterDisplay;

                if (handler != null)
                {
                    handler(preferenceCenterId);
                }
            }

            void OnAuthorizedSettingsChanged(AuthorizedNotificationSetting[] authorizedSettings)
            {
                AuthorizedSettingsChangedEventHandler handler = Shared.OnAuthorizedSettingsChanged;

                if (handler != null)
                {
                    handler(authorizedSettings);
                }
            }
        }
    }

    public static class Features
    {
        public const string FEATURE_NONE = "FEATURE_NONE";
        public const string FEATURE_IN_APP_AUTOMATION = "FEATURE_IN_APP_AUTOMATION";
        public const string FEATURE_MESSAGE_CENTER = "FEATURE_MESSAGE_CENTER";
        public const string FEATURE_PUSH = "FEATURE_PUSH";
        public const string FEATURE_ANALYTICS = "FEATURE_ANALYTICS";
        public const string FEATURE_TAGS_AND_ATTRIBUTES = "FEATURE_TAGS_AND_ATTRIBUTES";
        public const string FEATURE_CONTACTS = "FEATURE_CONTACTS";
        public const string FEATURE_ALL = "FEATURE_ALL";
    }

    /// <summary>
    /// Airship config environment
    /// </summary>
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

    public record IOSEnvironmentConfig
    {
        /// <summary>
        /// Log privacy level. By default it logs at `private`, not logging anything lower than info to the console
        /// and redacting logs with string interpolation. `public` will log all configured log levels to the console
        /// without redacting any of the log lines.
        /// </summary>
        public LogPrivacyLevel? logPrivacyLevel;
    }

    public enum LogPrivacyLevel
    {
        [AirshipEnumStringValue("private")]
        Private,
        [AirshipEnumStringValue("public")]
        Public
    }

    public enum Site
    {
        [AirshipEnumStringValue("us")]
        US,
        [AirshipEnumStringValue("eu")]
        EU
    }

    public record IOSConfig
    {
        // itunesId for rate app and app store deep links.
        public string? itunesId;
    }

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

    public record AirshipConfig
        {
            // Default environment.
            public ConfigEnvironment? defaultEnvironment;

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