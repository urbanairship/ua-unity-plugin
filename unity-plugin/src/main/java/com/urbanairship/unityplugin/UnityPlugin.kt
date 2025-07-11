/* Copyright Airship and Contributors */
package com.urbanairship.unityplugin

import com.unity3d.player.UnityPlayer
import com.unity3d.player.a.a
import com.urbanairship.UAirship
import com.urbanairship.android.framework.proxy.ProxyLogger
import com.urbanairship.android.framework.proxy.TagGroupOperation
import com.urbanairship.android.framework.proxy.proxies.AirshipProxy
import com.urbanairship.json.JsonException
import com.urbanairship.json.JsonValue
import org.json.JSONArray

class UnityPlugin {

    private val airshipProxyInstance = AirshipProxy.shared(UnityPlayer.currentActivity.applicationContext)

    private var listener: String? = nul

    fun setListener(listener: String) {
        ProxyLogger.debug("UnityPlugin setListener: $listener")
        this.listener = listener
    }

    // Push

    fun getUserNotificationsEnabled(): Boolean {
        ProxyLogger.debug("UnityPlugin getUserNotificationsEnabled")
        return airshipProxyInstance.push.isUserNotificationsEnabled()
    }

    fun setUserNotificationsEnabled(enabled: Boolean) {
        ProxyLogger.debug("UnityPlugin setUserNotificationsEnabled: $enabled")
        airshipProxyInstance.push.setUserNotificationsEnabled(enabled)
    }

    // Channel
    
    fun getChannelId(): String? {
        ProxyLogger.debug("UnityPlugin getChannelId")
        return airshipProxyInstance.channel.getChannelId()
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

    //fun editTagGroups(operations: List<TagGroupOperation>)

    // TODO finish the implementation using the proxy

    companion object {
        private val instance = UnityPlugin()

        @JvmStatic
        fun shared(): UnityPlugin {
            return instance
        }
    }
}