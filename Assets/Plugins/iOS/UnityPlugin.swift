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

private enum HandleResult {
    case handledSync(Any?)
    case handledAsync(Any?)
    case notHandled
}

@_cdecl("UnityPlugin_call")
public func UnityPlugin_call(_ method: UnsafePointer<CChar>, argsJson: UnsafePointer<CChar>) -> UnsafeMutablePointer<CChar>? {
    let methodStr = String(cString: method)
    let argsJsonStr = String(cString: argsJson)

    let args: [Any]
    do {
        let data = argsJsonStr.data(using: .utf8) ?? Data()
        args = (try JSONSerialization.jsonObject(with: data) as? [Any]) ?? []
    } catch {
        AirshipLogger.error("Failed to deserialize arguments for method \(methodStr): \(error)")
        return strdup("{}")
    }

    let result: Any?

    do {
        // Sync path
        let syncResult = try UnityPlugin.shared.handleCall(method: methodStr, args: args)
        switch syncResult {
        case .handledSync(let value):
            result = value
        case .notHandled:
            // Async path
            let asyncResult = try runAsync { try await UnityPlugin.shared.handleCallAsync(method: methodStr, args: args) }
            switch asyncResult {
            case .handledAsync(let value):
                result = value
            case .notHandled:
                AirshipLogger.error("Unknown method: \(methodStr)")
                return strdup("{}")
            default:
                AirshipLogger.error("Unexpected result type for async handler \(methodStr)")
                return strdup("{}")
            }
        default:
            AirshipLogger.error("Unexpected result type for sync handler \(methodStr)")
            return strdup("{}")
        }
    } catch {
        AirshipLogger.error("Error executing method \(methodStr): \(error)")
        return strdup("{}")
    }

    do {
        let jsonResult = try AirshipJSON.wrap(result).toString()
        return strdup(jsonResult)
    } catch {
        return strdup("{}")
    }
}

private func runAsync<T>(_ block: @escaping () async throws -> T) throws -> T {
    let semaphore = DispatchSemaphore(value: 0)
    var result: T?
    var callError: Error?

    Task {
        do {
            result = try await block()
        } catch {
            callError = error
        }
        semaphore.signal()
    }

    if Thread.isMainThread {
        while semaphore.wait(timeout: .now()) == .timedOut {
            RunLoop.current.run(mode: .default, before: Date(timeIntervalSinceNow: 0.005))
        }
    } else {
        semaphore.wait()
    }

    if let callError { throw callError }
    return result!
}

class UnityPlugin: NSObject {

    static let shared = UnityPlugin()

    public var listener: String? = nil

    private override init() {
        super.init()
        startEventProcessing()
    }

    // private static let initializeOnce: Void = {
    //     // Add Notification Observer
    //     NotificationCenter.default.addObserver(forName: UIApplication.didFinishLaunchingNotification,
    //                                            object: nil,
    //                                            queue: nil) { notification in
    //         // TODO let's see how we handle take off
    //         // UnityPlugin.performTakeOff(withLaunchOptions: notification.userInfo)
    //     }
    // }()

    func startEventProcessing() {
        Task { @MainActor in
            for await _ in AirshipProxyEventEmitter.shared.pendingEventAdded {
                self.notifyPendingEvents()
            }
        }
    }

