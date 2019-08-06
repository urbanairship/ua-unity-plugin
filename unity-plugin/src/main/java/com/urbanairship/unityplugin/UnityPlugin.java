/* Copyright Airship and Contributors */

package com.urbanairship.unityplugin;

import com.unity3d.player.UnityPlayer;
import com.urbanairship.Logger;
import com.urbanairship.UAirship;
import com.urbanairship.analytics.CustomEvent;
import com.urbanairship.json.JsonException;
import com.urbanairship.json.JsonList;
import com.urbanairship.json.JsonMap;
import com.urbanairship.json.JsonValue;
import com.urbanairship.push.PushManager;
import com.urbanairship.push.PushMessage;
import com.urbanairship.push.TagGroupsEditor;
import com.urbanairship.util.UAStringUtil;

import org.json.JSONArray;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

public class UnityPlugin {

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
        return UAirship.shared().getPushManager().getChannelId();
    }

    public void addTag(String tag) {
        PluginLogger.debug("UnityPlugin addTag: " + tag);

        PushManager push = UAirship.shared().getPushManager();
        Set<String> tags = push.getTags();
        tags.add(tag);
        push.setTags(tags);
    }

    public void removeTag(String tag) {
        PluginLogger.debug("UnityPlugin removeTag: " + tag);

        PushManager push = UAirship.shared().getPushManager();

        Set<String> tags = push.getTags();
        tags.remove(tag);
        push.setTags(tags);
    }

    public String getTags() {
        PluginLogger.debug("UnityPlugin getTags");
        JSONArray jsonArray = new JSONArray();
        for (String tag : UAirship.shared().getPushManager().getTags()) {
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
        Logger.debug("UnityPlugin setBackgroundLocationAllowed: " + allowed);
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
                        eventBuilder.addProperty(name, jsonMap.opt("stringValue").getString());
                        break;
                    case "d":
                        eventBuilder.addProperty(name, jsonMap.opt("doubleValue").getDouble(0));
                        break;
                    case "b":
                        eventBuilder.addProperty(name, jsonMap.opt("boolValue").getBoolean(false));
                        break;
                    case "sa":
                        List<String> strings = new ArrayList<>();
                        for (JsonValue jsonValue : jsonMap.opt("stringArrayValue").getList()) {
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
        UAirship.shared().getInbox().startInboxActivity();
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

        TagGroupsEditor editor = UAirship.shared().getPushManager().editTagGroups();
        applyTagGroupOperations(editor, payload);
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

    void onDeepLinkReceived(String deepLink) {
        PluginLogger.debug("UnityPlugin deepLink received: " + deepLink);

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnDeepLinkReceived", deepLink);
        }
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
}
