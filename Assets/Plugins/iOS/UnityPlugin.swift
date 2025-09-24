import Foundation
import SwiftUI
import AirshipFrameworkProxy

@_cdecl("UnityPlugin_call")
public func UnityPlugin_call(_ method: String, args: Any...) async throws -> (any Sendable)? {

    AirshipLogger.debug("UnityPlugin \(method): \(args.first)")

    switch method {
        case "setListener":
            let listener = requireAnyString(args.first)
            // TODO

        // Airship
        case "takeOff":
            return try AirshipProxy.shared.takeOff(json: requireAnyArg(args.first))

        case "isFlying":
            return AirshipProxy.shared.isFlying()

        // Channel
        case "getChannelId":
            return try AirshipProxy.shared.channel.channelID

        case "waitForChannelId":
            return try await AirshipProxy.shared.channel.waitForChannelID()

        case "addTag":
            try AirshipProxy.shared.channel.addTags(requireStringArg(args.first))
            return nil

        case "removeTag":
            try AirshipProxy.shared.channel.removeTags(requireStringArg(args.first))
            return nil

        case "getTags":
            return try AirshipProxy.shared.channel.tags

        case "editTags":
            try AirshipProxy.shared.channel.editTags(
                operations: try requireCodableArg(args.first)
            )
            return nil

        case "editChannelTagGroups":
            try AirshipProxy.shared.channel.editTagGroups(
                operations: try requireCodableArg(args.first)
            )
            return nil

        case "editChannelAttributes":
            try AirshipProxy.shared.channel.editAttributes(
                operations: try requireCodableArg(args.first)
            )
            return nil

        case "getChannelSubscriptionLists":
            return try await AirshipProxy.shared.channel.fetchSubscriptionLists()

        case "editChannelSubscriptionLists":
            try AirshipProxy.shared.channel.editSubscriptionLists(
                json: try requireAnyArg(args.first)
            )
            return nil

        // Contact
        case "identify":
            try AirshipProxy.shared.contact.identify(try requireStringArg(args.first))
            return nil

        case "reset":
            try AirshipProxy.shared.contact.reset()
            return nil

        case "getNamedUserId":
            return try await AirshipProxy.shared.contact.namedUserID

        case "notifyRemoteLogin":
            try AirshipProxy.shared.contact.notifyRemoteLogin()
            return nil

        case "editContactTagGroups":
            try AirshipProxy.shared.contact.editTagGroups(
                operations: try requireCodableArg(args.first)
            )
            return nil

        case "editContactAttributes":
            try AirshipProxy.shared.contact.editAttributes(
                operations: try requireCodableArg(args.first)
            )
            return nil

        case "getContactSubscriptionLists":
            return try await AirshipProxy.shared.contact.getSubscriptionLists()

        case "editContactSubscriptionLists":
            try AirshipProxy.shared.contact.editSubscriptionLists(
                operations: try requireCodableArg(args.first)
            )
            return nil

        // Analytics
        case "associateIdentifier":
            guard args.count == 1 || args.count == 2 else {
                throw AirshipErrors.error("associateIdentifier call requires 1 to 2 strings parameters.")
            }
            try AirshipProxy.shared.analytics.associateIdentifier(
                identifier: args.count == 2 ? args[1] : nil,
                key: args[0]
            )
            return nil

        case "trackScreen":
            try AirshipProxy.shared.analytics.trackScreen(
                try? requireStringArg(args.first)
            )
            return nil

        case "addCustomEvent":
            try AirshipProxy.shared.analytics.addEvent(
                requireAnyArg(args.first)
            )
            return nil

        case "getSessionId":
            return try AirshipProxy.shared.analytics.getSessionID()

        // InApp
        case "setPaused":
            try AirshipProxy.shared.inApp.setPaused(try requireBooleanArg(args.first))
            return nil

        case "isPaused":
            return try AirshipProxy.shared.inApp.isPaused()

        case "setDisplayInterval":
            try AirshipProxy.shared.inApp.setDisplayInterval(
                milliseconds: try requireIntArg(args.first)
            )
            return nil

        case "getDisplayInterval":
            return try AirshipProxy.shared.inApp.getDisplayInterval()

        // Locale
        case "setLocaleOverride":
            try AirshipProxy.shared.locale.setCurrentLocale(
                try requireStringArg(args.first)
            )
            return nil

        case "clearLocaleOverride":
            try AirshipProxy.shared.locale.clearLocale()
            return nil

        case "getLocale":
            return try AirshipProxy.shared.locale.currentLocale

        // Message Center
        case "getUnreadCount":
            return try await AirshipProxy.shared.messageCenter.unreadCount

        case "getMessages":
            return try await AirshipProxy.shared.messageCenter.messages

        case "markMessageRead":
            try await AirshipProxy.shared.messageCenter.markMessageRead(
                messageID: requireStringArg(args.first)
            )
            return nil

        case "deleteMessage":
            try await AirshipProxy.shared.messageCenter.deleteMessage(
                messageID: requireStringArg(args.first)
            )
            return nil

        case "refreshMessages":
            try await AirshipProxy.shared.messageCenter.refresh()
            return nil

        case "setAutoLaunchDefaultMessageCenter":
            AirshipProxy.shared.messageCenter.setAutoLaunchDefaultMessageCenter(
                try requireBooleanArg(args.first)
            )
            return nil

        case "displayMessageCenter":
            try AirshipProxy.shared.messageCenter.display(
                messageID: try? requireStringArg(args.first)
            )
            return nil

        case "dismissMessageCenter":
            try AirshipProxy.shared.messageCenter.dismiss()
            return nil

        case "showMessageView":
            try AirshipProxy.shared.messageCenter.showMessageView(
                messageID: try requireStringArg(args.first)
            )
            return nil

        case "showMessageCenter":
            try AirshipProxy.shared.messageCenter.showMessageCenter(
                messageID: try? requireStringArg(args.first)
            )
            return nil

        // Preference Center
        case "displayPreferenceCenter":
            try AirshipProxy.shared.preferenceCenter.displayPreferenceCenter(
                preferenceCenterID: try requireStringArg(args.first)
            )
            return nil

        case "getPreferenceCenterConfig":
            return try await AirshipProxy.shared.preferenceCenter.getPreferenceCenterConfig(
                preferenceCenterID: try requireStringArg(args.first)
            )

        case "setAutoLaunchDefaultPreferenceCenter":
            guard
                args.count == 2,
                let identifier: String = args[0] as? String,
                let autoLaunch: Bool = args[1] as? Bool
            else {
                throw AirshipErrors.error("setAutoLaunchDefaultPreferenceCenter call requires [String, Bool]")
            }

            AirshipProxy.shared.preferenceCenter.setAutoLaunchPreferenceCenter(
                autoLaunch,
                preferenceCenterID: identifier
            )
            return nil

        // Privacy Manager
        case "setEnabledFeatures":
            try AirshipProxy.shared.privacyManager.setEnabled(
                featureNames: try requireStringArrayArg(args.first)
            )
            return nil

        case "getEnabledFeatures":
            return try AirshipProxy.shared.privacyManager.getEnabledNames()
            
        case "enableFeatures":
            try AirshipProxy.shared.privacyManager.enable(
                featureNames: try requireStringArrayArg(args.first)
            )
            return nil

        case "disableFeatures":
            try AirshipProxy.shared.privacyManager.disable(
                featureNames: try requireStringArrayArg(args.first)
            )
            return nil

        case "isFeaturesEnabled":
            return try AirshipProxy.shared.privacyManager.isEnabled(
                featuresNames: try requireStringArrayArg(args.first)
            )

        // Push
        case "isUserNotificationsEnabled":
            return try AirshipProxy.shared.push.isUserNotificationsEnabled()

        case "setUserNotificationsEnabled":
            try AirshipProxy.shared.push.setUserNotificationsEnabled(
                try requireBooleanArg(args.first)
            )
            return nil

        case "enableUserNotifications":
            return try await AirshipProxy.shared.push.enableUserPushNotifications(
                args: try optionalCodableArg(args.first)
            )

        case "getNotificationStatus":
            return try await AirshipProxy.shared.push.notificationStatus

        case "getPushToken":
            return try AirshipProxy.shared.push.getRegistrationToken()

        case "getActiveNotifications":
            return try await AirshipProxy.shared.push.getActiveNotifications()

        case "clearNotifications":
            AirshipProxy.shared.push.clearNotifications()
            return nil

        case "clearNotification":
            AirshipProxy.shared.push.clearNotification(
                try requireStringArg(args.first)
            )
            return nil

        case "setForegroundPresentationOptions":
            try AirshipProxy.shared.push.setForegroundPresentationOptions(
                names: try requireStringArrayArg(args.first)
            )
            return nil

        case "setNotificationOptions":
            try AirshipProxy.shared.push.setNotificationOptions(
                names: try requireStringArrayArg(args.first)
            )
            return nil

        case "isAutobadgeEnabled":
            return try AirshipProxy.shared.push.isAutobadgeEnabled()

        case "setAutobadgeEnabled":
            try AirshipProxy.shared.push.setAutobadgeEnabled(
                try requireBooleanArg(args.first)
            )
            return nil

        case "setBadgeNumber":
            try await AirshipProxy.shared.push.setBadgeNumber(
                try requireIntArg(args.first)
            )
            return nil

        case "getBadgeNumber":
            return try AirshipProxy.shared.push.getBadgeNumber()

        case "setQuietTimeEnabled":
            try AirshipProxy.shared.push.setQuietTimeEnabled(
                try requireBooleanArg(args.first)
            )
            return nil

        case "isQuietTimeEnabled":
            return try AirshipProxy.shared.push.isQuietTimeEnabled()

        case "setQuietTime":
            try AirshipProxy.shared.push.setQuietTime(
                try requireCodableArg(args.first)
            )
            return nil

        case "getQuietTime":
            return try AirshipJSON.wrap(try AirshipProxy.shared.push.getQuietTime())
    }
}

