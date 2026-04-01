/* Copyright Airship and Contributors */

import Foundation
import SwiftUI
import AirshipFrameworkProxy
import AirshipUnityCBridge

#if canImport(AirshipKit)
import AirshipKit
#elseif canImport(AirshipCore)
import AirshipCore
#endif

private let _notHandled = "NOT_HANDLED"

@_cdecl("UnityPlugin_call")
public func UnityPlugin_call(_ method: UnsafePointer<CChar>, argsJson: UnsafePointer<CChar>) -> UnsafePointer<CChar>? {
    let methodStr = String(cString: method)
    let argsJsonStr = String(cString: argsJson)

    let args: [Any]
    do {
        let data = argsJsonStr.data(using: .utf8) ?? Data()
        args = (try JSONSerialization.jsonObject(with: data) as? [Any]) ?? []
    } catch {
        AirshipLogger.error("Failed to deserialize arguments for method \(methodStr): \(error)")
        return UnsafePointer(strdup("{}"))
    }

    var result: Any?

    // Try sync path
    if Thread.isMainThread {
        do {
            result = try UnityPlugin.shared.handleCall(method: methodStr, args: args)
        } catch {
            AirshipLogger.error("Error executing method \(methodStr): \(error)")
            return UnsafePointer(strdup("{}"))
        }
    }
    
    // Fall through to async path
    if !Thread.isMainThread || (result as? String) == _notHandled {
        let semaphore = DispatchSemaphore(value: 0)
        var callError: Error?
        result = nil

        Task {
            do {
                result = try await UnityPlugin.shared.handleCallAsync(method: methodStr, args: args)
            } catch {
                callError = error
            }
            semaphore.signal()
        }

        if Thread.isMainThread {
            // Spin the RunLoop instead of blocking, so @MainActor work can execute
            while semaphore.wait(timeout: .now()) == .timedOut {
                RunLoop.current.run(mode: .default, before: Date(timeIntervalSinceNow: 0.005))
            }
        } else {
            semaphore.wait()
        }
        
        if let callError {
            AirshipLogger.error("Error executing method \(methodStr): \(callError)")
            return UnsafePointer(strdup("{}"))
        }
    }

    do {
        let jsonResult = try AirshipJSON.wrap(result).toString()
        return UnsafePointer(strdup(jsonResult))
    } catch {
        return UnsafePointer(strdup("{}"))
    }
}

class UnityPlugin: NSObject {

    static let shared = UnityPlugin()

    public var listener: String? = nil
    public var storedDeepLink: String? = nil

    private override init() {
        super.init()
    }

    private static let initializeOnce: Void = {
        // Add Notification Observer
        NotificationCenter.default.addObserver(forName: UIApplication.didFinishLaunchingNotification,
                                               object: nil,
                                               queue: nil) { notification in
            // TODO let's see how we handle take off
            // UnityPlugin.performTakeOff(withLaunchOptions: notification.userInfo)
        }
    }()

