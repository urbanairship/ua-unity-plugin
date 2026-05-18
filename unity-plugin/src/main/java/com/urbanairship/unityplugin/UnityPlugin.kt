/* Copyright Airship and Contributors */
package com.urbanairship.unityplugin

import android.os.Build
import androidx.annotation.RequiresApi
import com.unity3d.player.UnityPlayer
import com.urbanairship.Autopilot
import com.urbanairship.UALog
import com.urbanairship.android.framework.proxy.MessageCenterMessage
import com.urbanairship.android.framework.proxy.events.EventEmitter
import com.urbanairship.android.framework.proxy.events.EventType
import com.urbanairship.android.framework.proxy.proxies.AirshipProxy
import com.urbanairship.android.framework.proxy.proxies.EnableUserNotificationsArgs
import com.urbanairship.android.framework.proxy.proxies.FeatureFlagProxy
import com.urbanairship.json.JsonMap
import com.urbanairship.json.JsonValue
import com.urbanairship.json.optionalField
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
                    else -> {}
                }
                true
            }
        }
    }

    fun setListener(listener: String) {
        UALog.d { "UnityPlugin setListener method call with: $listener" }
        this.listener = listener
    }

    // Airship

    fun takeOff(config: String): Boolean {
        UALog.d { "UnityPlugin takeOff method call with: $config" }
        return airshipProxyInstance.takeOff(JsonValue.parseString(config))
    }

    fun isFlying(): Boolean {
        UALog.d { "UnityPlugin isFlying method call" }
        return airshipProxyInstance.isFlying()
    }

    // Channel
    
    fun getChannelId(): String? {
        UALog.d { "UnityPlugin getChannelId method call" }
        return airshipProxyInstance.channel.getChannelId()
    }

    fun waitForChannelId(): String {
        UALog.d { "UnityPlugin waitForChannelId method call" }
        return runBlocking(Dispatchers.IO) {
            airshipProxyInstance.channel.waitForChannelId()
        }
    }

    fun getTags(): String {
        UALog.d { "UnityPlugin getTags method call" }
        val jsonArray = JSONArray()
        for (tag in airshipProxyInstance.channel.getTags()) {
            jsonArray.put(tag)
        }
        return jsonArray.toString()
    }

    fun editTags(payload: String) {
        UALog.d { "UnityPlugin editTags method call with: $payload" }
        airshipProxyInstance.channel.editTags(JsonValue.parseString(payload).optMap().opt("values"))

    }

    fun editChannelTagGroups(payload: String) {
        UALog.d { "UnityPlugin editChannelTagGroups method call with: $payload" }
        airshipProxyInstance.channel.editTagGroups(JsonValue.parseString(payload).optMap().opt("values"))
    }

    fun editChannelAttributes(payload: String) {
        UALog.d { "UnityPlugin editChannelAttributes method call with: $payload" }
        airshipProxyInstance.channel.editAttributes(JsonValue.parseString(payload))
    }

    fun getChannelSubscriptionLists(): String {
        UALog.d { "UnityPlugin getChannelSubscriptionLists method call" }
        return runBlocking(Dispatchers.IO) {
            val jsonArray = JSONArray()
            for (tag in airshipProxyInstance.channel.getSubscriptionLists()) {
                jsonArray.put(tag)
            }
            jsonArray.toString()
        }
    }

    fun editChannelSubscriptionLists(payload: String) {
        UALog.d { "UnityPlugin editChannelSubscriptionLists method call with: $payload" }
        airshipProxyInstance.channel.editSubscriptionLists(JsonValue.parseString(payload).optMap().opt("values"))
    }

    // Contact

    fun identify(namedUserId: String?) {
        UALog.d { "UnityPlugin identify method call with: $namedUserId" }
        airshipProxyInstance.contact.identify(namedUserId)
    }

    fun reset() {
        UALog.d { "UnityPlugin reset method call" }
        airshipProxyInstance.contact.reset()
    }

    fun getNamedUserId(): String? {
        UALog.d { "UnityPlugin getNamedUserId method call" }
        return airshipProxyInstance.contact.getNamedUserId()
    }

    fun notifyRemoteLogin() {
        UALog.d { "UnityPlugin notifyRemoteLogin method call" }
        airshipProxyInstance.contact.notifyRemoteLogin()
    }

    fun editContactTagGroups(payload: String) {
        UALog.d { "UnityPlugin editContactTagGroups method call with: $payload" }
        airshipProxyInstance.contact.editTagGroups(JsonValue.parseString(payload).optMap().opt("values"))
    }

    fun editContactAttributes(payload: String) {
        UALog.d { "UnityPlugin editContactAttributes method call with: $payload" }
        airshipProxyInstance.contact.editAttributes(JsonValue.parseString(payload))
    }

    fun getContactSubscriptionLists(): String {
        UALog.d { "UnityPlugin getContactSubscriptionLists method call" }
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
        UALog.d { "UnityPlugin editContactSubscriptionLists method call with: $payload" }
        airshipProxyInstance.contact.editSubscriptionLists(JsonValue.parseString(payload).optMap().opt("values"))
    }

    // Analytics

    fun associateIdentifier(key: String, identifier: String?) {
        if (identifier == null) {
            UALog.d { "UnityPlugin associateIdentifier method call removed identifier for key: $key" }
        } else {
            UALog.d { "UnityPlugin associateIdentifier method call with identifier: $identifier for key: $key" }
        }
        airshipProxyInstance.analytics.associateIdentifier(key, identifier)
    }

    fun trackScreen(screenName: String) {
        UALog.d { "UnityPlugin trackScreen method call with: $screenName" }
        airshipProxyInstance.analytics.trackScreen(screenName)
    }

    fun addCustomEvent(eventPayload: String) {
        UALog.d { "UnityPlugin addCustomEvent method call with: $eventPayload" }
        airshipProxyInstance.analytics.addEvent(JsonValue.parseString(eventPayload))
    }

    fun getSessionId(): String {
        UALog.d { "UnityPlugin getSessionId method call" }
        return airshipProxyInstance.analytics.getSessionId()
    }

    // InApp

    fun setPaused(paused: Boolean) {
        UALog.d { "UnityPlugin setPaused method call with: $paused" }
        airshipProxyInstance.inApp.setPaused(paused)
    }

    fun isPaused(): Boolean {
        UALog.d { "UnityPlugin isPaused method call" }
        return airshipProxyInstance.inApp.isPaused()
    }

    fun setDisplayInterval(displayInterval: Long) {
        UALog.d { "UnityPlugin setDisplayInterval method call with: $displayInterval" }
        airshipProxyInstance.inApp.setDisplayInterval(displayInterval)
    }

    fun getDisplayInterval(): Long {
        UALog.d { "UnityPlugin getDisplayInterval method call" }
        return airshipProxyInstance.inApp.getDisplayInterval()
    }

    // Locale

    fun setLocaleOverride(localeIdentifier: String) {
        UALog.d { "UnityPlugin setLocaleOverride method call with: $localeIdentifier" }
        airshipProxyInstance.locale.setCurrentLocale(localeIdentifier)
    }

    fun clearLocaleOverride() {
        UALog.d { "UnityPlugin clearLocaleOverride method call" }
        airshipProxyInstance.locale.clearLocale()
    }

    fun getLocale(): String {
        UALog.d { "UnityPlugin getLocale method call" }
        return airshipProxyInstance.locale.getCurrentLocale()
    }

    // Message Center

    fun getUnreadCount(): Int {
        UALog.d { "UnityPlugin getUnreadCount method call" }
        return runBlocking(Dispatchers.IO) {
            airshipProxyInstance.messageCenter.getUnreadMessagesCount()
        }
    }

    fun getMessages(): String {
        UALog.d { "UnityPlugin getMessages method call" }
        return runBlocking(Dispatchers.IO) {
            getInboxMessagesAsJSON(airshipProxyInstance.messageCenter.getMessages())
        }
    }

    fun markMessageRead(messageId: String) {
        UALog.d { "UnityPlugin markMessageRead method call with: $messageId" }
        airshipProxyInstance.messageCenter.markMessageRead(messageId)
    }

    fun deleteMessage(messageId: String) {
        UALog.d { "UnityPlugin deleteMessage method call with: $messageId" }
        airshipProxyInstance.messageCenter.deleteMessage(messageId)
    }

    fun refreshMessages() {
        UALog.d { "UnityPlugin refreshMessages method call" }
        runBlocking(Dispatchers.IO) {
            airshipProxyInstance.messageCenter.refreshInbox()
        }
    }

    fun setAutoLaunchDefaultMessageCenter(enabled: Boolean) {
        UALog.d { "UnityPlugin setAutoLaunchDefaultMessageCenter method call with: $enabled" }
        airshipProxyInstance.messageCenter.setAutoLaunchDefaultMessageCenter(enabled)
    }

    fun displayMessageCenter(messageId: String?) {
        UALog.d { "UnityPlugin displayMessageCenter method call with: $messageId" }
        airshipProxyInstance.messageCenter.display(messageId)
    }

    fun dismissMessageCenter() {
        UALog.d { "UnityPlugin dismissMessageCenter method call" }
        airshipProxyInstance.messageCenter.dismiss()
    }

    fun showMessageView(messageId: String) {
        UALog.d { "UnityPlugin showMessageView method call with: $messageId" }
        airshipProxyInstance.messageCenter.showMessageView(messageId)
    }

    fun showMessageCenter(messageId: String?) {
        UALog.d { "UnityPlugin showMessageCenter method call with: $messageId" }
        airshipProxyInstance.messageCenter.showMessageCenter(messageId)
    }

    // Preference Center

    fun displayPreferenceCenter(preferenceCenterId: String) {
        UALog.d { "UnityPlugin displayPreferenceCenter method call with: $preferenceCenterId" }
        airshipProxyInstance.preferenceCenter.displayPreferenceCenter(preferenceCenterId)
    }

    fun getPreferenceCenterConfig(preferenceCenterId: String): String {
        UALog.d { "UnityPlugin getPreferenceCenterConfig method call with: $preferenceCenterId" }
        return runBlocking(Dispatchers.IO) {
            JsonValue.wrapOpt(airshipProxyInstance.preferenceCenter.getPreferenceCenterConfig(preferenceCenterId)).toString()
        }
    }

    fun setAutoLaunchDefaultPreferenceCenter(preferenceCenterId: String, autoLaunch: Boolean) {
        UALog.d { "UnityPlugin setAutoLaunchDefaultPreferenceCenter method call with: $preferenceCenterId, $autoLaunch" }
        airshipProxyInstance.preferenceCenter.setAutoLaunchPreferenceCenter(preferenceCenterId, autoLaunch)
    }

    // Privacy Manager

    fun setEnabledFeatures(features: Array<String>) {
        UALog.d { "UnityPlugin setEnabledFeatures method call with: ${features.joinToString()}" }
        airshipProxyInstance.privacyManager.setEnabledFeatures(features.asList())
    }

    fun getEnabledFeatures(): Array<String> {
        UALog.d { "UnityPlugin getEnabledFeatures method call" }
        return airshipProxyInstance.privacyManager.getFeatureNames().toTypedArray()
    }

    fun enableFeatures(features: Array<String>) {
        UALog.d { "UnityPlugin enableFeatures method call with: ${features.joinToString()}" }
        airshipProxyInstance.privacyManager.enableFeatures(features.asList())
    }

    fun disableFeatures(features: Array<String>) {
        UALog.d { "UnityPlugin disableFeatures method call with: ${features.joinToString()}" }
        airshipProxyInstance.privacyManager.disableFeatures(features.asList())
    }

    fun isFeaturesEnabled(features: Array<String>): Boolean {
        UALog.d { "UnityPlugin isFeaturesEnabled method call with: ${features.joinToString()}" }
        return airshipProxyInstance.privacyManager.isFeatureEnabled(features.asList())
    }

    // Push

    fun isUserNotificationsEnabled(): Boolean {
        UALog.d { "UnityPlugin isUserNotificationsEnabled method call" }
        return airshipProxyInstance.push.isUserNotificationsEnabled()
    }

    fun setUserNotificationsEnabled(enabled: Boolean) {
        UALog.d { "UnityPlugin setUserNotificationsEnabled method call with: $enabled" }
        airshipProxyInstance.push.setUserNotificationsEnabled(enabled)
    }

    fun enableUserNotifications(fallback: String?): Boolean {
        UALog.d { "UnityPlugin enableUserNotifications method call with: $fallback" }
        return runBlocking(Dispatchers.IO) {
            airshipProxyInstance.push.enableUserPushNotifications(
                EnableUserNotificationsArgs.fromJson(JsonValue.parseString(fallback))
            )
        }
    }

    fun getNotificationStatus(): String {
        UALog.d { "UnityPlugin getNotificationStatus method call" }
        return runBlocking(Dispatchers.IO) {
            airshipProxyInstance.push.getNotificationStatus().toJsonValue().toString()
        }
    }

    fun getPushToken(): String? {
        UALog.d { "UnityPlugin getPushToken method call" }
        return airshipProxyInstance.push.getRegistrationToken()
    }

    fun getActiveNotifications(): String {
        UALog.d { "UnityPlugin getActiveNotifications method call" }
        return JsonValue.wrapOpt(airshipProxyInstance.push.getActiveNotifications()).toString()
    }

    fun clearNotifications() {
        UALog.d { "UnityPlugin clearNotifications method call" }
        airshipProxyInstance.push.clearNotifications()
    }

    fun clearNotification(identifier: String) {
        UALog.d { "UnityPlugin clearNotification method call with: $identifier" }
        airshipProxyInstance.push.clearNotification(identifier)
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    fun isNotificationChannelEnabled(channelId: String): Boolean {
        UALog.d { "UnityPlugin isNotificationChannelEnabled method call with: $channelId" }
        return airshipProxyInstance.push.isNotificationChannelEnabled(channelId)
    }

    fun setNotificationConfig(config: String) {
        UALog.d { "UnityPlugin setNotificationConfig method call with: $config" }
        airshipProxyInstance.push.setNotificationConfig(JsonValue.parseString(config))
    }

    fun setForegroundNotificationsEnabled(enabled: Boolean) {
        UALog.d { "UnityPlugin setForegroundNotificationsEnabled method call with: $enabled" }
        airshipProxyInstance.push.isForegroundNotificationsEnabled = enabled
    }

    fun isForegroundNotificationsEnabled(): Boolean {
        UALog.d { "UnityPlugin isForegroundNotificationsEnabled method call" }
        return airshipProxyInstance.push.isForegroundNotificationsEnabled
    }

    fun runAction(name: String, value: String?): String {
        UALog.d { "UnityPlugin runAction method call with: $name, $value" }
        return runBlocking(Dispatchers.IO) {
            val actionResult = airshipProxyInstance.actions.runAction(name, JsonValue.parseString(value))
            JsonValue.wrapOpt(actionResult).toString()
        }
    }

    fun flag(name: String): String {
        UALog.d { "UnityPlugin flag method call with: $name" }
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
        UALog.d { "UnityPlugin trackInteraction method call with: $flag" }
        airshipProxyInstance.featureFlagManager.trackInteraction(FeatureFlagProxy(JsonValue.parseString(flag)))
    }

    // TODO finish the implementation (live activity and live update)

    fun onPushReceived(message: JsonValue?) {
        UALog.d { "UnityPlugin push received: $message" }

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPushReceived", message.toString())
        }
    }

    fun onPushOpened(message: JsonValue?) {
        UALog.d { "UnityPlugin push opened: $message" }

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPushOpened", message.toString())
        }
    }

    fun onDeepLinkReceived(deepLink: String?): Boolean {
        UALog.d { "UnityPlugin deepLink received: $deepLink" }

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnDeepLinkReceived", deepLink)
            return true
        }
        return false
    }

    fun onChannelCreated(channelId: String?) {
        UALog.d { "UnityPlugin channel created: $channelId" }

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnChannelCreated", channelId)
        }
    }

    fun onShowInbox(messageId: String?) {
        if (messageId == null) {
            UALog.d { "UnityPlugin show inbox" }

            if (listener != null) {
                UnityPlayer.UnitySendMessage(listener, "OnShowInbox", "")
            }
        } else {
            UALog.d { "UnityPlugin show inbox message: $messageId" }

            if (listener != null) {
                UnityPlayer.UnitySendMessage(listener, "OnShowInbox", messageId)
            }
        }
    }

    fun onInboxUpdated(messageUnreadCount: Int?, messageCount: Int?) {
        UALog.d { "UnityPlugin inboxUpdated (unread = $messageUnreadCount, total = $messageCount)" }

        if (messageUnreadCount == null) {
            UALog.e { "UnityPlugin failed to retrieve message unread count" }
        }
        if (messageCount == null) {
            UALog.e { "UnityPlugin failed to retrieve message count" }
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
        UALog.d { "UnityPlugin preference center display: $preferenceCenterId" }

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPreferenceCenterDisplay", preferenceCenterId)
        }
    }

    fun onPushTokenReceived(pushToken: String?) {
        UALog.d { "UnityPlugin push token received: $pushToken" }

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPushTokenReceived", pushToken)
        }
    }

    fun onNotificationStatusChanged(status: JsonValue?) {
        UALog.d { "UnityPlugin notification status changed: ${status?.toString()}" }

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnNotificationStatusChanged", status?.toString())
        }
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

        @JvmStatic
        fun shared(): UnityPlugin {
            return instance
        }
    }
}