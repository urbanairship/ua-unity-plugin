/*
 Copyright 2018 Urban Airship and Contributors
*/

package com.urbanairship.unityplugin;

import android.content.Context;
import android.preference.PreferenceManager;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.urbanairship.AirshipConfigOptions;
import com.urbanairship.Autopilot;
import com.urbanairship.UAirship;
import com.urbanairship.actions.DeepLinkListener;
import com.urbanairship.channel.AirshipChannelListener;
import com.urbanairship.messagecenter.MessageCenter;
import com.urbanairship.push.NotificationActionButtonInfo;
import com.urbanairship.push.NotificationInfo;
import com.urbanairship.push.NotificationListener;
import com.urbanairship.push.PushListener;
import com.urbanairship.push.PushMessage;
import com.urbanairship.richpush.RichPushInbox;

import static com.urbanairship.unityplugin.UnityPlugin.AUTO_LAUNCH_MESSAGE_CENTER;

public class UnityAutopilot extends Autopilot {

    @Override
    public void onAirshipReady(UAirship airship) {

        airship.getChannel().addChannelListener(new AirshipChannelListener() {
            @Override
            public void onChannelCreated(@NonNull String channelId) {
                UnityPlugin.shared().onChannelCreated(channelId);
            }

            @Override
            public void onChannelUpdated(@NonNull String channelId) {
                UnityPlugin.shared().onChannelUpdated(channelId);

            }
        });

        airship.getPushManager().addPushListener(new PushListener() {
            @Override
            public void onPushReceived(@NonNull PushMessage message, boolean notificationPosted) {
                UnityPlugin.shared().onPushReceived(message);
            }
        });

        airship.getPushManager().setNotificationListener(new NotificationListener() {
            @Override
            public void onNotificationPosted(@NonNull NotificationInfo notificationInfo) {

            }

            @Override
            public boolean onNotificationOpened(@NonNull NotificationInfo notificationInfo) {
                UnityPlugin.shared().onPushOpened(notificationInfo.getMessage());
                return false;
            }

            @Override
            public boolean onNotificationForegroundAction(@NonNull NotificationInfo notificationInfo, @NonNull NotificationActionButtonInfo notificationActionButtonInfo) {
                UnityPlugin.shared().onPushOpened(notificationInfo.getMessage());
                return false;
            }

            @Override
            public void onNotificationBackgroundAction(@NonNull NotificationInfo notificationInfo, @NonNull NotificationActionButtonInfo notificationActionButtonInfo) {
                UnityPlugin.shared().onPushOpened(notificationInfo.getMessage());
            }

            @Override
            public void onNotificationDismissed(@NonNull NotificationInfo notificationInfo) {

            }
        });

        airship.getMessageCenter().setOnShowMessageCenterListener(new MessageCenter.OnShowMessageCenterListener() {
            @Override
            public boolean onShowMessageCenter(@Nullable String messageId) {
                if (PreferenceManager.getDefaultSharedPreferences(UAirship.getApplicationContext()).getBoolean(AUTO_LAUNCH_MESSAGE_CENTER, true)) {
                    return false;
                } else {
                    UnityPlugin.shared().onShowInbox(messageId);
                    return true;
                }
            }
        });

        airship.getInbox().addListener(new RichPushInbox.Listener() {
            @Override
            public void onInboxUpdated() {
                UnityPlugin.shared().onInboxUpdated();
            }
        });

        airship.setDeepLinkListener(new DeepLinkListener() {
            @Override
            public boolean onDeepLink(@NonNull String deepLink) {
                 if (deepLink == null) {
                     return false;
                }

                UnityPlugin.shared().setDeepLink(deepLink);
                return UnityPlugin.shared().onDeepLinkReceived(deepLink);
            }
        });
    }

    public AirshipConfigOptions createAirshipConfigOptions(Context context) {
        int resourceId = context.getResources().getIdentifier("airship_config", "xml", context.getPackageName());
        if (resourceId <= 0) {
            PluginLogger.error("airship_config.xml not found. Make sure Urban Airship is configured Window => Urban Airship => Settings.");
            return null;
        }

        AirshipConfigOptions options = new AirshipConfigOptions.Builder()
                .applyConfig(context, resourceId)
                .build();

        PluginLogger.setLogLevel(options.logLevel);

        return options;
    }
}
