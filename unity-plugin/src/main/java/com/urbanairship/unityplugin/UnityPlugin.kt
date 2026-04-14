/* Copyright Airship and Contributors */
package com.urbanairship.unityplugin

import android.R.id.message
import android.os.Build
import android.os.Bundle
import androidx.annotation.RequiresApi
import com.unity3d.player.UnityPlayer
import com.urbanairship.Autopilot
import com.urbanairship.PrivacyManager
import com.urbanairship.android.framework.proxy.EventType
import com.urbanairship.android.framework.proxy.MessageCenterMessage
import com.urbanairship.android.framework.proxy.ProxyLogger
import com.urbanairship.android.framework.proxy.events.EventEmitter
import com.urbanairship.android.framework.proxy.proxies.AirshipProxy
import com.urbanairship.android.framework.proxy.proxies.EnableUserNotificationsArgs
import com.urbanairship.android.framework.proxy.proxies.FeatureFlagProxy
import com.urbanairship.android.framework.proxy.NotificationStatus
import com.urbanairship.json.JsonException
import com.urbanairship.json.JsonMap
import com.urbanairship.json.JsonValue
import com.urbanairship.json.optionalField
import com.urbanairship.json.requireMap
import com.urbanairship.messagecenter.MessageCenter
import com.urbanairship.push.PushMessage
import com.urbanairship.util.UAStringUtil
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.launch
import kotlinx.coroutines.plus
import kotlinx.coroutines.runBlocking
import org.json.JSONArray
import org.json.JSONObject


class UnityPlugin {
    private val scope: CoroutineScope = CoroutineScope(Dispatchers.Main) + SupervisorJob()

    private val airshipProxyInstance = AirshipProxy.shared(UnityPlayer.currentActivity.applicationContext)

    private var listener: String? = null

    init {
        Autopilot.automaticTakeOff(UnityPlayer.currentActivity.applicationContext)

        scope.launch {
            EventEmitter.shared().pendingEventListener.collect {
                notifyPendingEvents()
            }
        }
    }

    private fun notifyPendingEvents() {
        EventType.entries.forEach { eventType ->
            EventEmitter.shared().processPending(listOf(eventType)) { event ->
                when (event.type) {
                    EventType.CHANNEL_CREATED -> onChannelCreated(event.body.optionalField<String>("channelId"))
                    EventType.DEEP_LINK_RECEIVED -> onDeepLinkReceived(event.body.optionalField<String>("deepLink"))
                    EventType.DISPLAY_MESSAGE_CENTER -> onShowInbox(event.body.optionalField<String>("messageId"))
                    EventType.DISPLAY_PREFERENCE_CENTER -> onPreferenceCenterDisplay(event.body.optionalField<String>("preferenceCenterId"))
                    EventType.MESSAGE_CENTER_UPDATED -> onInboxUpdated(event.body.optionalField<Int>("messageUnreadCount"), event.body.optionalField<Int>("messageCount"))
                    EventType.PUSH_TOKEN_RECEIVED -> onPushTokenReceived(event.body.optionalField<String>("pushToken"))
                    EventType.FOREGROUND_NOTIFICATION_RESPONSE_RECEIVED -> onPushOpened(event.body.optionalField<JsonValue>("pushPayload"))
                    EventType.BACKGROUND_NOTIFICATION_RESPONSE_RECEIVED -> onPushOpened(event.body.optionalField<JsonValue>("pushPayload"))
                    EventType.FOREGROUND_PUSH_RECEIVED -> onPushReceived(event.body.optionalField<JsonValue>("pushPayload"))
                    EventType.BACKGROUND_PUSH_RECEIVED -> onPushReceived(event.body.optionalField<JsonValue>("pushPayload"))
                    EventType.NOTIFICATION_STATUS_CHANGED -> onNotificationStatusChanged(event.body.optionalField<JsonValue>("status"))
                    EventType.PENDING_EMBEDDED_UPDATED -> {}
                }
                true
            }
        }
    }

    fun setListener(listener: String) {
        ProxyLogger.debug("UnityPlugin setListener method call with: $listener")
        this.listener = listener
    }

    // Airship

    fun takeOff(config: String): Boolean {
        ProxyLogger.debug("UnityPlugin takeOff method call with: $config")
        return airshipProxyInstance.takeOff(JsonValue.parseString(config))
    }

