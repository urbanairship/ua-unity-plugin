/* Copyright Airship and Contributors */

package com.urbanairship.unityplugin;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.preference.PreferenceManager;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.unity3d.player.UnityPlayer;
import com.urbanairship.UAirship;
import com.urbanairship.analytics.CustomEvent;
import com.urbanairship.channel.AirshipChannel;
import com.urbanairship.channel.AttributeEditor;
import com.urbanairship.json.JsonException;
import com.urbanairship.json.JsonList;
import com.urbanairship.json.JsonMap;
import com.urbanairship.json.JsonValue;
import com.urbanairship.messagecenter.MessageCenter;
import com.urbanairship.push.PushMessage;
import com.urbanairship.channel.TagGroupsEditor;
import com.urbanairship.richpush.RichPushMessage;
import com.urbanairship.util.UAStringUtil;

import org.json.JSONArray;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

public class UnityPlugin {

    static final String AUTO_LAUNCH_MESSAGE_CENTER = "com.urbanairship.auto_launch_message_center";

    private static UnityPlugin instance = new UnityPlugin();
    private String listener;
    private PushMessage incomingPush;
    private String deepLink;

    UnityPlugin() {
    }

    public static UnityPlugin shared() {
        return instance;
    }

    public String getDeepLink(boolean clear) {
        PluginLogger.debug("UnityPlugin getDeepLink clear " + clear);

        String link = deepLink;
        if (clear) {
            deepLink = null;
        }
        return link;
    }

    public void setListener(String listener) {
        PluginLogger.debug("UnityPlugin setListener: " + listener);
        this.listener = listener;
    }

    public String getIncomingPush(boolean clear) {
        PluginLogger.debug("UnityPlugin getIncomingPush clear " + clear);

        String push = getPushPayload(incomingPush);
        if (clear) {
            incomingPush = null;
        }
        return push;
    }

    public boolean getUserNotificationsEnabled() {
        PluginLogger.debug("UnityPlugin getUserNotificationsEnabled");
        return UAirship.shared().getPushManager().getUserNotificationsEnabled();
    }

    public void setUserNotificationsEnabled(boolean enabled) {
        PluginLogger.debug("UnityPlugin setUserNotificationsEnabled: " + enabled);
        UAirship.shared().getPushManager().setUserNotificationsEnabled(enabled);
    }

    public String getChannelId() {
        PluginLogger.debug("UnityPlugin getChannelId");
        return UAirship.shared().getChannel().getId();
    }

    public void addTag(String tag) {
        PluginLogger.debug("UnityPlugin addTag: " + tag);

        UAirship.shared().getChannel().editTags().addTag(tag).apply();
    }

    public void removeTag(String tag) {
        PluginLogger.debug("UnityPlugin removeTag: " + tag);

        UAirship.shared().getChannel().editTags().removeTag(tag).apply();
    }

    public String getTags() {
        PluginLogger.debug("UnityPlugin getTags");
        JSONArray jsonArray = new JSONArray();
        for (String tag : UAirship.shared().getChannel().getTags()) {
            jsonArray.put(tag);
        }

        return jsonArray.toString();
    }

    public void setLocationEnabled(boolean enabled) {
        PluginLogger.debug("UnityPlugin setLocationEnabled: " + enabled);
        UAirship.shared().getLocationManager().setLocationUpdatesEnabled(enabled);
    }

    public boolean isLocationEnabled() {
        PluginLogger.debug("UnityPlugin isLocationUpdatesEnabled");
        return UAirship.shared().getLocationManager().isLocationUpdatesEnabled();
    }

    public void setBackgroundLocationAllowed(boolean allowed) {
        PluginLogger.debug("UnityPlugin setBackgroundLocationAllowed: " + allowed);
        UAirship.shared().getLocationManager().setBackgroundLocationAllowed(allowed);
    }

