/*
 Copyright 2016 Urban Airship and Contributors
*/

package com.urbanairship.unityplugin;

import android.content.Context;
import android.content.Intent;
import android.os.Handler;
import android.os.Looper;

import com.urbanairship.AirshipConfigOptions;
import com.urbanairship.Autopilot;
import com.urbanairship.Logger;
import com.urbanairship.UAirship;
import com.urbanairship.actions.Action;
import com.urbanairship.actions.ActionArguments;
import com.urbanairship.actions.ActionRegistry;
import com.urbanairship.actions.ActionResult;
import com.urbanairship.actions.DeepLinkAction;


public class UnityAutopilot extends Autopilot {

    @Override
    public void onAirshipReady(UAirship airship) {

        ActionRegistry.Entry entry = airship.getActionRegistry().getEntry(DeepLinkAction.DEFAULT_REGISTRY_NAME);
        entry.setDefaultAction(new Action() {
            @Override
            public ActionResult perform(ActionArguments arguments) {
                String deeplink = arguments.getValue().getString();
                if (deeplink != null) {
                    UnityPlugin.shared().setDeepLink(deeplink);
                    UnityPlugin.shared().onDeepLinkReceived(deeplink);
                }

                new Handler(Looper.getMainLooper()).post(new Runnable() {
                    @Override
                    public void run() {
                        Intent launch = UAirship.getPackageManager().getLaunchIntentForPackage(UAirship.getPackageName());
                        launch.addCategory(Intent.CATEGORY_LAUNCHER);
                        launch.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_SINGLE_TOP);
                        UAirship.getApplicationContext().startActivity(launch);
                    }
                });

                return ActionResult.newResult(arguments.getValue());
            }

            @Override
            public boolean acceptsArguments(ActionArguments arguments) {
                return SITUATION_PUSH_OPENED == arguments.getSituation();
            }
        });
    }

    public AirshipConfigOptions createAirshipConfigOptions(Context context) {
        int resourceId = context.getResources().getIdentifier("airship_config", "xml", context.getPackageName());
        if (resourceId <= 0) {
            Logger.error("airship_config.xml not found. Make sure Urban Airship is configured Window => Urban Airship => Settings.");
            return null;
        }

        return new AirshipConfigOptions.Builder()
                .applyConfig(context, resourceId)
                .build();
    }
}
