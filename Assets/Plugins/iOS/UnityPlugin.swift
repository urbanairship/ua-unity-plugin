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

enum HandleResult {
    case handledSync(Any?)
    case handledAsync(Any?)
    case notHandled
}

func makeSuccessResponse(_ jsonResult: String) -> UnsafeMutablePointer<CChar>? {
    if let data = try? JSONSerialization.data(withJSONObject: ["result": jsonResult]),
       let str = String(data: data, encoding: .utf8) {
        return strdup(str)
    }
    return strdup("{\"error\":\"Failed to create response envelope\"}")
}

func makeErrorResponse(_ message: String) -> UnsafeMutablePointer<CChar>? {
    if let data = try? JSONSerialization.data(withJSONObject: ["error": message]),
       let str = String(data: data, encoding: .utf8) {
        return strdup(str)
    }
    return strdup("{\"error\":\"Unknown error\"}")
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
        return makeErrorResponse("Failed to deserialize arguments for method \(methodStr): \(error)")
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
                return makeErrorResponse("Unknown method: \(methodStr)")
            default:
                return makeErrorResponse("Unexpected result type for async handler \(methodStr)")
            }
        default:
            return makeErrorResponse("Unexpected result type for sync handler \(methodStr)")
        }
    } catch {
        return makeErrorResponse("Error executing method \(methodStr): \(error)")
    }

    do {
        let jsonResult = try AirshipJSON.wrap(result).toString()
        return makeSuccessResponse(jsonResult)
    } catch {
        return makeErrorResponse("Failed to serialize result for \(methodStr): \(error)")
    }
}