    public boolean isBackgroundLocationAllowed() {
        PluginLogger.debug("UnityPlugin isBackgroundLocationAllowed");
        return UAirship.shared().getLocationManager().isBackgroundLocationAllowed();
    }

    public void addCustomEvent(String eventPayload) {
        PluginLogger.debug("UnityPlugin addCustomEvent: " + eventPayload);

        if (UAStringUtil.isEmpty(eventPayload)) {
            PluginLogger.error("Missing event payload.");
            return;
        }

        JsonMap customEventMap = null;
        try {
            customEventMap = JsonValue.parseString(eventPayload).getMap();
        } catch (JsonException e) {
            PluginLogger.error("Failed to parse event payload", e);
        }
        if (customEventMap == null) {
            PluginLogger.error("Event payload must define a JSON object.");
            return;
        }

        String eventName = customEventMap.opt("eventName").getString();
        if (eventName == null) {
            return;
        }

        String eventValue = customEventMap.opt("eventValue").getString();
        String transactionId = customEventMap.opt("transactionId").getString();
        String interactionType = customEventMap.opt("interactionType").getString();
        String interactionId = customEventMap.opt("interactionId").getString();
        JsonList properties = customEventMap.opt("properties").getList();

        CustomEvent.Builder eventBuilder = new CustomEvent.Builder(eventName)
                .setEventValue(eventValue);

        if (!UAStringUtil.isEmpty(transactionId)) {
            eventBuilder.setTransactionId(transactionId);
        }

        if (!UAStringUtil.isEmpty(interactionType) && !UAStringUtil.isEmpty(interactionId)) {
            eventBuilder.setInteraction(interactionType, interactionId);
        }

        if (properties != null) {
            for (JsonValue property : properties) {
                JsonMap jsonMap = property.getMap();
                if (jsonMap == null) {
                    continue;
                }

                String name = jsonMap.opt("name").getString();
                String type = jsonMap.opt("type").getString();

                if (UAStringUtil.isEmpty(name) || UAStringUtil.isEmpty(type)) {
                    continue;
                }

                switch (type) {
                    case "s":
                        String stringValue = jsonMap.opt("stringValue").getString();
                        if (stringValue != null) {
                            eventBuilder.addProperty(name, stringValue);
                        }
                        break;
                    case "d":
                        eventBuilder.addProperty(name, jsonMap.opt("doubleValue").getDouble(0));
                        break;
                    case "b":
                        eventBuilder.addProperty(name, jsonMap.opt("boolValue").getBoolean(false));
                        break;
                    case "sa":
                        JsonList stringArrayValue = jsonMap.opt("stringArrayValue").getList();
                        if (stringArrayValue == null) {
                            break;
                        }
                        List<String> strings = new ArrayList<>();
                        for (JsonValue jsonValue : stringArrayValue) {
                            if (jsonValue.isString()) {
                                strings.add(jsonValue.getString());
                            } else {
                                strings.add(jsonValue.toString());
                            }
                        }

                        eventBuilder.addProperty(name, strings);
                        break;
                    default:
                        continue;
                }
            }
        }

        UAirship.shared().getAnalytics().addEvent(eventBuilder.build());
    }

    public void trackScreen(String screenName) {
        if (UAStringUtil.isEmpty(screenName)) {
            PluginLogger.error("Missing screen name");
            return;
        }

        PluginLogger.debug("UnityPlugin trackScreen: " + screenName);

        UAirship.shared().getAnalytics().trackScreen(screenName);
    }

    public void associateIdentifier(String key, String identifier) {
        if (key == null) {
            PluginLogger.debug("UnityPlugin associateIdentifier failed, key cannot be null");
            return;
        }

        if (identifier == null) {
            PluginLogger.debug("UnityPlugin associateIdentifier removed identifier for key: " + key);
            UAirship.shared().getAnalytics().editAssociatedIdentifiers().removeIdentifier(key).apply();
        } else {
            PluginLogger.debug("UnityPlugin associateIdentifier with identifier: " + identifier + " for key: " + key);
            UAirship.shared().getAnalytics().editAssociatedIdentifiers().addIdentifier(key, identifier).apply();
        }
    }

