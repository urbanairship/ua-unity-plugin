/*
 Copyright 2015 Urban Airship and Contributors
*/

package com.urbanairship.unityplugin;

import com.unity3d.player.UnityPlayer;
import com.urbanairship.Logger;
import com.urbanairship.UAirship;
import com.urbanairship.push.PushManager;
import com.urbanairship.push.PushMessage;

import org.json.JSONArray;

import java.util.ArrayList;
import java.util.List;
import java.util.Set;

public class UnityPlugin {

    private static UnityPlugin instance = new UnityPlugin();
    private List<String> listeners = new ArrayList<>();
    private PushMessage incomingPush;
    private List<PushMessage> receivePushes = new ArrayList<>();
    private String deepLink;

    UnityPlugin() {
    }

    public static UnityPlugin shared() {
        return instance;
    }

    public String getDeepLink(boolean clear) {
        Logger.debug("UnityPlugin getDeepLink clear " + clear);

        String link = deepLink;
        if (clear) {
            deepLink = null;
        }
        return link;
    }

    // Listener

    public void addListener(String listener) {
        Logger.debug("UnityPlugin adding listener: " + listener);

        listeners.add(listener);

        if (!receivePushes.isEmpty()) {
            for (PushMessage pushMessage : receivePushes) {
                notifyReceivedPush(pushMessage);
            }

            receivePushes.clear();
        }
    }

    public void removeListener(String listener) {
        Logger.debug("UnityPlugin removing listener: " + listener);

        listeners.remove(listener);
    }

    // push

    public String getIncomingPush(boolean clear) {
        Logger.debug("UnityPlugin getIncomingPush clear " + clear);

        String push = getPushPayload(incomingPush);
        if (clear) {
            incomingPush = null;
        }
        return push;
    }

    public boolean isPushEnabled() {
        Logger.debug("UnityPlugin isPushEnabled");
        return UAirship.shared().getPushManager().getUserNotificationsEnabled();
    }

    public void enablePush() {
        Logger.debug("UnityPlugin enablePush");
        UAirship.shared().getPushManager().setUserNotificationsEnabled(true);
    }

    public void disablePush() {
        Logger.debug("UnityPlugin disablePush");
        UAirship.shared().getPushManager().setUserNotificationsEnabled(false);
    }

    public String getChannelId() {
        Logger.debug("UnityPlugin getChannelId");
        return UAirship.shared().getPushManager().getChannelId();
    }

    public void addTag(String tag) {
        Logger.debug("UnityPlugin addTag: " + tag);

        PushManager push = UAirship.shared().getPushManager();
        Set<String> tags = push.getTags();
        tags.add(tag);
        push.setTags(tags);
    }

    public void removeTag(String tag) {
        Logger.debug("UnityPlugin removeTag: " + tag);

        PushManager push = UAirship.shared().getPushManager();

        Set<String> tags = push.getTags();
        tags.remove(tag);
        push.setTags(tags);
    }

    public String getTags() {
        Logger.debug("UnityPlugin getTags");
        JSONArray jsonArray = new JSONArray();
        for (String tag : UAirship.shared().getPushManager().getTags()) {
            jsonArray.put(tag);
        }

        return jsonArray.toString();
    }

    public void setAlias(String alias) {
        Logger.debug("UnityPlugin setAlias: " + alias);
        UAirship.shared().getPushManager().setAlias(alias);
    }

    // location

    public void enableLocation() {
        Logger.debug("UnityPlugin enableLocation");
        UAirship.shared().getLocationManager().setLocationUpdatesEnabled(true);
    }

    public boolean isLocationEnabled() {
        Logger.debug("UnityPlugin isLocationEnabled");
        return UAirship.shared().getLocationManager().isLocationUpdatesEnabled();
    }

    public void disableLocation() {
        Logger.debug("UnityPlugin disableLocation");
        UAirship.shared().getLocationManager().setLocationUpdatesEnabled(false);
    }

    public void enableBackgroundLocation() {
        Logger.debug("UnityPlugin enableBackgroundLocation");
        UAirship.shared().getLocationManager().setBackgroundLocationAllowed(true);
    }

    public void disableBackgroundLocation() {
        Logger.debug("UnityPlugin disableBackgroundLocation");
        UAirship.shared().getLocationManager().setBackgroundLocationAllowed(false);
    }

    public boolean isBackgroundLocationEnabled() {
        Logger.debug("UnityPlugin isBackgroundLocationEnabled");
        return UAirship.shared().getLocationManager().isBackgroundLocationAllowed();
    }

    void onPushOpened(PushMessage message) {
        Logger.debug("UnityPlugin push opened.");
        this.incomingPush = message;
    }

    void onPushReceived(PushMessage message) {
        Logger.debug("UnityPlugin push received.");
        if (listeners.isEmpty()) {
            receivePushes.add(message);
        } else {
            notifyReceivedPush(message);
        }
    }

    private void notifyReceivedPush(PushMessage pushMessage) {
        for (String listener : listeners) {
            try {
                Logger.debug("UnityPlugin notifying " + listener + " push received");
                UnityPlayer.UnitySendMessage(listener, "OnPushReceived", getPushPayload(pushMessage));
            } catch (Exception e) {
                Logger.error("Failed to send message to " + listener, e);
            }
        }
    }

    private String getPushPayload(PushMessage pushMessage) {
        if (pushMessage == null) {
            return "";
        }

        return pushMessage.getAlert();
    }

    void setDeepLink(String deepLink) {
        Logger.debug("UnityPlugin setDeepLink: " + deepLink);

        this.deepLink = deepLink;
    }
}
