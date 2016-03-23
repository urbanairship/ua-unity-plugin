/*
 Copyright 2016 Urban Airship and Contributors
*/

package com.urbanairship.unityplugin;

import android.content.Context;

import com.urbanairship.push.BaseIntentReceiver;
import com.urbanairship.push.PushMessage;

public class IntentReceiver extends BaseIntentReceiver {
    @Override
    protected void onChannelRegistrationSucceeded(Context context, String channelId) {
        UnityPlugin.shared().onChannelRegistrationSucceeded(channelId);
    }

    @Override
    protected void onChannelRegistrationFailed(Context context) {

    }

    @Override
    protected void onPushReceived(Context context, PushMessage pushMessage, int notificationId) {
        UnityPlugin.shared().onPushReceived(pushMessage);
    }

    @Override
    protected void onBackgroundPushReceived(Context context, PushMessage pushMessage) {

    }

    @Override
    protected boolean onNotificationOpened(Context context, PushMessage pushMessage, int notificationId) {
        UnityPlugin.shared().onPushOpened(pushMessage);
        return false;
    }

    @Override
    protected boolean onNotificationActionOpened(Context context, PushMessage pushMessage, int notificationId, String buttonId, boolean isForeground) {
        return false;
    }
}