    public String getNamedUserId() {
        PluginLogger.debug("UnityPlugin getNamedUserId");
        return UAirship.shared().getNamedUser().getId();
    }

    public void setNamedUserId(String namedUserId) {
        PluginLogger.debug("UnityPlugin setNamedUserId: " + namedUserId);
        UAirship.shared().getNamedUser().setId(namedUserId);
    }

    public void displayMessageCenter() {
        PluginLogger.debug("UnityPlugin displayMessageCenter");
        UAirship.shared().getMessageCenter().showMessageCenter();
    }

    /**
     * Display an inbox message in the default message center.
     *
     * @param messageId The id of the message to be displayed.
     */
    public void displayInboxMessage(String messageId) {
        PluginLogger.debug("UnityPlugin displayInboxMessage %s", messageId);

        Intent intent = new Intent(UnityPlayer.currentActivity, CustomMessageActivity.class)
                .setAction(MessageCenter.VIEW_MESSAGE_INTENT_ACTION)
                .setPackage(UnityPlayer.currentActivity.getPackageName())
                .setData(Uri.fromParts(MessageCenter.MESSAGE_DATA_SCHEME, messageId, null))
                .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_SINGLE_TOP);

        UnityPlayer.currentActivity.getApplicationContext().startActivity(intent);
    }

    /**
     * Refresh the message center inbox.
     *
     * The `OnInboxUpdated` event will fire after the inbox has been successfully refreshed.
     */
    public void refreshInbox() {
        PluginLogger.debug("UnityPlugin refreshInbox");
        UAirship.shared().getInbox().fetchMessages(); // this needs to fire an event
    }

    /**
     * Retrieves the current inbox messages.
     */
    public String getInboxMessages() {
        PluginLogger.debug("UnityPlugin getInboxMessages");

        return getInboxMessagesAsJSON();
    }

    /**
     * Marks an inbox message as read.
     *
     * @param messageId The id of the message to be marked as read.
     */
    public void markInboxMessageRead(@NonNull String messageId) {
        PluginLogger.debug("UnityPlugin markInboxMessageRead %s", messageId);
        RichPushMessage message = UAirship.shared().getInbox().getMessage(messageId);

        if (message == null) {
            PluginLogger.debug("Message (%s) not found.", messageId);
        } else {
            message.markRead();
        }
    }

    /**
     * Deletes an inbox message.
     *
     * @param messageId The id of the message to be deleted.
     */
    public void deleteInboxMessage(@NonNull String messageId) {
        PluginLogger.debug("UnityPlugin deleteInboxMessage %s", messageId);
        RichPushMessage message = UAirship.shared().getInbox().getMessage(messageId);

        if (message == null) {
            PluginLogger.debug("Message (%s) not found.", messageId);
        } else {
            message.delete();
        }
    }

    /**
     * Sets the default behavior when the message center is launched from a push notification. If set to false the message center must be manually launched.
     *
     * @param enabled {@code true} to automatically launch the default message center, {@code false} to disable. Default is {@code true}.
     */
    public void setAutoLaunchDefaultMessageCenter(boolean enabled) {
        PluginLogger.debug("UnityPlugin setAutoLaunchDefaultMessageCenter");
        PreferenceManager.getDefaultSharedPreferences(UAirship.getApplicationContext())
                .edit()
                .putBoolean(AUTO_LAUNCH_MESSAGE_CENTER, enabled)
                .apply();
    }

    public int getMessageCenterUnreadCount() {
        PluginLogger.debug("UnityPlugin getMessageCenterUnreadCount");
        return UAirship.shared().getInbox().getUnreadCount();
    }

    public int getMessageCenterCount() {
        PluginLogger.debug("UnityPlugin getMessageCenterCount");
        return UAirship.shared().getInbox().getCount();
    }

    public void editNamedUserTagGroups(String payload) {
        PluginLogger.debug("UnityPlugin editNamedUserTagGroups");

        TagGroupsEditor editor = UAirship.shared().getNamedUser().editTagGroups();
        applyTagGroupOperations(editor, payload);
        editor.apply();
    }

    public void editChannelTagGroups(String payload) {
        PluginLogger.debug("UnityPlugin editChannelTagGroups");

        TagGroupsEditor editor = UAirship.shared().getChannel().editTagGroups();
        applyTagGroupOperations(editor, payload);
        editor.apply();
    }

    public void editChannelAttributes(String payload) {
        PluginLogger.debug("UnityPlugin editChannelAttributes");

        AttributeEditor editor = UAirship.shared().getChannel().editAttributes();
        applyAttributeOperations(editor, payload);
        editor.apply();
    }

    void onPushReceived(PushMessage message) {
        PluginLogger.debug("UnityPlugin push received. " + message);

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPushReceived", getPushPayload(message));
        }
    }

    void onPushOpened(PushMessage message) {
        PluginLogger.debug("UnityPlugin push opened. " + message);

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPushOpened", getPushPayload(message));
        }
        this.incomingPush = message;
    }

    boolean onDeepLinkReceived(String deepLink) {
        PluginLogger.debug("UnityPlugin deepLink received: " + deepLink);

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnDeepLinkReceived", deepLink);
            return true;
        }
        return false;
    }

    void onChannelCreated(String channelId) {
        PluginLogger.debug("UnityPlugin channel created: " + channelId);

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnChannelCreated", channelId);
        }
    }

    void onChannelUpdated(String channelId) {
        PluginLogger.debug("UnityPlugin channel updated: " + channelId);

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnChannelUpdated", channelId);
        }
    }

    void onShowInbox(@Nullable String messageId) {
        if (messageId == null) {
            PluginLogger.debug("UnityPlugin show inbox");

            if (listener != null) {
                UnityPlayer.UnitySendMessage(listener, "OnShowInbox", "");
            }
        } else {
            PluginLogger.debug("UnityPlugin show inbox message: ", messageId);

            if (listener != null) {
                UnityPlayer.UnitySendMessage(listener, "OnShowInbox", messageId);
            }
        }
    }

    void onInboxUpdated() {
        JsonMap counts = JsonMap.newBuilder()
                .put("unread", UAirship.shared().getInbox().getUnreadCount())
                .put("total", UAirship.shared().getInbox().getCount())
                .build();
        PluginLogger.debug("UnityPlugin inboxUpdated (unread = %s, total = %s)", UAirship.shared().getInbox().getUnreadCount(), UAirship.shared().getInbox().getCount());

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnInboxUpdated", counts.toString());
        }
    }

    private String getPushPayload(PushMessage message) {
        if (message == null) {
            return null;
        }

        Map<String, Object> payloadMap = new HashMap<>();

        List<Map<String, String>> extras = new ArrayList<>();

        for (String key : message.getPushBundle().keySet()) {
            String value;
            if (!UAStringUtil.equals(key, "google.sent_time")) {
                value = message.getPushBundle().getString(key);
            } else {
                continue;
            }

            if (value == null) {
                continue;
            }

            Map<String, String> extra = new HashMap<>();
            extra.put("key", key);
            extra.put("value", value);
            extras.add(extra);
        }

        if (message.getAlert() != null) {
            payloadMap.put("alert", message.getAlert());
        }

        if (message.getSendId() != null) {
            payloadMap.put("identifier", message.getSendId());
        }

        payloadMap.put("extras", extras);

        return JsonValue.wrapOpt(payloadMap).toString();
    }

    public String getInboxMessagesAsJSON() {
        List<Map<String, Object>> messages = new ArrayList<>();
        for (RichPushMessage message : UAirship.shared().getInbox().getMessages()) {
            Map<String, Object> messageMap = new HashMap<>();
            messageMap.put("id", message.getMessageId());
            messageMap.put("title", message.getTitle());
            messageMap.put("sentDate", message.getSentDate().getTime());
            String listIconUrl = message.getListIconUrl();
            if (listIconUrl != null) {
                messageMap.put("listIconUrl", listIconUrl);
            }
            messageMap.put("isRead", message.isRead());
            messageMap.put("isDeleted", message.isDeleted());

            if (message.getExtras().keySet().size() > 0) {
                List<String> extrasKeys = new ArrayList<>();
                List<Object> extrasValues = new ArrayList<>();
                Bundle extras = message.getExtras();

                for (String key : extras.keySet()) {
                    extrasKeys.add(key);
                    extrasValues.add(extras.get(key));
                }

                messageMap.put("extrasKeys", extrasKeys);
                messageMap.put("extrasValues", extrasValues);
            }
            messages.add(messageMap);
        }
        return JsonValue.wrapOpt(messages).toString();
    }

    void setDeepLink(String deepLink) {
        PluginLogger.debug("UnityPlugin setDeepLink: " + deepLink);

        this.deepLink = deepLink;
    }

    private static void applyTagGroupOperations(TagGroupsEditor editor, String payload) {
        JsonMap payloadMap;
        try {
            payloadMap = JsonValue.parseString(payload).getMap();
        } catch (JsonException e) {
            PluginLogger.error("Unable to apply tag group operations: ", e);
            return;
        }

        if (payloadMap == null || !payloadMap.opt("values").isJsonList()) {
            return;
        }

        for (JsonValue operation : payloadMap.opt("values").optList()) {
            if (!operation.isJsonMap()) {
                continue;
            }

            JsonList tags = operation.optMap().opt("tags").getList();
            String group = operation.optMap().opt("tagGroup").getString();
            String operationType = operation.optMap().opt("operation").getString();

            if (tags == null || tags.isEmpty() || UAStringUtil.isEmpty(group) || UAStringUtil.isEmpty(operationType)) {
                continue;
            }

            HashSet<String> tagSet = new HashSet<>();
            for (JsonValue tag : tags) {
                if (tag.isString()) {
                    tagSet.add(tag.getString());
                }
            }

            switch (operationType) {
                case "add":
                    editor.addTags(group, tagSet);
                    break;
                case "remove":
                    editor.removeTags(group, tagSet);
                    break;
            }
        }
    }

    private static void applyAttributeOperations(AttributeEditor editor, String payload) {
        JsonMap payloadMap;
        try {
            payloadMap = JsonValue.parseString(payload).getMap();
        } catch (JsonException e) {
            PluginLogger.error("Unable to apply attribute operations: ", e);
            return;
        }

        if (payloadMap == null || !payloadMap.opt("values").isJsonList()) {
            return;
        }

        for (JsonValue operation : payloadMap.opt("values").optList()) {
            if (!operation.isJsonMap()) {
                continue;
            }

            String action = operation.optMap().opt("action").getString();
            String key = operation.optMap().opt("key").getString();
            String value = operation.optMap().opt("value").getString();
            String type = operation.optMap().opt("type").getString();

            if (value == null || UAStringUtil.isEmpty(key) || UAStringUtil.isEmpty(action)) {
                continue;
            }

            switch (action) {
                case "Set":
                    if (type.equals("String")) {
                        editor.setAttribute(key, value);
                    } else if (type.equals("Integer")) {
                        editor.setAttribute(key, Integer.valueOf(value));
                    } else if (type.equals("Long")) {
                        editor.setAttribute(key, Long.valueOf(value));
                    } else if (type.equals("Float")) {
                        editor.setAttribute(key, Float.valueOf(value));
                    } else if (type.equals("Double")) {
                        editor.setAttribute(key, Double.valueOf(value));
                    }
                    break;
                case "Remove":
                    editor.removeAttribute(key);
                    break;
            }
        }
    }
}