private func requireAnyArg(_ arg: Any) throws -> Any {
    guard let value: Any = arg else {
        throw AirshipErrors.error("Argument must not be null")
    }
    return value
}

private func requireStringArg(_ arg: Any) throws -> String {
    guard let value: String = arg as? String else {
        throw AirshipErrors.error("Argument must be a string")
    }
    return value
}

private func requireBoolArg(_ arg: Any) throws -> Bool {
    guard let value: Bool = arg as? Bool else {
        throw AirshipErrors.error("Argument must be a bool")
    }
    return value
}

private func requireIntArg(_ arg: Any) throws -> Int {
    let value = try requireAnyArg()

    if let int = value as? Int {
        return int
    }

    if let double = value as? Double {
        return Int(double)
    }

    if let number = value as? NSNumber {
        return number.intValue
    }

    throw AirshipErrors.error("Argument must be an int")
}

private func requireDoubleArg(_ arg: Any) throws -> Double {
    let value = try requireAnyArg()

    if let double = value as? Double {
        return double
    }

    if let int = value as? Int {
        return Double(int)
    }

    if let number = value as? NSNumber {
        return number.doubleValue
    }

    throw AirshipErrors.error("Argument must be a double")
}

private func requireCodableArg<T: Decodable>(_ arg: Any) throws -> T  {
    guard let value: Any = arg else {
        throw AirshipErrors.error("Missing argument")
    }
    return try AirshipJSON.wrap(value).decode()
}

private func optionalCodableArg<T: Decodable>(_ arg: Any) throws -> T?  {
    guard let value: Any = arg else {
        return nil
    }
    return try AirshipJSON.wrap(value).decode()
}

private func requireStringArrayArg(_ arg: Any) throws -> [String] {
    guard let value: [String] = arg as? [String] else {
        throw AirshipErrors.error("Argument must be a string array")
    }
    return value
}