// Kept slightly below the C# AirshipCoroutineHelper timeout (60s) so this native
// timeout fires first and surfaces a specific error, avoiding a race between layers.
private let asyncTimeout: TimeInterval = 59.0

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

    let deadline = Date(timeIntervalSinceNow: asyncTimeout)

    if Thread.isMainThread {
        while semaphore.wait(timeout: .now()) == .timedOut {
            if Date() > deadline {
                throw AirshipErrors.error("Async call timed out after \(Int(asyncTimeout))s")
            }
            RunLoop.current.run(mode: .default, before: Date(timeIntervalSinceNow: 0.005))
        }
    } else {
        if semaphore.wait(timeout: .now() + asyncTimeout) == .timedOut {
            throw AirshipErrors.error("Async call timed out after \(Int(asyncTimeout))s")
        }
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
                    try AirshipProxy.shared.takeOff(json: try requireAnyArg(args.first))
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
                    operations: try requireJsonStringArgWithValues(args.first)
                )
                return .handledSync(nil)

            case "editChannelTagGroups":
                try AirshipProxy.shared.channel.editTagGroups(
                    operations: try requireJsonStringArgWithValues(args.first)
                )
                return .handledSync(nil)

            case "editChannelAttributes":
                try AirshipProxy.shared.channel.editAttributes(
                    operations: try requireJsonStringArgWithValues(args.first)
                )
                return .handledSync(nil)

            case "editChannelSubscriptionLists":
                let parsed = try requireJsonStringArg(args.first)
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
                    operations: try requireJsonStringArgWithValues(args.first)
                )
                return .handledSync(nil)

            case "editContactAttributes":
                try AirshipProxy.shared.contact.editAttributes(
                    operations: try requireJsonStringArgWithValues(args.first)
                )
                return .handledSync(nil)

            case "editContactSubscriptionLists":
                try AirshipProxy.shared.contact.editSubscriptionLists(
                    operations: try requireJsonStringArgWithValues(args.first)
                )
                return .handledSync(nil)

            // Analytics
            case "associateIdentifier":
                guard args.count == 1 || args.count == 2 else {
                    throw AirshipErrors.error("associateIdentifier call requires 1 to 2 strings parameters.")
                }
                try AirshipProxy.shared.analytics.associateIdentifier(
                    // An explicit null identifier clears the identifier.
                    identifier: args.count == 2 ? (try? requireStringArg(args[1])) : nil,
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
                    try requireJsonStringArg(args.first)
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

            case "getAuthorizedNotificationSettings":
                return .handledSync(try AirshipProxy.shared.push.getAuthorizedNotificationSettings())

            case "getAuthorizedNotificationStatus":
                return .handledSync(try AirshipProxy.shared.push.getAuthroizedNotificationStatus())

            case "trackInteraction":
                try AirshipProxy.shared.featureFlagManager.trackInteraction(
                    flag: try AirshipJSON.wrap(requireJsonStringArg(args.first)).decode()
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
                // parse the value string into structured JSON when possible,
                // falling back to the raw string value.
                let actionName = try requireStringArg(args.first)
                let rawActionValue: Any? = args.count > 1 ? args[1] : nil
                let actionValue: AirshipJSON?
                if let stringValue = rawActionValue as? String,
                   let data = stringValue.data(using: .utf8),
                   let parsed = try? JSONSerialization.jsonObject(with: data, options: [.fragmentsAllowed]) {
                    actionValue = try? AirshipJSON.wrap(parsed)
                } else if let rawActionValue {
                    actionValue = try? AirshipJSON.wrap(rawActionValue)
                } else {
                    actionValue = nil
                }
                return .handledAsync(try await AirshipProxy.shared.action.runAction(
                    actionName,
                    value: actionValue
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

            // Live Activity (iOS only)
            case "liveActivityList":
                if #available(iOS 16.1, *) {
                    let request: LiveActivityRequest.List = try requireCodableArg(args.first)
                    let activities = try await LiveActivityManager.shared.list(request)
                    return .handledAsync(try activities.map { try AirshipJSON.wrap($0).unWrap() })
                } else {
                    throw AirshipErrors.error("Live Activities require iOS 16.1+")
                }

            case "liveActivityListAll":
                if #available(iOS 16.1, *) {
                    let activities = try await LiveActivityManager.shared.listAll()
                    return .handledAsync(try activities.map { try AirshipJSON.wrap($0).unWrap() })
                } else {
                    throw AirshipErrors.error("Live Activities require iOS 16.1+")
                }

            case "liveActivityStart":
                if #available(iOS 16.1, *) {
                    let request: LiveActivityRequest.Start = try requireCodableArg(args.first)
                    let activity = try await LiveActivityManager.shared.start(request)
                    return .handledAsync(try AirshipJSON.wrap(activity).unWrap())
                } else {
                    throw AirshipErrors.error("Live Activities require iOS 16.1+")
                }

            case "liveActivityUpdate":
                if #available(iOS 16.1, *) {
                    let request: LiveActivityRequest.Update = try requireCodableArg(args.first)
                    try await LiveActivityManager.shared.update(request)
                    return .handledAsync(nil)
                } else {
                    throw AirshipErrors.error("Live Activities require iOS 16.1+")
                }

            case "liveActivityEnd":
                if #available(iOS 16.1, *) {
                    let request: LiveActivityRequest.End = try requireCodableArg(args.first)
                    try await LiveActivityManager.shared.end(request)
                    return .handledAsync(nil)
                } else {
                    throw AirshipErrors.error("Live Activities require iOS 16.1+")
                }

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
                           let pushPayload: ProxyPushPayload = try? AirshipJSON.wrap(pushPayloadRaw).decode() {
                            let isForeground = json["isForeground"] as? Bool ?? false
                            receivedNotification(pushPayload, isForeground: isForeground)
                        }
                    case .notificationResponseReceived:
                        if let pushPayloadRaw = json["pushPayload"],
                           let pushPayload: ProxyPushPayload = try? AirshipJSON.wrap(pushPayloadRaw).decode() {
                            let isForeground = json["isForeground"] as? Bool ?? false
                            let actionId = json["actionId"] as? String
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
                    case .pendingEmbeddedUpdated, .liveActivitiesUpdated, .overridePresentationOptions:
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
                                 methodName: "OnAuthorizedSettingsChanged",
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

    private func requireStringArrayArg(_ arg: Any?) throws -> [String] {
        guard let value: [String] = arg as? [String] else {
            throw AirshipErrors.error("Argument must be a string array")
        }
        return value
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
        return try AirshipJSON.wrap(value).decode()
    }

    private func requireJsonStringArg(_ arg: Any?) throws -> Any {
        guard let value = arg else {
            throw AirshipErrors.error("Missing argument")
        }
        if let jsonString = value as? String,
           let data = jsonString.data(using: .utf8) {
            return try JSONSerialization.jsonObject(with: data)
        }
        return value
    }

    private func requireJsonStringArgWithValues<T: Decodable>(_ arg: Any?) throws -> T {
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

    private func callUnitySendMessage(objectName: String, methodName: String, message: String) {
        UnitySendMessage(objectName, methodName, message)
    }
}
