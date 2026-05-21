/* Copyright Airship and Contributors */
package com.urbanairship.unityplugin

import android.content.Context
import com.urbanairship.Airship
import com.urbanairship.AirshipConfigOptions
import com.urbanairship.analytics.Extension
import com.urbanairship.UALog
import com.urbanairship.android.framework.proxy.BaseAutopilot
import com.urbanairship.android.framework.proxy.ProxyStore

class UnityAutopilot : BaseAutopilot() {
    override fun onMigrateData(context: Context, proxyStore: ProxyStore) {

    }

    override fun onReady(context: Context) {
        UALog.i { "UnityAutopilot - onAirshipReady" }
        Airship.analytics.registerSDKExtension(Extension.UNITY, BuildConfig.PLUGIN_VERSION)
    }

    override fun createConfigBuilder(context: Context): AirshipConfigOptions.Builder {
        val resourceId = context.resources.getIdentifier("airship_config", "xml", context.packageName)
        if (resourceId <= 0) {
            UALog.e { "airship_config.xml not found. Make sure you call TakeOff() or you configured Airship in the Unity Editor Window => Urban Airship => Settings." }
            return super.createConfigBuilder(context)
        }

        val builder = AirshipConfigOptions.newBuilder()
            .applyConfig(context, resourceId)

        return builder
    }
}