    func handleCall(method: String, args: [Any]) throws -> HandleResult {
        AirshipLogger.debug("UnityPlugin \(method): \(args)")

        switch method {
            case "setListener":
                listener = try requireStringArg(args.first)
                return .handledSync(nil)

            // Airship
            case "takeOff":
                let value = try MainActor.assumeIsolated {
                    try AirshipProxy.shared.takeOff(json: try requireParsedAnyArg(args.first))
                }
                return .handledSync(value)

            case "isFlying":
                return .handledSync(AirshipProxy.shared.isFlying())

            // Channel
            case "getChannelId":
                return .handledSync(try AirshipProxy.shared.channel.channelID)

            case "addTag":
                try AirshipProxy.shared.channel.addTags([requireStringArg(args.first)])
                return .handledSync(nil)

            case "removeTag":
                try AirshipProxy.shared.channel.removeTags([requireStringArg(args.first)])
                return .handledSync(nil)

            case "getTags":
                return .handledSync(try AirshipProxy.shared.channel.tags)

            case "editTags":
                try AirshipProxy.shared.channel.editTags(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return .handledSync(nil)

            case "editChannelTagGroups":
                try AirshipProxy.shared.channel.editTagGroups(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return .handledSync(nil)

            case "editChannelAttributes":
                try AirshipProxy.shared.channel.editAttributes(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return .handledSync(nil)

            case "editChannelSubscriptionLists":
                let parsed = try requireParsedAnyArg(args.first)
                guard let dict = parsed as? [String: Any], let values = dict["values"] else {
                    throw AirshipErrors.error("Missing 'values' key in JSON wrapper")
                }
                try AirshipProxy.shared.channel.editSubscriptionLists(
                    json: values
                )
                return .handledSync(nil)

            // Contact
            case "identify":
                try AirshipProxy.shared.contact.identify(try requireStringArg(args.first))
                return .handledSync(nil)

            case "reset":
                try AirshipProxy.shared.contact.reset()
                return .handledSync(nil)

            case "notifyRemoteLogin":
                try AirshipProxy.shared.contact.notifyRemoteLogin()
                return .handledSync(nil)

            case "editContactTagGroups":
                try AirshipProxy.shared.contact.editTagGroups(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return .handledSync(nil)

            case "editContactAttributes":
                try AirshipProxy.shared.contact.editAttributes(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return .handledSync(nil)

            case "editContactSubscriptionLists":
                try AirshipProxy.shared.contact.editSubscriptionLists(
                    operations: try requireParsedArgWithValues(args.first)
                )
                return .handledSync(nil)

            // Analytics
            case "associateIdentifier":
                guard args.count == 1 || args.count == 2 else {
                    throw AirshipErrors.error("associateIdentifier call requires 1 to 2 strings parameters.")
                }
                try AirshipProxy.shared.analytics.associateIdentifier(
                    identifier: args.count == 2 ? requireStringArg(args[1]) : nil,
                    key: requireStringArg(args[0])
                )
                return .handledSync(nil)

            case "trackScreen":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.analytics.trackScreen(
                        try? requireStringArg(args.first)
                    )
                }
                return .handledSync(nil)

            case "addCustomEvent":
                try AirshipProxy.shared.analytics.addEvent(
                    try requireParsedAnyArg(args.first)
                )
                return .handledSync(nil)

            case "getSessionId":
                let value = try MainActor.assumeIsolated {
                    try AirshipProxy.shared.analytics.getSessionID()
                }
                return .handledSync(value)

            // InApp
            case "setPaused":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.inApp.setPaused(try requireBoolArg(args.first))
                }
                return .handledSync(nil)

            case "isPaused":
                let value = try MainActor.assumeIsolated {
                    try AirshipProxy.shared.inApp.isPaused()
                }
                return .handledSync(value)

            case "getDisplayInterval":
                let value = try MainActor.assumeIsolated {
                    try AirshipProxy.shared.inApp.getDisplayInterval()
                }
                return .handledSync(value)

            // Locale
            case "setLocaleOverride":
                try AirshipProxy.shared.locale.setCurrentLocale(
                    try requireStringArg(args.first)
                )
                return .handledSync(nil)

            case "clearLocaleOverride":
                try AirshipProxy.shared.locale.clearLocale()
                return .handledSync(nil)

            case "getLocale":
                return .handledSync(try AirshipProxy.shared.locale.currentLocale)

            // Message Center
            case "setAutoLaunchDefaultMessageCenter":
                try MainActor.assumeIsolated {
                    AirshipProxy.shared.messageCenter.setAutoLaunchDefaultMessageCenter(
                        try requireBoolArg(args.first)
                    )
                }
                return .handledSync(nil)

            case "displayMessageCenter":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.messageCenter.display(
                        messageID: try? requireStringArg(args.first)
                    )
                }
                return .handledSync(nil)

            case "dismissMessageCenter":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.messageCenter.dismiss()
                }
                return .handledSync(nil)

            case "showMessageView":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.messageCenter.showMessageView(
                        messageID: try requireStringArg(args.first)
                    )
                }
                return .handledSync(nil)

            case "showMessageCenter":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.messageCenter.showMessageCenter(
                        messageID: try? requireStringArg(args.first)
                    )
                }
                return .handledSync(nil)

            // Preference Center
            case "displayPreferenceCenter":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.preferenceCenter.displayPreferenceCenter(
                        preferenceCenterID: try requireStringArg(args.first)
                    )
                }
                return .handledSync(nil)

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
                return .handledSync(nil)

            // Privacy Manager
            case "setEnabledFeatures":
                try AirshipProxy.shared.privacyManager.setEnabled(
                    featureNames: try requireStringArrayArg(args)
                )
                return .handledSync(nil)

            case "getEnabledFeatures":
                return .handledSync(try AirshipProxy.shared.privacyManager.getEnabledNames())
                
            case "enableFeatures":
                try AirshipProxy.shared.privacyManager.enable(
                    featureNames: try requireStringArrayArg(args)
                )
                return .handledSync(nil)

