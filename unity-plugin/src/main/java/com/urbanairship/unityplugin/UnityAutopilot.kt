/* Copyright Airship and Contributors */
package com.urbanairship.unityplugin

import android.content.Context
import com.urbanairship.AirshipConfigOptions
import com.urbanairship.UAirship
import com.urbanairship.analytics.Extension
import com.urbanairship.android.framework.proxy.BaseAutopilot
import com.urbanairship.android.framework.proxy.ProxyLogger
import com.urbanairship.android.framework.proxy.ProxyStore

class UnityAutopilot : BaseAutopilot() {
    override fun onMigrateData(context: Context, proxyStore: ProxyStore) {

    }

    override fun onReady(context: Context, airship: UAirship) {
        airship.analytics.registerSDKExtension(Extension.UNITY, BuildConfig.PLUGIN_VERSION)
    }

    override fun createConfigBuilder(context: Context): AirshipConfigOptions.Builder {
        val resourceId = context.resources.getIdentifier("airship_config", "xml", context.packageName)
        if (resourceId <= 0) {
            ProxyLogger.error("airship_config.xml not found. Make sure Urban Airship is configured Window => Urban Airship => Settings.")
            return super.createConfigBuilder(context)
        }

        val builder = AirshipConfigOptions.newBuilder()
            .applyConfig(context, resourceId)

        //ProxyLogger.setLogLevel(options.logLevel)

        return builder
    }
}