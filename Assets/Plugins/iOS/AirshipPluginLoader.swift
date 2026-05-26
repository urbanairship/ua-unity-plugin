/* Copyright Airship and Contributors */

import Foundation
import AirshipFrameworkProxy

@objc(AirshipPluginLoader)
@MainActor
public class AirshipPluginLoader: NSObject, AirshipPluginLoaderProtocol {
    
    public static func onLoad() {
        UnityAutopilot.shared.onLoad()
    }
}
