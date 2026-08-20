/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable annotations

namespace AirshipSDK
{

    /// <summary>
    /// Airship Push.
    /// </summary>
    public class AirshipPush
    {
        private IAirshipPlugin plugin;

        /// <summary>
        /// iOS only push methods.
        /// </summary>
        public readonly IAirshipPushIOS iOS;

        /// <summary>
        /// Android only push methods.
        /// </summary>
        public readonly IAirshipPushAndroid android;

        internal AirshipPush(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
#if UNITY_IOS
            iOS = new AirshipPushIOS (plugin);
#else
            iOS = new StubbedAirshipPushIOS ();
#endif
#if UNITY_ANDROID
            android = new AirshipPushAndroid (plugin);
#else
            android = new StubbedAirshipPushAndroid ();
#endif
        }

        /// <summary>
        /// Enables/disables notifications on Airship.
        /// 
        /// When enabled, it will cause the user to be prompted for
        /// the permission on platforms that support it.
        /// To get the result of the prompt, use `enableUserNotifications`.
        /// </summary>
        /// <param name="enabled">true to enable, false to disable.</param>
        public void SetUserNotificationsEnabled(bool enabled)
        {
            plugin.Call("setUserNotificationsEnabled", enabled);
        }

        /// <summary>
        /// Checks if user notifications are enabled or not on Airship.
        /// </summary>
        /// <returns>true if user notifications are enabled, otherwise false.</returns>
        public bool IsUserNotificationEnabled()
        {
            return plugin.Call<bool>("isUserNotificationsEnabled");
        }

        /// <summary>
        /// Enables user notifications asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="fallback">Optional fallback.</param>
        /// <param name="onComplete">Callback invoked with the permission result when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator EnableUserNotifications(EnabledUserPushNotificationsArgs? fallback, Action<bool> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    return plugin.Call<bool>("enableUserNotifications", fallback);
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Gets the notification status asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="onComplete">Callback invoked with the notification status when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator GetNotificationStatus(Action<PushNotificationStatus> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    string statusJson = plugin.Call<string>("getNotificationStatus");
                    if (String.IsNullOrEmpty(statusJson))
                    {
                        throw new Exception("Airship: empty response from getNotificationStatus");
                    }
                    return JsonUtility.FromJson<PushNotificationStatus>(statusJson);
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Gets the registration token if generated.
        /// </summary>
        /// <returns>The push token.</returns>
        public string? GetPushToken()
        {
            return plugin.Call<string?>("getPushToken");
        }

        /// <summary>
        /// Gets the list of active notifications asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// 
        /// On Android, this list only includes notifications sent through Airship.
        /// </summary>
        /// <param name="onComplete">Callback invoked with the list of active notifications when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator GetActiveNotifications(Action<IEnumerable<PushMessage>> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
                    string jsonPushMessages = plugin.Call<string>("getActiveNotifications");
                    if (String.IsNullOrEmpty(jsonPushMessages))
                    {
                        return (IEnumerable<PushMessage>)new List<PushMessage>();
                    }

                    var pushMessages = new List<PushMessage>();
                    PushMessage[] parsedMessages = JsonArray<PushMessage>.FromJson(jsonPushMessages).values;
                    if (parsedMessages != null)
                    {
                        foreach (PushMessage pushMessage in parsedMessages)
                        {
                            if (pushMessage != null)
                            {
                                pushMessages.Add(pushMessage);
                            }
                        }
                    }

                    return (IEnumerable<PushMessage>)pushMessages;
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Clears all notifications for the app.
        /// </summary>
        public void ClearNotifications()
        {
            plugin.Call("clearNotifications");
        }

        /// <summary>
        /// Clears a specific notification.
        /// 
        /// On Android, you can use this method to clear notifications outside of Airship.
        /// The identifier is in the format of <tag>:<id>.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        public void ClearNotification(string identifier)
        {
            plugin.Call("clearNotification", identifier);
        }
    }

    public interface IAirshipPushIOS {
        void SetForegroundPresentationOptions(ForegroundPresentationOption[] options);
        void SetNotificationOptions(NotificationOption[] options);
        bool IsAutobadgeEnabled();
        void SetAutobadgeEnabled(bool enabled);
        IEnumerator SetBadgeNumber(int badge, Action onComplete = null, Action<Exception> onError = null);
        int GetBadgeNumber();
        void SetQuietTimeEnabled(bool enabled);
        bool IsQuietTimeEnabled();
        void SetQuietTime(QuietTime quietTime);
        QuietTime? GetQuietTime();