            case "disableFeatures":
                try AirshipProxy.shared.privacyManager.disable(
                    featureNames: try requireStringArrayArg(args)
                )
                return .handledSync(nil)

            case "isFeaturesEnabled":
                return .handledSync(try AirshipProxy.shared.privacyManager.isEnabled(
                    featuresNames: try requireStringArrayArg(args)
                ))

            // Push
            case "isUserNotificationsEnabled":
                return .handledSync(try AirshipProxy.shared.push.isUserNotificationsEnabled())

            case "setUserNotificationsEnabled":
                try AirshipProxy.shared.push.setUserNotificationsEnabled(
                    try requireBoolArg(args.first)
                )
                return .handledSync(nil)

            case "getPushToken":
                let value = try MainActor.assumeIsolated {
                    try AirshipProxy.shared.push.getRegistrationToken()
                }
                return .handledSync(value)

            case "clearNotifications":
                AirshipProxy.shared.push.clearNotifications()
                return .handledSync(nil)

            case "clearNotification":
                AirshipProxy.shared.push.clearNotification(
                    try requireStringArg(args.first)
                )
                return .handledSync(nil)

            // Push iOS
            case "setForegroundPresentationOptions":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.push.setForegroundPresentationOptions(
                        names: try requireStringArrayArg(args.first)
                    )
                }
                return .handledSync(nil)

            case "setNotificationOptions":
                try MainActor.assumeIsolated {
                    try AirshipProxy.shared.push.setNotificationOptions(
                        names: try requireStringArrayArg(args.first)
                    )
                }
                return .handledSync(nil)

            case "isAutobadgeEnabled":
                return .handledSync(try AirshipProxy.shared.push.isAutobadgeEnabled())

            case "setAutobadgeEnabled":
                try AirshipProxy.shared.push.setAutobadgeEnabled(
                    try requireBoolArg(args.first)
                )
                return .handledSync(nil)

            case "getBadgeNumber":
                let value = try MainActor.assumeIsolated {
                    try AirshipProxy.shared.push.getBadgeNumber()
                }
                return .handledSync(value)

            case "setQuietTimeEnabled":
                try AirshipProxy.shared.push.setQuietTimeEnabled(
                    try requireBoolArg(args.first)
                )
                return .handledSync(nil)

            case "isQuietTimeEnabled":
                return .handledSync(try AirshipProxy.shared.push.isQuietTimeEnabled())

            case "setQuietTime":
                try AirshipProxy.shared.push.setQuietTime(
                    try requireCodableArg(args.first)
                )
                return .handledSync(nil)

            case "getQuietTime":
                return .handledSync(try AirshipProxy.shared.push.getQuietTime())

            case "trackInteraction":
                try AirshipProxy.shared.featureFlagManager.trackInteraction(
                    flag: try AirshipJSON.wrap(requireParsedAnyArg(args.first)).decode()
                )
                return .handledSync(nil)
            
            default:
                return .notHandled
        }
    }

    func handleCallAsync(method: String, args: [Any]) async throws -> HandleResult {
        AirshipLogger.debug("UnityPlugin async \(method): \(args)")

        switch method {
            case "waitForChannelId":
                return .handledAsync(try await AirshipProxy.shared.channel.waitForChannelID())

            case "getChannelSubscriptionLists":
                return .handledAsync(try await AirshipProxy.shared.channel.fetchSubscriptionLists())

            case "getContactSubscriptionLists":
                let subscriptionLists = try await AirshipProxy.shared.contact.getSubscriptionLists()
                let subscriptionListsData = try AirshipJSON.wrap(subscriptionLists).toString().data(using: .utf8) ?? Data()
                let subscriptionListsDict = try JSONSerialization.jsonObject(with: subscriptionListsData) as? [String: [String]] ?? [:]
                var resultArray: [[String: Any]] = []
                for (listId, scopes) in subscriptionListsDict {
                    resultArray.append(["listId": listId, "scopes": scopes])
                }
                return .handledAsync(resultArray)
            
            case "getNamedUserId":
                return .handledAsync(try await AirshipProxy.shared.contact.namedUserID)

            case "setDisplayInterval":
                try await AirshipProxy.shared.inApp.setDisplayInterval(
                    milliseconds: try requireIntArg(args.first)
                )
                return .handledAsync(nil)
            
            case "getUnreadCount":
                return .handledAsync(try await AirshipProxy.shared.messageCenter.unreadCount)

            case "getMessages":
                return .handledAsync(try await AirshipProxy.shared.messageCenter.messages)

            case "markMessageRead":
                try await AirshipProxy.shared.messageCenter.markMessageRead(
                    messageID: requireStringArg(args.first)
                )
                return .handledAsync(nil)

            case "deleteMessage":
                try await AirshipProxy.shared.messageCenter.deleteMessage(
                    messageID: requireStringArg(args.first)
                )
                return .handledAsync(nil)

            case "refreshMessages":
                try await AirshipProxy.shared.messageCenter.refresh()
                return .handledAsync(nil)

            case "getPreferenceCenterConfig":
                return .handledAsync(try await AirshipProxy.shared.preferenceCenter.getPreferenceCenterConfig(
                    preferenceCenterID: try requireStringArg(args.first)
                ))

            case "enableUserNotifications":
                return .handledAsync(try await AirshipProxy.shared.push.enableUserPushNotifications(
                    args: try optionalCodableArg(args.first)
                ))

            case "getNotificationStatus":
                return .handledAsync(try await AirshipProxy.shared.push.notificationStatus)

            case "getActiveNotifications":
                return .handledAsync(try await AirshipProxy.shared.push.getActiveNotifications())

            case "setBadgeNumber":
                try await AirshipProxy.shared.push.setBadgeNumber(
                    try requireIntArg(args.first)
                )
                return .handledAsync(nil)

            case "runAction":
                return .handledAsync(try await AirshipProxy.shared.action.runAction(
                    try requireStringArg(args.first),
                    value: try? AirshipJSON.wrap(try requireStringArg(args[1]))
                ))

            case "flag":
                let flag = try await AirshipProxy.shared.featureFlagManager.flag(
                    name: try requireStringArg(args.first)
                )
                let flagJsonData = try AirshipJSON.wrap(flag).toString().data(using: .utf8) ?? Data()
                let flagDict = try JSONSerialization.jsonObject(with: flagJsonData) as? [String: Any] ?? [:]
                
                var result: [String: Any] = [:]
                result["isEligible"] = flagDict["isEligible"] as? Bool ?? false
                result["exists"] = flagDict["exists"] as? Bool ?? false
            
                if let internalObj = flagDict["_internal"],
                   let internalData = try? JSONSerialization.data(withJSONObject: internalObj) {
                    result["_internal"] = String(data: internalData, encoding: .utf8) ?? ""
                }
            
                if let variablesObj = flagDict["variables"],
                   let variablesData = try? JSONSerialization.data(withJSONObject: variablesObj) {
                    result["variables"] = String(data: variablesData, encoding: .utf8) ?? ""
                }
            
                return .handledAsync(result)

            default:
                return .notHandled
        }
    }

    @MainActor
    private func notifyPendingEvents() {
        for eventType in AirshipProxyEventType.allCases {
            AirshipProxyEventEmitter.shared.processPendingEvents(type: eventType) { event in
                if let data = try? JSONEncoder().encode(event.body),
                   let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any] {
                    switch event.type {
                    case .channelCreated:
                        if let channelId = json["channelId"] as? String {
                            channelCreated(channelId)
                        }
                    case .deepLinkReceived:
                        if let deepLink = json["deepLink"] as? String {
                            receivedDeepLink(deepLink)
                        }
                    case .pushReceived:
                        if let pushPayloadRaw = json["pushPayload"],
                           let pushPayload: ProxyPushPayload = try? AirshipJSON.wrap(pushPayloadRaw).decode(),
                           let isForeground = json["isForeground"] as? Bool {
                            receivedNotification(pushPayload, isForeground: isForeground)
                        }
                    case .notificationResponseReceived:
                        if let pushPayloadRaw = json["pushPayload"],
                           let pushPayload: ProxyPushPayload = try? AirshipJSON.wrap(pushPayloadRaw).decode(),
                           let isForeground = json["isForeground"] as? Bool,
                           let actionId = json["actionId"] as? String? {
                            receivedNotificationResponse(pushPayload, isForeground: isForeground, actionId: actionId)
                        }
                    case .messageCenterUpdated:
                        if let messageCount = json["messageCount"] as? Int,
                           let messageUnreadCount = json["messageUnreadCount"] as? Int {
                            inboxUpdated(messageCount: messageCount, messageUnreadCount: messageUnreadCount)
                        }
                    case .displayMessageCenter:
                        if let messageId = json["messageId"] as? String? {
                            messageCenterDisplayed(messageId)
                        }
                    case .displayPreferenceCenter:
                        if let preferenceCenterId = json["preferenceCenterId"] as? String? {
                            preferenceCenterDisplayed(preferenceCenterId)
                        }
                    case .authorizedNotificationSettingsChanged:
                        if let authorizedSettings = json["authorizedSettings"] as? [String] {
                            authorizedNotificationSettingsChanged(authorizedSettings)
                        }
                    case .pushTokenReceived:
                        if let pushToken = json["pushToken"] as? String {
                            pushTokenReceived(pushToken)
                        }
                    case .notificationStatusChanged:
                        if let statusRaw = json["status"],
                           let status: NotificationStatus = try? AirshipJSON.wrap(statusRaw).decode() {
                            notificationStatusChanged(status)
                        }
                    case .pendingEmbeddedUpdated, .liveActivitiesUpdated:
                        break
                    }
                }
                return true
            }
        }
    }

    // Push Notification Delegates
    
    public func receivedNotification(_ pushPayload: ProxyPushPayload, isForeground: Bool) {
        AirshipLogger.debug("UnityPlugin receivedNotification \(pushPayload)")
        
        do {
            let jsonPush = try AirshipJSON.wrap(pushPayload).toString()
            
            if let listener = self.listener {
                callUnitySendMessage(objectName: listener,
                                     methodName: "OnPushReceived",
                                     message: jsonPush
                )
            }
        } catch {
            AirshipLogger.debug("UnityPlugin failed to serialize push")
        }
    }
    
    public func receivedNotificationResponse(_ pushPayload: ProxyPushPayload, isForeground: Bool, actionId: String?) {
        AirshipLogger.debug("UnityPlugin receivedNotificationResponse \(pushPayload)")
        
        do {
            let jsonPush = try AirshipJSON.wrap(pushPayload).toString()
            
            if let listener = self.listener {
                callUnitySendMessage(objectName: listener,
                                     methodName: "OnPushOpened",
                                     message: jsonPush
                )
            }
        } catch {
            AirshipLogger.debug("UnityPlugin failed to serialize push")
        }
    }

    // Airship DeepLink Delegate
    public func receivedDeepLink(_ deepLink: String) {
        AirshipLogger.debug("UnityPlugin receivedDeepLink \(deepLink)")

        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnDeepLinkReceived",
                                 message: deepLink
            )
        }
    }

    // Channel Creation Event
    public func channelCreated(_ channelId: String) {
        AirshipLogger.debug("UnityPlugin channelCreated: \(channelId)")
        
        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnChannelCreated",
                                 message: channelId
            )
        }
    }

    // Inbox Message List Updated Notification
    public func inboxUpdated(messageCount: Int, messageUnreadCount: Int) {
        let counts : [String: Any] = [
            "unread": messageUnreadCount,
            "total": messageCount
        ]

        AirshipLogger.debug("UnityPlugin inboxUpdated(unread = \(messageUnreadCount), total = \(messageCount))")

        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnInboxUpdated",
                                 message: convertToJson(counts)
            )
        }
    }

    // Message Center Display Delegate
    
    public func messageCenterDisplayed(_ messageId: String? = nil) {
        AirshipLogger.debug("UnityPlugin messageCenterDisplayed \(String(describing: messageId))")
        
        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnShowInbox",
                                 message: messageId ?? ""
            )
        }
    }
    
    // Preference Center Display Delegate
    public func preferenceCenterDisplayed(_ preferenceCenterId: String? = nil) {
        AirshipLogger.debug("UnityPlugin preferenceCenterDisplayed \(String(describing: preferenceCenterId))")
        
        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnPreferenceCenterDisplay",
                                 message: preferenceCenterId ?? ""
            )
        }
    }
    
    public func authorizedNotificationSettingsChanged(_ authorizedSettings: [String]) {
        AirshipLogger.debug("UnityPlugin authorizedNotificationSettingsChanged \(String(describing: authorizedSettings))")
        
        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnAuthorizedNotificationSettingsChanged",
                                 message: convertToJson(authorizedSettings)
            )
        }
    }

    public func pushTokenReceived(_ pushToken: String) {
        AirshipLogger.debug("UnityPlugin pushTokenReceived \(pushToken)")
        
        if let listener = self.listener {
            callUnitySendMessage(objectName: listener,
                                 methodName: "OnPushTokenReceived",
                                 message: pushToken
            )
        }
    }
    
    public func notificationStatusChanged(_ status: NotificationStatus) {
        AirshipLogger.debug("UnityPlugin notificationStatusChanged \(status)")
        
        do {
            let jsonStatus = try AirshipJSON.wrap(status).toString()
        
            if let listener = self.listener {
                callUnitySendMessage(objectName: listener,
                                     methodName: "OnNotificationStatusChanged",
                                     message: jsonStatus
                )
            }
        } catch {
            AirshipLogger.debug("UnityPlugin failed to serialize notification status")
        }
    }
    
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