    fun isFlying(): Boolean {
        ProxyLogger.debug("UnityPlugin isFlying method call")
        return airshipProxyInstance.isFlying()
    }

    // Channel
    
    fun getChannelId(): String? {
        ProxyLogger.debug("UnityPlugin getChannelId method call")
        return airshipProxyInstance.channel.getChannelId()
    }

    fun waitForChannelId(): String {
        ProxyLogger.debug("UnityPlugin waitForChannelId method call")
        return runBlocking(Dispatchers.IO) {
            airshipProxyInstance.channel.waitForChannelId()
        }
    }

    fun getTags(): String {
        ProxyLogger.debug("UnityPlugin getTags method call")
        val jsonArray = JSONArray()
        for (tag in airshipProxyInstance.channel.getTags()) {
            jsonArray.put(tag)
        }
        return jsonArray.toString()
    }

    fun editTags(payload: String) {
        ProxyLogger.debug("UnityPlugin editTags method call with: $payload")
        try {
            airshipProxyInstance.channel.editTags(JsonValue.parseString(payload).optMap().opt("values"))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    fun editChannelTagGroups(payload: String) {
        ProxyLogger.debug("UnityPlugin editChannelTagGroups method call with: $payload")
        try {
            airshipProxyInstance.channel.editTagGroups(JsonValue.parseString(payload).optMap().opt("values"))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    fun editChannelAttributes(payload: String) {
        ProxyLogger.debug("UnityPlugin editChannelAttributes method call with: $payload")
        try {
            airshipProxyInstance.channel.editAttributes(JsonValue.parseString(payload).optMap().opt("values"))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    fun getChannelSubscriptionLists(): String {
        ProxyLogger.debug("UnityPlugin getChannelSubscriptionLists method call")
        return runBlocking(Dispatchers.IO) {
            val jsonArray = JSONArray()
            for (tag in airshipProxyInstance.channel.getSubscriptionLists()) {
                jsonArray.put(tag)
            }
            jsonArray.toString()
        }
    }

    fun editChannelSubscriptionLists(payload: String) {
        ProxyLogger.debug("UnityPlugin editChannelSubscriptionLists method call with: $payload")
        try {
            airshipProxyInstance.channel.editSubscriptionLists(JsonValue.parseString(payload).optMap().opt("values"))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    // Contact

    fun identify(namedUserId: String?) {
        ProxyLogger.debug("UnityPlugin identify method call with: $namedUserId")
        airshipProxyInstance.contact.identify(namedUserId)
    }

    fun reset() {
        ProxyLogger.debug("UnityPlugin reset method call")
        airshipProxyInstance.contact.reset()
    }

    fun getNamedUserId(): String? {
        ProxyLogger.debug("UnityPlugin getNamedUserId method call")
        return airshipProxyInstance.contact.getNamedUserId()
    }

    fun notifyRemoteLogin() {
        ProxyLogger.debug("UnityPlugin notifyRemoteLogin method call")
        airshipProxyInstance.contact.notifyRemoteLogin()
    }

    fun editContactTagGroups(payload: String) {
        ProxyLogger.debug("UnityPlugin editContactTagGroups method call with: $payload")
        try {
            airshipProxyInstance.contact.editTagGroups(JsonValue.parseString(payload).optMap().opt("values"))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    fun editContactAttributes(payload: String) {
        ProxyLogger.debug("UnityPlugin editContactAttributes method call with: $payload")
        try {
            airshipProxyInstance.contact.editAttributes(JsonValue.parseString(payload).optMap().opt("values"))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    fun getContactSubscriptionLists(): String {
        ProxyLogger.debug("UnityPlugin getContactSubscriptionLists method call")
        return runBlocking(Dispatchers.IO) {
            val resultArray = JSONArray()
            airshipProxyInstance.contact.getSubscriptionLists().forEach { subscription ->
                val scopesArray = JSONArray()
                for (scope in subscription.value) {
                    scopesArray.put(scope)
                }
                val item = JSONObject()
                item.put("listId", subscription.key)
                item.put("scopes", scopesArray)
                resultArray.put(item)
            }
            resultArray.toString()
        }
    }

    fun editContactSubscriptionLists(payload: String) {
        ProxyLogger.debug("UnityPlugin editContactSubscriptionLists method call with: $payload")
        try {
            airshipProxyInstance.contact.editSubscriptionLists(JsonValue.parseString(payload).optMap().opt("values"))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    // Analytics

    fun associateIdentifier(key: String, identifier: String?) {
        if (identifier == null) {
            ProxyLogger.debug("UnityPlugin associateIdentifier method call removed identifier for key: $key")
        } else {
            ProxyLogger.debug("UnityPlugin associateIdentifier method call with identifier: $identifier for key: $key")
        }
        airshipProxyInstance.analytics.associateIdentifier(key, identifier)
    }

    fun trackScreen(screenName: String) {
        ProxyLogger.debug("UnityPlugin trackScreen method call with: $screenName")
        airshipProxyInstance.analytics.trackScreen(screenName)
    }

    fun addCustomEvent(eventPayload: String) {
        ProxyLogger.debug("UnityPlugin addCustomEvent method call with: $eventPayload")
        try {
            airshipProxyInstance.analytics.addEvent(JsonValue.parseString(eventPayload))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse event payload", e)
        }
    }

    fun getSessionId(): String {
        ProxyLogger.debug("UnityPlugin getSessionId method call")
        return airshipProxyInstance.analytics.getSessionId()
    }

    // InApp

    fun setPaused(paused: Boolean) {
        ProxyLogger.debug("UnityPlugin setPaused method call with: $paused")
        airshipProxyInstance.inApp.setPaused(paused)
    }

    fun isPaused(): Boolean {
        ProxyLogger.debug("UnityPlugin isPaused method call")
        return airshipProxyInstance.inApp.isPaused()
    }

    fun setDisplayInterval(displayInterval: Long) {
        ProxyLogger.debug("UnityPlugin setDisplayInterval method call with: $displayInterval")
        airshipProxyInstance.inApp.setDisplayInterval(displayInterval)
    }

    fun getDisplayInterval(): Long {
        ProxyLogger.debug("UnityPlugin getDisplayInterval method call")
        return airshipProxyInstance.inApp.getDisplayInterval()
    }

    // Locale

    fun setLocaleOverride(localeIdentifier: String) {
        ProxyLogger.debug("UnityPlugin setLocaleOverride method call with: $localeIdentifier")
        airshipProxyInstance.locale.setCurrentLocale(localeIdentifier)
    }

    fun clearLocaleOverride() {
        ProxyLogger.debug("UnityPlugin clearLocaleOverride method call")
        airshipProxyInstance.locale.clearLocale()
    }

    fun getLocale(): String {
        ProxyLogger.debug("UnityPlugin getLocale method call")
        return airshipProxyInstance.locale.getCurrentLocale()
    }

    // Message Center

    fun getUnreadCount(): Int {
        ProxyLogger.debug("UnityPlugin getUnreadCount method call")
        return runBlocking(Dispatchers.IO) {
            airshipProxyInstance.messageCenter.getUnreadMessagesCount()
        }
    }

    fun getMessages(): String {
        ProxyLogger.debug("UnityPlugin getMessages method call")
        return runBlocking(Dispatchers.IO) {
            getInboxMessagesAsJSON(airshipProxyInstance.messageCenter.getMessages())
        }
    }

    fun markMessageRead(messageId: String) {
        ProxyLogger.debug("UnityPlugin markMessageRead method call with: $messageId")
        airshipProxyInstance.messageCenter.markMessageRead(messageId)
    }

    fun deleteMessage(messageId: String) {
        ProxyLogger.debug("UnityPlugin deleteMessage method call with: $messageId")
        airshipProxyInstance.messageCenter.deleteMessage(messageId)
    }

    fun refreshMessages() {
        ProxyLogger.debug("UnityPlugin refreshMessages method call")
        runBlocking(Dispatchers.IO) {
            airshipProxyInstance.messageCenter.refreshInbox()
        }
    }

    fun setAutoLaunchDefaultMessageCenter(enabled: Boolean) {
        ProxyLogger.debug("UnityPlugin setAutoLaunchDefaultMessageCenter method call with: $enabled")
        airshipProxyInstance.messageCenter.setAutoLaunchDefaultMessageCenter(enabled)
    }

    fun displayMessageCenter(messageId: String?) {
        ProxyLogger.debug("UnityPlugin displayMessageCenter method call with: $messageId")
        airshipProxyInstance.messageCenter.display(messageId)
    }

    fun dismissMessageCenter() {
        ProxyLogger.debug("UnityPlugin dismissMessageCenter method call")
        airshipProxyInstance.messageCenter.dismiss()
    }

    fun showMessageView(messageId: String) {
        ProxyLogger.debug("UnityPlugin showMessageView method call with: $messageId")
        airshipProxyInstance.messageCenter.showMessageView(messageId)
    }

    fun showMessageCenter(messageId: String?) {
        ProxyLogger.debug("UnityPlugin showMessageCenter method call with: $messageId")
        airshipProxyInstance.messageCenter.showMessageCenter(messageId)
    }

    // Preference Center

    fun displayPreferenceCenter(preferenceCenterId: String) {
        ProxyLogger.debug("UnityPlugin displayPreferenceCenter method call with: $preferenceCenterId")
        airshipProxyInstance.preferenceCenter.displayPreferenceCenter(preferenceCenterId)
    }

    fun getPreferenceCenterConfig(preferenceCenterId: String): String {
        ProxyLogger.debug("UnityPlugin getPreferenceCenterConfig method call with: $preferenceCenterId")
        return runBlocking(Dispatchers.IO) {
            JsonValue.wrapOpt(airshipProxyInstance.preferenceCenter.getPreferenceCenterConfig(preferenceCenterId)).toString()
        }
    }

    fun setAutoLaunchDefaultPreferenceCenter(preferenceCenterId: String, autoLaunch: Boolean) {
        ProxyLogger.debug("UnityPlugin setAutoLaunchDefaultPreferenceCenter method call with: $preferenceCenterId, $autoLaunch")
        airshipProxyInstance.preferenceCenter.setAutoLaunchPreferenceCenter(preferenceCenterId, autoLaunch)
    }

    // Privacy Manager

    fun setEnabledFeatures(features: Array<String>) {
        ProxyLogger.debug("UnityPlugin setEnabledFeatures method call with: ${features.joinToString()}")
        airshipProxyInstance.privacyManager.setEnabledFeatures(features.asList())
    }

    fun getEnabledFeatures(): Array<String> {
        ProxyLogger.debug("UnityPlugin getEnabledFeatures method call")
        return airshipProxyInstance.privacyManager.getFeatureNames().toTypedArray()
    }

    fun enableFeatures(features: Array<String>) {
        ProxyLogger.debug("UnityPlugin enableFeatures method call with: ${features.joinToString()}")
        airshipProxyInstance.privacyManager.enableFeatures(features.asList())
    }

    fun disableFeatures(features: Array<String>) {
        ProxyLogger.debug("UnityPlugin disableFeatures method call with: ${features.joinToString()}")
        airshipProxyInstance.privacyManager.disableFeatures(features.asList())
    }

    fun isFeaturesEnabled(features: Array<String>): Boolean {
        ProxyLogger.debug("UnityPlugin isFeaturesEnabled method call with: ${features.joinToString()}")
        return airshipProxyInstance.privacyManager.isFeatureEnabled(features.asList())
    }

    // Push

    fun isUserNotificationsEnabled(): Boolean {
        ProxyLogger.debug("UnityPlugin isUserNotificationsEnabled method call")
        return airshipProxyInstance.push.isUserNotificationsEnabled()
    }

    fun setUserNotificationsEnabled(enabled: Boolean) {
        ProxyLogger.debug("UnityPlugin setUserNotificationsEnabled method call with: $enabled")
        airshipProxyInstance.push.setUserNotificationsEnabled(enabled)
    }

    fun enableUserNotifications(fallback: String?): Boolean {
        ProxyLogger.debug("UnityPlugin enableUserNotifications method call with: $fallback")
        return runBlocking(Dispatchers.IO) {
            airshipProxyInstance.push.enableUserPushNotifications(
                EnableUserNotificationsArgs.fromJson(JsonValue.parseString(fallback))
            )
        }
    }

    fun getNotificationStatus(): String {
        ProxyLogger.debug("UnityPlugin getNotificationStatus method call")
        return runBlocking(Dispatchers.IO) {
            airshipProxyInstance.push.getNotificationStatus().toJsonValue().toString()
        }
    }

    fun getPushToken(): String? {
        ProxyLogger.debug("UnityPlugin getPushToken method call")
        return airshipProxyInstance.push.getRegistrationToken()
    }

    fun getActiveNotifications(): String {
        ProxyLogger.debug("UnityPlugin getActiveNotifications method call")
        return JsonValue.wrapOpt(airshipProxyInstance.push.getActiveNotifications()).toString()
    }

    fun clearNotifications() {
        ProxyLogger.debug("UnityPlugin clearNotifications method call")
        airshipProxyInstance.push.clearNotifications()
    }

    fun clearNotification(identifier: String) {
        ProxyLogger.debug("UnityPlugin clearNotification method call with: $identifier")
        airshipProxyInstance.push.clearNotification(identifier)
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    fun isNotificationChannelEnabled(channelId: String): Boolean {
        ProxyLogger.debug("UnityPlugin isNotificationChannelEnabled method call with: $channelId")
        return airshipProxyInstance.push.isNotificationChannelEnabled(channelId)
    }

    fun setNotificationConfig(config: String) {
        ProxyLogger.debug("UnityPlugin setNotificationConfig method call with: $config")
        airshipProxyInstance.push.setNotificationConfig(JsonValue.parseString(config))
    }

    fun setForegroundNotificationsEnabled(enabled: Boolean) {
        ProxyLogger.debug("UnityPlugin setForegroundNotificationsEnabled method call with: $enabled")
        airshipProxyInstance.push.isForegroundNotificationsEnabled = enabled
    }

    fun isForegroundNotificationsEnabled(): Boolean {
        ProxyLogger.debug("UnityPlugin isForegroundNotificationsEnabled method call")
        return airshipProxyInstance.push.isForegroundNotificationsEnabled
    }

    fun runAction(name: String, value: String?): String {
        ProxyLogger.debug("UnityPlugin runAction method call with: $name, $value")
        return runBlocking(Dispatchers.IO) {
            val actionResult = airshipProxyInstance.actions.runAction(name, JsonValue.parseString(value))
            JsonValue.wrapOpt(actionResult).toString()
        }
    }

    fun flag(name: String): String {
        ProxyLogger.debug("UnityPlugin flag method call with: $name")
        return runBlocking(Dispatchers.IO) {
            val flagProxy = airshipProxyInstance.featureFlagManager.flag(name)
            val flagJson = flagProxy.toJsonValue().optMap()

            // Build a new JSON with _internal and variables as strings
            val result = JSONObject()
            result.put("isEligible", flagJson.opt("isEligible").getBoolean(false))
            result.put("exists", flagJson.opt("exists").getBoolean(false))

            // Stringify the nested objects so Unity's JsonUtility can deserialize them
            val internal = flagJson.opt("_internal").toJsonValue()
            if (!internal.isNull) {
                result.put("_internal", internal.toString())
            }

            val variables = flagJson.opt("variables").toJsonValue()
            if (!variables.isNull) {
                result.put("variables", variables.toString())
            }

            result.toString()
        }
    }

    fun trackInteraction(flag: String) {
        ProxyLogger.debug("UnityPlugin trackInteraction method call with: $flag")
        airshipProxyInstance.featureFlagManager.trackInteraction(FeatureFlagProxy(JsonValue.parseString(flag)))
    }

    // TODO finish the implementation (live activity and live update)

    fun onPushReceived(message: JsonValue?) {
        ProxyLogger.debug("UnityPlugin push received: $message")

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPushReceived", message.toString())
        }
    }

    fun onPushOpened(message: JsonValue?) {
        ProxyLogger.debug("UnityPlugin push opened: $message")

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPushOpened", message.toString())
        }
    }

    fun onDeepLinkReceived(deepLink: String?): Boolean {
        ProxyLogger.debug("UnityPlugin deepLink received: $deepLink")

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnDeepLinkReceived", deepLink)
            return true
        }
        return false
    }

    fun onChannelCreated(channelId: String?) {
        ProxyLogger.debug("UnityPlugin channel created: $channelId")

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnChannelCreated", channelId)
        }
    }

    fun onShowInbox(messageId: String?) {
        if (messageId == null) {
            ProxyLogger.debug("UnityPlugin show inbox")

            if (listener != null) {
                UnityPlayer.UnitySendMessage(listener, "OnShowInbox", "")
            }
        } else {
            ProxyLogger.debug("UnityPlugin show inbox message: ", messageId)

            if (listener != null) {
                UnityPlayer.UnitySendMessage(listener, "OnShowInbox", messageId)
            }
        }
    }

    fun onInboxUpdated(messageUnreadCount: Int?, messageCount: Int?) {
        ProxyLogger.debug("UnityPlugin inboxUpdated (unread = $messageUnreadCount, total = $messageCount)")

        if (messageUnreadCount == null) {
            ProxyLogger.error("UnityPlugin failed to retrieve message unread count")
        }
        if (messageCount == null) {
            ProxyLogger.error("UnityPlugin failed to retrieve message count")
        }

        val counts: JsonMap = JsonMap.newBuilder()
            .put("unread", messageUnreadCount?.toInt() ?: 0)
            .put("total", messageCount?.toInt() ?: 0)
            .build()
        
        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnInboxUpdated", counts.toString())
        }
    }

    fun onPreferenceCenterDisplay(preferenceCenterId: String?) {
        ProxyLogger.debug("UnityPlugin preference center display: $preferenceCenterId")

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPreferenceCenterDisplay", preferenceCenterId)
        }
    }

    fun onPushTokenReceived(pushToken: String?) {
        ProxyLogger.debug("UnityPlugin push token received: $pushToken")

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPushTokenReceived", pushToken)
        }
    }

    fun onNotificationStatusChanged(status: JsonValue?) {
        ProxyLogger.debug("UnityPlugin notification status changed: ${status?.toString()}")

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnNotificationStatusChanged", status?.toString())
        }
    }

    // TODO Probably remove that, I don't think we'll need it anymore
    private fun getPushPayload(message: PushMessage?): String? {
        if (message == null) {
            return null
        }

        val payloadMap: MutableMap<String?, Any?> = HashMap()

        val extras: MutableList<MutableMap<String?, String?>?> = ArrayList()

        for (key in message.getPushBundle().keySet()) {
            val value: String?
            if (!UAStringUtil.equals(key, "google.sent_time")) {
                value = message.getPushBundle().getString(key)
            } else {
                continue
            }

            if (value == null) {
                continue
            }

            val extra: MutableMap<String?, String?> = HashMap()
            extra.put("key", key)
            extra.put("value", value)
            extras.add(extra)
        }

        if (message.alert != null) {
            payloadMap.put("alert", message.alert)
        }

        if (message.sendId != null) {
            payloadMap.put("identifier", message.sendId)
        }

        payloadMap.put("extras", extras)

        return JsonValue.wrapOpt(payloadMap).toString()
    }

    fun getInboxMessagesAsJSON(messageList: List<MessageCenterMessage>): String {
        val messages: MutableList<MutableMap<String?, Any?>?> = ArrayList()
        for (message in messageList) {
            val messageMap: MutableMap<String?, Any?> = HashMap<String?, Any?>()
            messageMap["id"] = message.id
            messageMap["title"] = message.title
            messageMap["sentDate"] = message.sentDate
            val listIconUrl: String? = message.listIconUrl
            if (listIconUrl != null) {
                messageMap["listIconUrl"] = listIconUrl
            }
            messageMap["isRead"] = message.isRead

            if (message.extras.entries.isNotEmpty()) {
                val extrasKeys: MutableList<String?> = ArrayList()
                val extrasValues: MutableList<Any?> = ArrayList()

                for (entry in message.extras.entries.iterator()) {
                    extrasKeys.add(entry.key)
                    extrasValues.add(entry.value)
                }

                messageMap["extrasKeys"] = extrasKeys
                messageMap["extrasValues"] = extrasValues
            }
            messages.add(messageMap)
        }
        return JsonValue.wrapOpt(messages).toString()
    }

    companion object {
        private val instance = UnityPlugin()

        private val FEATURE_MAP = mapOf(
            "FEATURE_NONE" to PrivacyManager.Feature.NONE,
            "FEATURE_IN_APP_AUTOMATION" to PrivacyManager.Feature.IN_APP_AUTOMATION,
            "FEATURE_MESSAGE_CENTER" to PrivacyManager.Feature.MESSAGE_CENTER,
            "FEATURE_PUSH" to PrivacyManager.Feature.PUSH,
            "FEATURE_ANALYTICS" to PrivacyManager.Feature.ANALYTICS,
            "FEATURE_TAGS_AND_ATTRIBUTES" to PrivacyManager.Feature.TAGS_AND_ATTRIBUTES,
            "FEATURE_CONTACTS" to PrivacyManager.Feature.CONTACTS,
            "FEATURE_LOCATION" to PrivacyManager.Feature.FEATURE_FLAGS,
            "FEATURE_ALL" to PrivacyManager.Feature.ALL
        )

        @JvmStatic
        fun shared(): UnityPlugin {
            return instance
        }
    }
}