        AuthorizedNotificationSetting[] GetAuthorizedNotificationSettings();
        AuthorizedNotificationStatus GetAuthorizedNotificationStatus();
    }

    internal class StubbedAirshipPushIOS : IAirshipPushIOS {
        public void SetForegroundPresentationOptions(ForegroundPresentationOption[] options) {}
        public void SetNotificationOptions(NotificationOption[] options) {}
        public bool IsAutobadgeEnabled() { return false; }
        public void SetAutobadgeEnabled(bool enabled) {}
        public IEnumerator SetBadgeNumber(int badge, Action onComplete = null, Action<Exception> onError = null) { yield break; }
        public int GetBadgeNumber() { return 0; }
        public void SetQuietTimeEnabled(bool enabled) {}
        public bool IsQuietTimeEnabled() { return false; }
        public void SetQuietTime(QuietTime quietTime) {}
        public QuietTime? GetQuietTime() { return null; }
        public AuthorizedNotificationSetting[] GetAuthorizedNotificationSettings() { return new AuthorizedNotificationSetting[0]; }
        public AuthorizedNotificationStatus GetAuthorizedNotificationStatus() { return AuthorizedNotificationStatus.NotDetermined; }
    }

    /// <summary>
    /// IOS Push.
    /// </summary>
    public class AirshipPushIOS : IAirshipPushIOS
    {
        private IAirshipPlugin plugin;

        internal AirshipPushIOS(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Sets the foreground presentation options.
        /// </summary>
        /// <param name="options">The foreground options.</param>
        public void SetForegroundPresentationOptions(ForegroundPresentationOption[] options)
        {
            plugin.Call("setForegroundPresentationOptions", options);
        }

        /// <summary>
        /// Sets the notification options.
        /// </summary>
        /// <param name="options">The notification options.</param>
        public void SetNotificationOptions(NotificationOption[] options)
        {
            plugin.Call("setNotificationOptions", options);
        }

        /// <summary>
        /// Checks if autobadge is enabled.
        /// </summary>
        /// <returns>true if autobadge is enabled, otherwise false.</returns>
        public bool IsAutobadgeEnabled()
        {
            return plugin.Call<bool>("isAutobadgeEnabled");
        }

        /// <summary>
        /// Enables/disables autobadge.
        /// </summary>
        /// <param name="enabled">true to enable, false to disable.</param>
        public void SetAutobadgeEnabled(bool enabled)
        {
            plugin.Call("setAutobadgeEnabled", enabled);
        }

        /// <summary>
        /// Sets the badge number asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="badge">The badge number.</param>
        /// <param name="onComplete">Optional callback invoked when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator SetBadgeNumber(int badge, Action onComplete = null, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => plugin.Call("setBadgeNumber", badge),
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Gets the badge number.
        /// </summary>
        /// <returns>The badge number.</returns>
        public int GetBadgeNumber()
        {
            return plugin.Call<int>("getBadgeNumber");
        }

        /// <summary>
        /// Enables/disables quiet time.
        /// </summary>
        /// <param name="enabled">true to enable, false to disable.</param>
        public void SetQuietTimeEnabled(bool enabled)
        {
            plugin.Call("setQuietTimeEnabled", enabled);
        }

        /// <summary>
        /// Checks if quiet time is enabled or not.
        /// </summary>
        /// <returns>true if quiet time is enabled, otherwise false.</returns>
        public bool IsQuietTimeEnabled()
        {
            return plugin.Call<bool>("isQuietTimeEnabled");
        }

        /// <summary>
        /// Sets quiet time.
        /// </summary>
        /// <param name="quietTime">The quiet time.</param>
        public void SetQuietTime(QuietTime quietTime)
        {
            plugin.Call("setQuietTime", quietTime);
        }

        /// <summary>
        /// Gets the quiet time settings.
        /// </summary>
        /// <returns>The quiet time.</returns>
        public QuietTime? GetQuietTime()
        {
            return plugin.Call<QuietTime?>("getQuietTime");
        }

        public AuthorizedNotificationSetting[] GetAuthorizedNotificationSettings()
        {
            return plugin.Call<AuthorizedNotificationSetting[]>("getAuthorizedNotificationSettings");
        }

        public AuthorizedNotificationStatus GetAuthorizedNotificationStatus()
        {
            return plugin.Call<AuthorizedNotificationStatus>("getAuthorizedNotificationStatus");
        }
    }