    func handleCall(method: String, args: [Any]) throws -> Any? {
        AirshipLogger.debug("UnityPlugin \(method): \(args)")

        // TODO check how to handle the class attributes called in the static method
        // let instance = shared

        switch method {
            case "setListener":
                // shared.listener = requireAnyString(args.first)
                listener = try requireStringArg(args.first)
                return nil

            // Check if we still need this
//            case "getDeepLink":
//                let deepLink = convertToJson(storedDeepLink)
//                if (requireBoolArg(args.first)) {
//                    storedDeepLink = nil
//                }
//                return deepLink

            // Airship
            case "takeOff":
                return try MainActor.assumeIsolated {
                    return try AirshipProxy.shared.takeOff(json: try requireParsedAnyArg(args.first))
                }

            case "isFlying":
                return AirshipProxy.shared.isFlying()

            // Channel
            case "getChannelId":
                return try AirshipProxy.shared.channel.channelID

            case "addTag":
                try AirshipProxy.shared.channel.addTags([requireStringArg(args.first)])
                return nil

            case "removeTag":
                try AirshipProxy.shared.channel.removeTags([requireStringArg(args.first)])
                return nil

            case "getTags":
                return try AirshipProxy.shared.channel.tags

            case "editTags":
                try AirshipProxy.shared.channel.editTags(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return nil

            case "editChannelTagGroups":
                try AirshipProxy.shared.channel.editTagGroups(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return nil

            case "editChannelAttributes":
                try AirshipProxy.shared.channel.editAttributes(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return nil

            case "editChannelSubscriptionLists":
                let parsed = try requireParsedAnyArg(args.first)
                guard let dict = parsed as? [String: Any], let values = dict["values"] else {
                    throw AirshipErrors.error("Missing 'values' key in JSON wrapper")
                }
                try AirshipProxy.shared.channel.editSubscriptionLists(
                    json: values
                )
                return nil

            // Contact
            case "identify":
                try AirshipProxy.shared.contact.identify(try requireStringArg(args.first))
                return nil

            case "reset":
                try AirshipProxy.shared.contact.reset()
                return nil

            case "notifyRemoteLogin":
                try AirshipProxy.shared.contact.notifyRemoteLogin()
                return nil

            case "editContactTagGroups":
                try AirshipProxy.shared.contact.editTagGroups(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return nil

            case "editContactAttributes":
                try AirshipProxy.shared.contact.editAttributes(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return nil

            case "editContactSubscriptionLists":
                try AirshipProxy.shared.contact.editSubscriptionLists(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return nil

            // Analytics
            case "associateIdentifier":
                guard args.count == 1 || args.count == 2 else {
                    throw AirshipErrors.error("associateIdentifier call requires 1 to 2 strings parameters.")
                }
                try AirshipProxy.shared.analytics.associateIdentifier(
                    identifier: args.count == 2 ? requireStringArg(args[1]) : nil,
                    key: requireStringArg(args[0])
                )
                return nil

            case "trackScreen":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.analytics.trackScreen(
                        try? requireStringArg(args.first)
                    )
                }
                return nil

            case "addCustomEvent":
                try AirshipProxy.shared.analytics.addEvent(
                    try requireParsedAnyArg(args.first)
                )
                return nil

            case "getSessionId":
                return try MainActor.assumeIsolated {
                    try AirshipProxy.shared.analytics.getSessionID()
                }

            // InApp
            case "setPaused":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.inApp.setPaused(try requireBoolArg(args.first))
                }
                return nil

            case "isPaused":
                return try MainActor.assumeIsolated {
                    try AirshipProxy.shared.inApp.isPaused()
                }

            case "getDisplayInterval":
                return try MainActor.assumeIsolated {
                    try AirshipProxy.shared.inApp.getDisplayInterval()
                }

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
            case "setAutoLaunchDefaultMessageCenter":
                try MainActor.assumeIsolated {
                    AirshipProxy.shared.messageCenter.setAutoLaunchDefaultMessageCenter(
                        try requireBoolArg(args.first)
                    )
                }
                return nil

            case "displayMessageCenter":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.messageCenter.display(
                        messageID: try? requireStringArg(args.first)
                    )
                }
                return nil

            case "dismissMessageCenter":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.messageCenter.dismiss()
                }
                return nil

            case "showMessageView":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.messageCenter.showMessageView(
                        messageID: try requireStringArg(args.first)
                    )
                }
                return nil

            case "showMessageCenter":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.messageCenter.showMessageCenter(
                        messageID: try? requireStringArg(args.first)
                    )
                }
                return nil

            // Preference Center
            case "displayPreferenceCenter":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.preferenceCenter.displayPreferenceCenter(
                        preferenceCenterID: try requireStringArg(args.first)
                    )
                }
                return nil

            case "setAutoLaunchDefaultPreferenceCenter":
                guard
                    args.count == 2,
                    let identifier: String = args[0] as? String,
                    let autoLaunch: Bool = args[1] as? Bool
                else {
                    throw AirshipErrors.error("setAutoLaunchDefaultPreferenceCenter call requires [String, Bool]")
                }

                MainActor.assumeIsolated {
                    AirshipProxy.shared.preferenceCenter.setAutoLaunchPreferenceCenter(
                        autoLaunch,
                        preferenceCenterID: identifier
                    )
                }
                return nil

            // Privacy Manager
            case "setEnabledFeatures":
                try AirshipProxy.shared.privacyManager.setEnabled(
                    featureNames: try requireStringArrayArg(args)
                )
                return nil

            case "getEnabledFeatures":
                return try AirshipProxy.shared.privacyManager.getEnabledNames()
                
            case "enableFeatures":
                try AirshipProxy.shared.privacyManager.enable(
                    featureNames: try requireStringArrayArg(args)
                )
                return nil

            case "disableFeatures":
                try AirshipProxy.shared.privacyManager.disable(
                    featureNames: try requireStringArrayArg(args)
                )
                return nil

            case "isFeaturesEnabled":
                return try AirshipProxy.shared.privacyManager.isEnabled(
                    featuresNames: try requireStringArrayArg(args)
                )

            // Push
            case "isUserNotificationsEnabled":
                return try AirshipProxy.shared.push.isUserNotificationsEnabled()

            case "setUserNotificationsEnabled":
                try AirshipProxy.shared.push.setUserNotificationsEnabled(
                    try requireBoolArg(args.first)
                )
                return nil

            case "getPushToken":
                return try MainActor.assumeIsolated {
                    try AirshipProxy.shared.push.getRegistrationToken()
                }

            case "clearNotifications":
                AirshipProxy.shared.push.clearNotifications()
                return nil

            case "clearNotification":
                AirshipProxy.shared.push.clearNotification(
                    try requireStringArg(args.first)
                )
                return nil

            // Push iOS
            case "setForegroundPresentationOptions":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.push.setForegroundPresentationOptions(
                        names: try requireStringArrayArg(args.first)
                    )
                }
                return nil

