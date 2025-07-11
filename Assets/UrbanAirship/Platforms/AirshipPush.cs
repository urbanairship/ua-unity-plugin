/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship
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
        public readonly AirshipPushIOS iOS;

        /// <summary>
        /// Android only push methods.
        /// </summary>
        public readonly AirshipPushAndroid android;

        internal AirshipPush(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
            iOS = new AirshipPushIOS(plugin);
            android = new AirshipPushAndroid(plugin);
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
        /// Enables user notifications.
        /// </summary>
        /// <param name="fallback">Optional fallback.</param>
        /// <returns>The permission result.</returns>
        public bool EnableUserNotifications(PromptPermissionFallback? fallback)
        {
            return plugin.Call<bool>("enableUserNotifications", fallback);
        }

        /// <summary>
        /// Gets the notification status.
        /// </summary>
        /// <returns>The notification status.</returns>
        public PushNotificationStatus GetNotificationStatus()
        {
            // TODO parse this and return a PushNotificationStatus
            plugin.Call<string>("getNotificationStatus");
            return;
        }

        /// <summary>
        /// Gets the registration token if generated.
        /// </summary>
        /// <returns>The push token.</returns>
        public string GetPushToken()
        {
            return plugin.Call<string>("getPushToken");
        }

        /// <summary>
        /// Gets the list of active notifications.
        /// 
        /// On Android, this list only includes notifications sent through Airship.
        /// </summary>
        /// <returns>The list of active notifications.</returns>
        public IEnumerable<PushMessage> GetActiveNotifications()
        {
            string jsonPushMessages = plugin.Call<string>("getActiveNotifications");
            if (String.IsNullOrEmpty(jsonPushMessages))
            {
                return null;
            }

            var pushMessages = new List<PushMessage>();
            foreach (string pushMessageAsJson in JsonArray<string>.FromJson(pushMessagesAsJson).values)
            {
                pushMessages.Add(PushMessage.FromJson(pushMessageAsJson));
            }

            return pushMessages;
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

    /// <summary>
    /// IOS Push.
    /// </summary>
    public class AirshipPushIOS
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
        public void SetForegroundPresentationOptions(ForegroundPresentationOption options)
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
        /// Set the badge number.
        /// </summary>
        /// <param name="badge">The badge number.</param>
        public void SetBadgeNumber(int badge)
        {
            plugin.Call("setBadgeNumber", badge);
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
    }

    /// <summary>
    /// Android Push.
    /// </summary>
    public class AirshipPushAndroid
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
        public void SetNotificationConfig(NotificationConfig config)
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

        /// <summary>
        /// The notification permission status.
        /// </summary>
        public PermissionStatus notificationPermissionStatus;
    }

    /// <summary>
    /// Enum of permission status.
    /// </summary>
    [Serializable]
    public enum PermissionStatus
    {
        // Permission is granted.
        Granted = "granted",
        // Permission is denied.
        Denied = "denied",
        // Permission has not yet been requested.
        NotDetermined = "not_determined",
    }

    /// <summary>
    /// Fallback when prompting for permission and the permission is
    /// already denied on iOS or is denied silently on Android.
    /// </summary>
    [Serializable]
    public enum PromptPermissionFallback
    {
        // Take the user to the system settings to enable the permission.
        SystemSettings = "systemSettings"
    }

    /// <summary>
    /// Enum of foreground notification options.
    /// </summary>
    [Serializable]
    public enum ForegroundPresentationOption
    {
        // Play the sound associated with the notification.
        Sound = "sound",

        // Apply the notification's badge value to the app’s icon.
        Badge = "badge",

        // Show the notification in Notification Center. On iOS 13 an older,
        // this will also show the notification as a banner.
        List = "list",

        // Present the notification as a banner. On iOS 13 an older,
        // this will also show the notification in the Notification Center.
        Banner = "banner",
    }

    /// <summary>
    /// Enum of notification options. iOS only.
    /// </summary>
    [Serializable]
    public enum NotificationOption
    {
        // Alerts.
        Alert = "alert",
        // Sounds.
        Sound = "sound",
        // Badges.
        Badge = "badge",
        // Car play.
        CarPlay = "car_play",
        // Critical Alert.
        CriticalAlert = "critical_alert",
        // Provides app notification settings.
        ProvidesAppNotificationSettings = "provides_app_notification_settings",
        // Provisional.
        Provisional = "provisional"
    }

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
    public enum AuthorizedNotificationSetting
    {
        // Alerts.
        Alert = "alert",
        // Sounds.
        Sound = "sound",
        // Badges.
        Badge = "badge",
        // CarPlay.
        CarPlay = "car_play",
        // Lock screen.
        LockScreen = "lock_screen",
        // Notification center.
        NotificationCenter = "notification_center",
        // Critical alert.
        CriticalAlert = "critical_alert",
        // Announcement.
        Announcement = "announcement",
        // Scheduled delivery.
        ScheduledDelivery = "scheduled_delivery",
        // Time sensitive.
        TimeSensitive = "time_sensitive",
    }

    public record NotificationConfig
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
}