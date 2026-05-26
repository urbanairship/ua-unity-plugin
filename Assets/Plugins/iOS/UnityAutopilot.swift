/* Copyright Airship and Contributors */

import Foundation
import AirshipFrameworkProxy

#if canImport(AirshipKit)
import AirshipKit
#elseif canImport(AirshipCore)
import AirshipCore
#endif

@MainActor
final class UnityAutopilot: NSObject, AirshipProxyDelegate {

    static let shared = UnityAutopilot()

    private static let pluginVersionKey = "UAUnityPluginVersion"

    func onLoad() {
        AirshipProxy.shared.delegate = self
        try? AirshipProxy.shared.attemptTakeOff()
    }

    func loadDefaultConfig() -> AirshipConfig {
        return (try? AirshipConfig.default()) ?? AirshipConfig()
    }

    func migrateData(store: ProxyStore) {

    }

    func onAirshipReady() {
        AirshipLogger.info("UnityAutopilot - onAirshipReady")
        let version = Bundle.main.infoDictionary?[Self.pluginVersionKey] as? String ?? "0.0.0"
        Airship.analytics.registerSDKExtension(.unity, version: version)
    }
}