    public interface IAirshipPushAndroid {
        bool IsNotificationChannelEnabled(string channel);
        void SetNotificationConfig(AndroidNotificationConfig config);
        void SetForegroundNotificationsEnabled(bool enabled);
        bool IsForegroundNotificationsEnabled();
    }

    internal class StubbedAirshipPushAndroid : IAirshipPushAndroid {
        public bool IsNotificationChannelEnabled(string channel) { return false; }
        public void SetNotificationConfig(AndroidNotificationConfig config) {}
        public void SetForegroundNotificationsEnabled(bool enabled) {}
        public bool IsForegroundNotificationsEnabled() { return false; }
    }

    /// <summary>
    /// Android Push.
    /// </summary>
    public class AirshipPushAndroid : IAirshipPushAndroid
    {
        private IAirshipPlugin plugin;

        internal AirshipPushAndroid(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Checks if a notification category/channel is enabled.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        /// <returns>true if the channel is enabled, otherwise false.</returns>
        public bool IsNotificationChannelEnabled(string channel)
        {
            return plugin.Call<bool>("isNotificationChannelEnabled", channel);
        }

        /// <summary>
        /// Sets the notification config.
        /// </summary>
        /// <param name="config">The notification config.</param>
        public void SetNotificationConfig(AndroidNotificationConfig config)
        {
            plugin.Call("setNotificationConfig", config);
        }

        /// <summary>
        /// Enables/disables showing notifications in the foreground.
        /// </summary>
        /// <param name="enabled">true to enable, false to disable.</param>
        public void SetForegroundNotificationsEnabled(bool enabled)
        {
            plugin.Call("setForegroundNotificationsEnabled", enabled);
        }

        /// <summary>
        /// Checks if notifications show in the foreground.
        /// </summary>
        /// <returns>true if notifications show in the foreground, otherwise false.</returns>
        public bool IsForegroundNotificationsEnabled()
        {
            return plugin.Call<bool>("isForegroundNotificationsEnabled");
        }
    }

    /// <summary>
    /// Push notification status object.
    /// </summary>
    [Serializable]
    public record PushNotificationStatus
    {
        /// <summary>
        /// If user notifications are enabled.
        /// </summary>
        public bool isUserNotificationsEnabled;

        /// <summary>
        /// If notifications are allowed at the system level for the application.
        /// </summary>
        public bool areNotificationsAllowed;

        /// <summary>
        /// If the push feature is enabled on PrivacyManager.
        /// </summary>
        public bool isPushPrivacyFeatureEnabled;

        /// <summary>
        /// If push registration was able to generate a token.
        /// </summary>
        public bool isPushTokenRegistered;

        /// <summary>
        /// If Airship is able to send and display a push notification.
        /// </summary>
        public bool isOptedIn;

        /// <summary>
        /// Checks for isUserNotificationsEnabled, areNotificationsAllowed, and isPushPrivacyFeatureEnabled.
        /// If this flag is true but `isOptedIn` is false, that means push token was not able to be registered.
        /// </summary>
        public bool isUserOptedIn;

        // Both natives send this as a string ("granted" / "denied" / "not_determined").
        // Unity's JsonUtility only maps enums from integers, so the raw string is captured
        // here and parsed by the property below. Reading it as an enum field silently
        // yielded PermissionStatus.Granted (value 0) for every status.
        [SerializeField]
        private string notificationPermissionStatus;

        /// <summary>
        /// The notification permission status.
        /// </summary>
        public PermissionStatus NotificationPermissionStatus {
            get { return AirshipUtils.ParseEnum(notificationPermissionStatus, PermissionStatus.NotDetermined); }
        }
    }

