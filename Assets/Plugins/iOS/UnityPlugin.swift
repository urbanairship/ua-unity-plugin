import Foundation
import SwiftUI
import AirshipFrameworkProxy

@_cdecl("UnityPlugin_call")
public func UnityPlugin_call(_ method: String, args: String) async throws -> (any Sendable)? {
    // TODO Check the method name and call the appropriate method.
    print("Hello Swift seems to work like that")

    switch method {
        // Airship
        case "takeOff":
            return try AirshipProxy.shared.takeOff(
                // TODO decode the arg
            )

        case "isFlying":
            return AirshipProxy.shared.isFlying()

        // Channel
        case "getChannelId":
            return try AirshipProxy.shared.channel.channelID

        case "waitForChannelId":
            return try await AirshipProxy.shared.channel.waitForChannelID()

        case "addTag":

        case "removeTag":

        case "getTags":
            return try AirshipProxy.shared.channel.tags

        case "editTags":

        case "editChannelTagGroups":

        case "editChannelAttributes":

        case "getChannelSubscriptionLists":
            return try await AirshipProxy.shared.channel.fetchSubscriptionLists()

        case "editChannelSubscriptionLists":
        

        // Contact
        case "identify":
        case "reset":
        case "getNamedUserId":
        case "notifyRemoteLogin":
        case "editContactTagGroups":
        case "editContactAttributes":
        case "getContactSubscriptionLists":
        case "editContactSubscriptionLists":

        // Analytics
        case "associateIdentifier":
        case "trackScreen":
        case "addCustomEvent":
        case "getSessionId":

        // InApp
        case "setPaused":
        case "isPaused":
        case "setDisplayInterval":
        case "getDisplayInterval":

        // Locale
        case "setLocaleOverride":
        case "clearLocaleOverride":
        case "getLocale":

        // Message Center
        case "getUnreadCount":
        case "getMessages":
        case "markMessageRead":
        case "deleteMessage":
        case "refreshMessages":
        case "setAutoLaunchDefaultMessageCenter":
        case "displayMessageCenter":
        case "dismissMessageCenter":
        case "showMessageView":
        case "showMessageCenter":

        // Preference Center
        case "displayPreferenceCenter":
        case "getPreferenceCenterConfig":
        case "setAutoLaunchDefaultPreferenceCenter":

        // Privacy Manager
        case "setEnabledFeatures":
        case "getEnabledFeatures":
        case "enableFeatures":
        case "disableFeatures":
        case "isFeaturesEnabled":

        // Push
        case "isUserNotificationsEnabled":
        case "setUserNotificationsEnabled":
        case "enableUserNotifications":
        case "getNotificationStatus":
        case "getPushToken":
        case "getActiveNotifications":
        case "clearNotifications":
        case "clearNotification":
    }
}