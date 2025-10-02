/* Copyright Airship and Contributors */
package com.urbanairship.unityplugin

import com.unity3d.player.UnityPlayer
import com.urbanairship.PrivacyManager
import com.urbanairship.android.framework.proxy.ProxyLogger
import com.urbanairship.android.framework.proxy.proxies.AirshipProxy
import com.urbanairship.android.framework.proxy.proxies.EnableUserNotificationsArgs
import com.urbanairship.json.JsonException
import com.urbanairship.json.JsonMap
import com.urbanairship.json.JsonValue
import com.urbanairship.push.PushMessage
import com.urbanairship.util.UAStringUtil
import org.json.JSONArray


class UnityPlugin {

    private val airshipProxyInstance = AirshipProxy.shared(UnityPlayer.currentActivity.applicationContext)

    private var listener: String? = null

    fun setListener(listener: String) {
        ProxyLogger.debug("UnityPlugin setListener: $listener")
        this.listener = listener
    }

    // Airship

    fun takeOff(config: String): Boolean {
        ProxyLogger.debug("UnityPlugin takeOff: $config")
        return airshipProxyInstance.takeOff(JsonValue.parseString(config))
    }

    fun isFlying(): Boolean {
        ProxyLogger.debug("UnityPlugin isFlying")
        return airshipProxyInstance.isFlying()
    }

    // Channel
    
    fun getChannelId(): String? {
        ProxyLogger.debug("UnityPlugin getChannelId")
        return airshipProxyInstance.channel.getChannelId()
    }

    suspend fun waitForChannelId(): String {
        ProxyLogger.debug("UnityPlugin waitForChannelId")
        return airshipProxyInstance.channel.waitForChannelId()
    }

    fun addTag(tag: String) {
        ProxyLogger.debug("UnityPlugin addTag: $tag")
        airshipProxyInstance.channel.addTag(tag)
    }

    fun removeTag(tag: String) {
        ProxyLogger.debug("UnityPlugin removeTag: $tag")
        airshipProxyInstance.channel.removeTag(tag)
    }

    fun getTags(): String {
        ProxyLogger.debug("UnityPlugin getTags")
        val jsonArray = JSONArray()
        for (tag in airshipProxyInstance.channel.getTags()) {
            jsonArray.put(tag)
        }
        return jsonArray.toString()
    }