    /// <summary>
    /// Enum of permission status.
    /// </summary>
    [Serializable]
    public enum PermissionStatus
    {
        // Permission is granted.
        [AirshipEnumStringValue("granted")]
        Granted,
        // Permission is denied.
        [AirshipEnumStringValue("denied")]
        Denied,
        // Permission has not yet been requested.
        [AirshipEnumStringValue("not_determined")]
        NotDetermined,
    }

    /// <summary>
    /// Fallback when prompting for permission and the permission is
    /// already denied on iOS or is denied silently on Android.
    /// </summary>
    [Serializable]
    public enum PromptPermissionFallback
    {
        // Take the user to the system settings to enable the permission.
        [AirshipEnumStringValue("systemSettings")]
        SystemSettings
    }

    [Serializable]
    public record EnabledUserPushNotificationsArgs {
        public PromptPermissionFallback? fallback;
    }

    /// <summary>
    /// Enum of foreground notification options.
    /// </summary>
    [Serializable]
    public enum ForegroundPresentationOption
    {
        // Play the sound associated with the notification.
        [AirshipEnumStringValue("sound")]
        Sound,

        // Apply the notification's badge value to the app’s icon.
        [AirshipEnumStringValue("badge")]
        Badge,

        // Show the notification in Notification Center. On iOS 13 an older,
        // this will also show the notification as a banner.
        [AirshipEnumStringValue("list")]
        List,

        // Present the notification as a banner. On iOS 13 an older,
        // this will also show the notification in the Notification Center.
        [AirshipEnumStringValue("banner")]
        Banner,
    }

    /// <summary>
    /// Enum of notification options. iOS only.
    /// </summary>
    [Serializable]
    public enum NotificationOption
    {
        // Alerts.
        [AirshipEnumStringValue("alert")]
        Alert,
        // Sounds.
        [AirshipEnumStringValue("sound")]
        Sound,
        // Badges.
        [AirshipEnumStringValue("badge")]
        Badge,
        // Car play.
        [AirshipEnumStringValue("car_play")]
        CarPlay,
        // Critical Alert.
        [AirshipEnumStringValue("critical_alert")]
        CriticalAlert,
        // Provides app notification settings.
        [AirshipEnumStringValue("provides_app_notification_settings")]
        ProvidesAppNotificationSettings,
        // Provisional.
        [AirshipEnumStringValue("provisional")]
        Provisional
    }

    [Serializable]
    public record QuietTime
    {
        // Start hour. Must be 0-23.
        public int startHour;

        // Start minute. Must be 0-59.
        public int startMinute;

        // End hour. Must be 0-23.
        public int endHour;

        // End minute. Must be 0-59.
        public int endMinute;
    }

    /// <summary>
    /// Enum of authorized notification options.
    /// </summary>
    [Serializable]
    public enum AuthorizedNotificationSetting
    {
        // Alerts.
        [AirshipEnumStringValue("alert")]
        Alert,
        // Sounds.
        [AirshipEnumStringValue("sound")]
        Sound,
        // Badges.
        [AirshipEnumStringValue("badge")]
        Badge,
        // CarPlay.
        [AirshipEnumStringValue("car_play")]
        CarPlay,
        // Lock screen.
        [AirshipEnumStringValue("lock_screen")]
        LockScreen,
        // Notification center.
        [AirshipEnumStringValue("notification_center")]
        NotificationCenter,
        // Critical alert.
        [AirshipEnumStringValue("critical_alert")]
        CriticalAlert,
        // Announcement.
        [AirshipEnumStringValue("announcement")]
        Announcement,
        // Scheduled delivery.
        [AirshipEnumStringValue("scheduled_delivery")]
        ScheduledDelivery,
        // Time sensitive.
        [AirshipEnumStringValue("time_sensitive")]
        TimeSensitive,
    }

    /// <summary>
    /// Enum of authorized notification status.
    /// </summary>
    [Serializable]
    public enum AuthorizedNotificationStatus
    {
        // Not determined.
        [AirshipEnumStringValue("not_determined")]
        NotDetermined,
        // Denied.
        [AirshipEnumStringValue("denied")]
        Denied,
        // Authorized.
        [AirshipEnumStringValue("authorized")]
        Authorized,
        // Provisional.
        [AirshipEnumStringValue("provisional")]
        Provisional,
        // Ephemeral.
        [AirshipEnumStringValue("ephemeral")]
        Ephemeral,
    }

}