            case "setNotificationOptions":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.push.setNotificationOptions(
                        names: try requireStringArrayArg(args.first)
                    )
                }
                return nil

            case "isAutobadgeEnabled":
                return try AirshipProxy.shared.push.isAutobadgeEnabled()

            case "setAutobadgeEnabled":
                try AirshipProxy.shared.push.setAutobadgeEnabled(
                    try requireBoolArg(args.first)
                )
                return nil

            case "getBadgeNumber":
                return try MainActor.assumeIsolated {
                    try AirshipProxy.shared.push.getBadgeNumber()
                }

            case "setQuietTimeEnabled":
                try AirshipProxy.shared.push.setQuietTimeEnabled(
                    try requireBoolArg(args.first)
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
                return try AirshipProxy.shared.push.getQuietTime()

            case "trackInteraction":
                try AirshipProxy.shared.featureFlagManager.trackInteraction(
                    flag: try AirshipJSON.wrap(requireParsedAnyArg(args.first)).decode()
                )
                return nil
            
            default:
                return _notHandled
        }
    }

    func handleCallAsync(method: String, args: [Any]) async throws -> Any? {
        AirshipLogger.debug("UnityPlugin async \(method): \(args)")

        switch method {
            case "waitForChannelId":
                return try await AirshipProxy.shared.channel.waitForChannelID()

            case "getChannelSubscriptionLists":
                return try await AirshipProxy.shared.channel.fetchSubscriptionLists()

            case "getContactSubscriptionLists":
                let subscriptionLists = try await AirshipProxy.shared.contact.getSubscriptionLists()
                let subscriptionListsData = try AirshipJSON.wrap(subscriptionLists).toString().data(using: .utf8) ?? Data()
                let subscriptionListsDict = try JSONSerialization.jsonObject(with: subscriptionListsData) as? [String: [String]] ?? [:]
                var resultArray: [[String: Any]] = []
                for (listId, scopes) in subscriptionListsDict {
                    resultArray.append(["listId": listId, "scopes": scopes])
                }
                return resultArray
            
            case "getNamedUserId":
                return try await AirshipProxy.shared.contact.namedUserID

            case "setDisplayInterval":
                try await AirshipProxy.shared.inApp.setDisplayInterval(
                    milliseconds: try requireIntArg(args.first)
                )
                return nil
            
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

            case "getPreferenceCenterConfig":
                return try await AirshipProxy.shared.preferenceCenter.getPreferenceCenterConfig(
                    preferenceCenterID: try requireStringArg(args.first)
                )

            case "enableUserNotifications":
                return try await AirshipProxy.shared.push.enableUserPushNotifications(
                    args: try optionalCodableArg(args.first)
                )

            case "getNotificationStatus":
                return try await AirshipProxy.shared.push.notificationStatus

            case "getActiveNotifications":
                return try await AirshipProxy.shared.push.getActiveNotifications()

            case "setBadgeNumber":
                try await AirshipProxy.shared.push.setBadgeNumber(
                    try requireIntArg(args.first)
                )
                return nil

            case "runAction":
                return try await AirshipProxy.shared.action.runAction(
                    try requireStringArg(args.first),
                    value: try? AirshipJSON.wrap(try requireStringArg(args[1]))
                )

            case "flag":
                let flag = try await AirshipProxy.shared.featureFlagManager.flag(
                    name: try requireStringArg(args.first)
                )
                let flagJsonData = try AirshipJSON.wrap(flag).toString().data(using: .utf8) ?? Data()
                let flagDict = try JSONSerialization.jsonObject(with: flagJsonData) as? [String: Any] ?? [:]
                
                var result: [String: Any] = [:]
                result["isEligible"] = flagDict["isEligible"] as? Bool ?? false
                result["exists"] = flagDict["exists"] as? Bool ?? false
            
                // Stringify the nested objects so Unity's JsonUtility can deserialize them
                if let internalObj = flagDict["_internal"],
                   let internalData = try? JSONSerialization.data(withJSONObject: internalObj) {
                    result["_internal"] = String(data: internalData, encoding: .utf8) ?? ""
                }
            
                if let variablesObj = flagDict["variables"],
                   let variablesData = try? JSONSerialization.data(withJSONObject: variablesObj) {
                    result["variables"] = String(data: variablesData, encoding: .utf8) ?? ""
                }
            
                return result

            default:
                return nil
        }
    }

    // Push Notification Delegates

    public func receivedForegroundNotification(_ userInfo: [AnyHashable: Any], completionHandler: @escaping () -> Void) {
        AirshipLogger.debug("UnityPlugin receivedForegroundNotification \(userInfo)")

        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnPushReceived",
                                 message: convertPushToJson(push: userInfo)
            )
            completionHandler()
        }
    }

    public func receivedNotificationResponse(_ notificationResponse: UNNotificationResponse, completionHandler: @escaping () -> Void) {
        AirshipLogger.debug("UnityPlugin receivedNotificationResponse \(notificationResponse)")

        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnPushOpened",
                                 message: convertPushToJson(
                                    push: notificationResponse.notification.request.content.userInfo
                                 )
            )
            completionHandler()
        }
    }

    // Airship DeepLink Delegate

    public func receivedDeepLink(_ url: URL, completionHandler: @escaping () -> Void) {
        AirshipLogger.debug("UnityPlugin receivedDeepLink \(url)")

        let deepLinkString = url.absoluteString
        self.storedDeepLink = deepLinkString

        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnDeepLinkReceived",
                                 message: deepLinkString
            )
        }
        completionHandler()
    }

    // Channel Registration Events

    public func channelCreated(_ notification: Notification) {
        guard let channelID = notification.userInfo?[AirshipNotifications.ChannelCreated.channelIDKey] as? String else {
            return
        }
        AirshipLogger.debug("UnityPlugin channelCreated: \(channelID)")
        
        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnChannelCreated",
                                 message: channelID
            )
        }
    }

    // Inbox Message List Updated Notification
    public func inboxUpdated() async {
        do {
            let unreadCount =  try await AirshipProxy.shared.messageCenter.unreadCount
            let totalCount = try await AirshipProxy.shared.messageCenter.messages.count
        
            let counts : [String: Any] = [
                "unread": unreadCount,
                "total": totalCount
            ]

            AirshipLogger.debug("UnityPlugin inboxUpdated(unread = \(unreadCount), total = \(totalCount))")

            if let listener = self.listener {
                callUnitySendMessage(objectName: listener,
                                     methodName: "OnInboxUpdated",
                                     message: convertToJson(counts)
                )
            }
        } catch {
            AirshipLogger.error("Error executing method inboxUpdated: \(error)")
            return
        }
    }

    // TODO Message Center Display Delegates

    // TODO Implement the rest of the delegates (PC and AuthorizedSettings)

    private func requireAnyArg(_ arg: Any? = nil) throws -> Any {
        guard let value: Any = arg else {
            throw AirshipErrors.error("Argument must not be null")
        }
        return value
    }

    private func requireParsedAnyArg(_ arg: Any?) throws -> Any {
        guard let value = arg else {
            throw AirshipErrors.error("Missing argument")
        }
        if let jsonString = value as? String,
           let data = jsonString.data(using: .utf8) {
            return try JSONSerialization.jsonObject(with: data)
        }
        return value
    }

    private func requireStringArg(_ arg: Any?) throws -> String {
        guard let value: String = arg as? String else {
            throw AirshipErrors.error("Argument must be a string")
        }
        return value
    }

    private func requireBoolArg(_ arg: Any?) throws -> Bool {
        guard let value: Bool = arg as? Bool else {
            throw AirshipErrors.error("Argument must be a bool")
        }
        return value
    }

    private func requireIntArg(_ arg: Any?) throws -> Int {
        let value = try requireAnyArg(arg)

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

    private func requireDoubleArg(_ arg: Any?) throws -> Double {
        let value = try requireAnyArg(arg)

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

    private func requireCodableArg<T: Decodable>(_ arg: Any? = nil) throws -> T  {
        guard let value: Any = arg else {
            throw AirshipErrors.error("Missing argument")
        }
        return try AirshipJSON.wrap(value).decode()
    }

    private func optionalCodableArg<T: Decodable>(_ arg: Any? = nil) throws -> T?  {
        guard let value: Any = arg else {
            return nil
        }
        if let stringValue = value as? String,
           let data = stringValue.data(using: .utf8),
           let parsed = try? JSONSerialization.jsonObject(with: data) {
            return try AirshipJSON.wrap(parsed).decode()
        }
        return try AirshipJSON.wrap(value).decode()
    }

    private func requireParsedArgWithValues<T: Decodable>(_ arg: Any?) throws -> T {
        guard let value = arg else {
            throw AirshipErrors.error("Missing argument")
        }
        guard let jsonString = value as? String else {
            throw AirshipErrors.error("Argument must be a JSON string")
        }
        guard let data = jsonString.data(using: .utf8) else {
            throw AirshipErrors.error("Failed to encode JSON string to UTF-8")
        }
        let parsed = try JSONSerialization.jsonObject(with: data)
        if let dict = parsed as? [String: Any] {
            guard let values = dict["values"] else {
                throw AirshipErrors.error("Missing 'values' key in JSON wrapper")
            }
            return try AirshipJSON.wrap(values).decode()
        }
        return try AirshipJSON.wrap(parsed).decode()
    }

    // TODO Delete this if not needed
    private func optionalParsedArg<T: Decodable>(_ arg: Any?) throws -> T? {
        guard let value = arg else {
            return nil
        }
        return try requireParsedArgWithValues(value)
    }

    private func requireStringArrayArg(_ arg: Any?) throws -> [String] {
        guard let value: [String] = arg as? [String] else {
            throw AirshipErrors.error("Argument must be a string array")
        }
        return value
    }

    private func callUnitySendMessage(objectName: String, methodName: String, message: String) {
        UnitySendMessage(objectName, methodName, message)
    }

    /// Converts a push notification payload to a JSON string.
    ///
    /// - Parameter push: The push notification payload.
    /// - Returns: A JSON string representation of the push.
    private func convertPushToJson(push: [AnyHashable: Any]) -> String {
        let alert = (push["aps"] as? [String: Any])?["alert"] as? String
        let identifier = push["_"] as? String

        var extras = [[String: Any]]()

        for (key, value) in push {
            guard let keyString = key as? String else {
                continue
            }
            
            // Skip "aps" and "_" keys.
            if keyString == "_" || keyString == "aps" {
                continue
            }

            var extraValue = value
            
            if !(extraValue is String) {
                extraValue = convertToJson(extraValue)
            }
            
            extras.append(["key": keyString, "value": extraValue])
        }

        var serializedPayload = [String: Any]()
        serializedPayload["alert"] = alert
        serializedPayload["identifier"] = identifier

        if !extras.isEmpty {
            serializedPayload["extras"] = extras
        }

        return convertToJson(serializedPayload)
    }

    /// Converts an object to a JSON string.
    ///
    /// - Parameter obj: The object to be serialized.
    /// - Returns: A JSON string representation of the object, or "{}" if serialization fails.
    private func convertToJson(_ obj: Any) -> String {
        do {
            let data = try JSONSerialization.data(withJSONObject: obj, options: [])
            if let jsonString = String(data: data, encoding: .utf8) {
                return jsonString
            }
        } catch {
            print("Error converting object to JSON: \(error.localizedDescription)")
        }
        
        return "{}"
    }
}