    fun editTags(payload: String) {
        ProxyLogger.debug("UnityPlugin editTags: $payload")
        try {
            airshipProxyInstance.channel.editTags(JsonValue.parseString(payload))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    fun editChannelTagGroups(payload: String) {
        ProxyLogger.debug("UnityPlugin editChannelTagGroups: $payload")
        try {
            airshipProxyInstance.channel.editTagGroups(JsonValue.parseString(payload))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    fun editChannelAttributes(payload: String) {
        ProxyLogger.debug("UnityPlugin editChannelAttributes: $payload")
        try {
            airshipProxyInstance.channel.editAttributes(JsonValue.parseString(payload))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    suspend fun getChannelSubscriptionLists(): String {
        ProxyLogger.debug("UnityPlugin getChannelSubscriptionLists")
        val jsonArray = JSONArray()
        for (tag in airshipProxyInstance.channel.getSubscriptionLists()) {
            jsonArray.put(tag)
        }
        return jsonArray.toString()
    }

    fun editChannelSubscriptionLists(payload: String) {
        ProxyLogger.debug("UnityPlugin editChannelSubscriptionLists: $payload")
        try {
            airshipProxyInstance.channel.editSubscriptionLists(JsonValue.parseString(payload))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    // Contact

    fun identify(namedUserId: String?) {
        ProxyLogger.debug("UnityPlugin identify: $namedUserId")
        airshipProxyInstance.contact.identify(namedUserId)
    }

    fun reset() {
        ProxyLogger.debug("UnityPlugin reset")
        airshipProxyInstance.contact.reset()
    }

    fun getNamedUserId(): String? {
        ProxyLogger.debug("UnityPlugin getNamedUserId")
        return airshipProxyInstance.contact.getNamedUserId()
    }

    fun notifyRemoteLogin() {
        ProxyLogger.debug("UnityPlugin notifyRemoteLogin")
        airshipProxyInstance.contact.notifyRemoteLogin()
    }

    fun editContactTagGroups(payload: String) {
        ProxyLogger.debug("UnityPlugin editContactTagGroups: $payload")
        try {
            airshipProxyInstance.contact.editTagGroups(JsonValue.parseString(payload))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    fun editContactAttributes(payload: String) {
        ProxyLogger.debug("UnityPlugin editContactAttributes: $payload")
        try {
            airshipProxyInstance.contact.editAttributes(JsonValue.parseString(payload))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    fun getContactSubscriptionLists(): String {
        ProxyLogger.debug("UnityPlugin getContactSubscriptionLists")

        // TODO finish this
        val jsonArray = JSONArray()
//        for (tag in airshipProxyInstance.contact.getSubscriptionLists()) {
//            jsonArray.put(tag)
//        }
        return jsonArray.toString()
    }

    fun editContactSubscriptionLists(payload: String) {
        ProxyLogger.debug("UnityPlugin editContactSubscriptionLists: $payload")
        try {
            airshipProxyInstance.contact.editSubscriptionLists(JsonValue.parseString(payload))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse payload", e)
        }
    }

    // Analytics

    fun associateIdentifier(key: String, identifier: String?) {
        if (identifier == null) {
            ProxyLogger.debug("UnityPlugin associateIdentifier removed identifier for key: $key")
        } else {
            ProxyLogger.debug("UnityPlugin associateIdentifier with identifier: $identifier for key: $key")
        }
        airshipProxyInstance.analytics.associateIdentifier(key, identifier)
    }

    fun trackScreen(screenName: String) {
        ProxyLogger.debug("UnityPlugin trackScreen: $screenName")
        airshipProxyInstance.analytics.trackScreen(screenName)
    }

    fun addCustomEvent(eventPayload: String) {
        ProxyLogger.debug("UnityPlugin addCustomEvent: $eventPayload")
        try {
            airshipProxyInstance.analytics.addEvent(JsonValue.parseString(eventPayload))
        } catch (e: JsonException) {
            ProxyLogger.error("Failed to parse event payload", e)
        }
    }

    fun getSessionId(): String {
        ProxyLogger.debug("UnityPlugin getSessionId")
        return airshipProxyInstance.analytics.getSessionId()
    }

    // InApp

    fun setPaused(paused: Boolean) {
        ProxyLogger.debug("UnityPlugin setPaused: $paused")
        airshipProxyInstance.inApp.setPaused(paused)
    }

    fun isPaused(): Boolean {
        ProxyLogger.debug("UnityPlugin isPaused")
        return airshipProxyInstance.inApp.isPaused()
    }

    fun setDisplayInterval(displayInterval: Long) {
        ProxyLogger.debug("UnityPlugin setDisplayInterval: $displayInterval")
        airshipProxyInstance.inApp.setDisplayInterval(displayInterval)
    }

    fun getDisplayInterval(): Long {
        ProxyLogger.debug("UnityPlugin getDisplayInterval")
        return airshipProxyInstance.inApp.getDisplayInterval()
    }

    // Locale

    fun setLocaleOverride(localeIdentifier: String) {
        ProxyLogger.debug("UnityPlugin setLocaleOverride: $localeIdentifier")
        airshipProxyInstance.locale.setCurrentLocale(localeIdentifier)
    }

    fun clearLocaleOverride() {
        ProxyLogger.debug("UnityPlugin clearLocaleOverride")
        airshipProxyInstance.locale.clearLocale()
    }

    fun getLocale(): String {
        ProxyLogger.debug("UnityPlugin getLocale")
        return airshipProxyInstance.locale.getCurrentLocale()
    }

    // Message Center

    suspend fun getUnreadCount(): Int {
        ProxyLogger.debug("UnityPlugin getUnreadCount")
        return airshipProxyInstance.messageCenter.getUnreadMessagesCount()
    }

    suspend fun getMessages(): String {
        ProxyLogger.debug("UnityPlugin getMessages")
        return JsonValue.wrapOpt(airshipProxyInstance.messageCenter.getMessages()).toString()
    }

    fun markMessageRead(messageId: String) {
        ProxyLogger.debug("UnityPlugin markMessageRead: $messageId")
        airshipProxyInstance.messageCenter.markMessageRead(messageId)
    }

    fun deleteMessage(messageId: String) {
        ProxyLogger.debug("UnityPlugin deleteMessage: $messageId")
        airshipProxyInstance.messageCenter.deleteMessage(messageId)
    }

    suspend fun refreshMessages() {
        ProxyLogger.debug("UnityPlugin refreshMessages")
        airshipProxyInstance.messageCenter.refreshInbox()
    }

    fun setAutoLaunchDefaultMessageCenter(enabled: Boolean) {
        ProxyLogger.debug("UnityPlugin setAutoLaunchDefaultMessageCenter: $enabled")
        airshipProxyInstance.messageCenter.setAutoLaunchDefaultMessageCenter(enabled)
    }

    fun displayMessageCenter(messageId: String?) {
        ProxyLogger.debug("UnityPlugin displayMessageCenter: $messageId")
        airshipProxyInstance.messageCenter.display(messageId)
    }

    fun dismissMessageCenter() {
        ProxyLogger.debug("UnityPlugin dismissMessageCenter")
        airshipProxyInstance.messageCenter.dismiss()
    }

    fun showMessageView(messageId: String) {
        ProxyLogger.debug("UnityPlugin showMessageView: $messageId")
        airshipProxyInstance.messageCenter.showMessageView(messageId)
    }

    fun showMessageCenter(messageId: String?) {
        ProxyLogger.debug("UnityPlugin showMessageCenter: $messageId")
        airshipProxyInstance.messageCenter.showMessageCenter(messageId)
    }

    // Preference Center

    fun displayPreferenceCenter(preferenceCenterId: String) {
        ProxyLogger.debug("UnityPlugin displayPreferenceCenter: $preferenceCenterId")
        airshipProxyInstance.preferenceCenter.displayPreferenceCenter(preferenceCenterId)
    }

    suspend fun getPreferenceCenterConfig(preferenceCenterId: String): String {
        ProxyLogger.debug("UnityPlugin getPreferenceCenterConfig: $preferenceCenterId")
        return JsonValue.wrapOpt(airshipProxyInstance.preferenceCenter.getPreferenceCenterConfig(preferenceCenterId)).toString()
    }

    fun setAutoLaunchDefaultPreferenceCenter(preferenceCenterId: String, autoLaunch: Boolean) {
        ProxyLogger.debug("UnityPlugin setAutoLaunchDefaultPreferenceCenter: $preferenceCenterId, $autoLaunch")
        airshipProxyInstance.preferenceCenter.setAutoLaunchPreferenceCenter(preferenceCenterId, autoLaunch)
    }

    // Privacy Manager

    fun setEnabledFeatures(features: Array<String>) {
        ProxyLogger.debug("UnityPlugin setEnabledFeatures: $features")
        airshipProxyInstance.privacyManager.setEnabledFeatures(features.asList())
    }

    fun getEnabledFeatures(): Array<String> {
        ProxyLogger.debug("UnityPlugin getEnabledFeatures")
        return airshipProxyInstance.privacyManager.getFeatureNames().toTypedArray()
    }

    fun enableFeatures(features: Array<String>) {
        ProxyLogger.debug("UnityPlugin enableFeatures: $features")
        airshipProxyInstance.privacyManager.enableFeatures(features.asList())
    }

    fun disableFeatures(features: Array<String>) {
        ProxyLogger.debug("UnityPlugin disableFeatures: $features")
        airshipProxyInstance.privacyManager.disableFeatures(features.asList())
    }

    fun isFeaturesEnabled(features: Array<String>): Boolean {
        ProxyLogger.debug("UnityPlugin isFeaturesEnabled: $features")
        return airshipProxyInstance.privacyManager.isFeatureEnabled(features.asList())
    }

    // Push

    fun isUserNotificationsEnabled(): Boolean {
        ProxyLogger.debug("UnityPlugin isUserNotificationsEnabled")
        return airshipProxyInstance.push.isUserNotificationsEnabled()
    }

    fun setUserNotificationsEnabled(enabled: Boolean) {
        ProxyLogger.debug("UnityPlugin setUserNotificationsEnabled: $enabled")
        airshipProxyInstance.push.setUserNotificationsEnabled(enabled)
    }

    suspend fun enableUserNotifications(fallback: String?): Boolean {
        ProxyLogger.debug("UnityPlugin enableUserNotifications: $fallback")
        return airshipProxyInstance.push.enableUserPushNotifications(
            EnableUserNotificationsArgs.fromJson(JsonValue.parseString(fallback))
        )
    }

    suspend fun getNotificationStatus(): String {
        ProxyLogger.debug("UnityPlugin getNotificationStatus")
        return airshipProxyInstance.push.getNotificationStatus().toJsonValue().toString()
    }

    fun getPushToken(): String? {
        ProxyLogger.debug("UnityPlugin getPushToken")
        return airshipProxyInstance.push.getRegistrationToken()
    }

    fun getActiveNotifications(): String {
        ProxyLogger.debug("UnityPlugin getActiveNotifications")
        return JsonValue.wrapOpt(airshipProxyInstance.push.getActiveNotifications()).toString()
    }

    fun clearNotifications() {
        ProxyLogger.debug("UnityPlugin clearNotifications")
        airshipProxyInstance.push.clearNotifications()
    }

    fun clearNotification(identifier: String) {
        ProxyLogger.debug("UnityPlugin clearNotification: $identifier")
        airshipProxyInstance.push.clearNotification(identifier)
    }

    // TODO Just noticed I forgot to implement the android specific push methods, I need to add that

    // TODO finish the implementation

    fun onPushReceived(message: PushMessage?) {
        ProxyLogger.debug("UnityPlugin push received: $message")

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPushReceived", getPushPayload(message))
        }
    }

    fun onPushOpened(message: PushMessage?) {
        ProxyLogger.debug("UnityPlugin push opened: $message")

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnPushOpened", getPushPayload(message))
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

    // TODO Implement the rest of the listeners (PreferenceCenter)

    suspend fun onInboxUpdated() {
        val unreadCount = getUnreadCount()
        val totalCount = getMessages().count()
        val counts: JsonMap = JsonMap.newBuilder()
            .put("unread", unreadCount)
            .put("total", totalCount)
            .build()
        ProxyLogger.debug(
            "UnityPlugin inboxUpdated (unread = %s, total = %s)",
            unreadCount, totalCount
        )

        if (listener != null) {
            UnityPlayer.UnitySendMessage(listener, "OnInboxUpdated", counts.toString())
        }
    }



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

    companion object {
        private val instance = UnityPlugin()

        private val featuresMap = mapOf(
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