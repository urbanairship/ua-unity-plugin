/*
 Copyright 2015 Urban Airship and Contributors
*/

package com.urbanairship.unityplugin;

import android.app.Application;
import android.content.Intent;

import com.urbanairship.UAirship;
import com.urbanairship.actions.Action;
import com.urbanairship.actions.ActionArguments;
import com.urbanairship.actions.ActionRegistry;
import com.urbanairship.actions.ActionResult;
import com.urbanairship.actions.DeepLinkAction;
import com.urbanairship.actions.Situation;

public class UrbanAirshipApplication extends Application {
    public void onCreate() {
        super.onCreate();

        UAirship.takeOff(this, new UAirship.OnReadyCallback() {
            @Override
            public void onAirshipReady(UAirship airship) {
                ActionRegistry.Entry entry = ActionRegistry.shared().getEntry(DeepLinkAction.DEFAULT_REGISTRY_NAME);
                entry.setDefaultAction(new Action() {
                    @Override
                    public ActionResult perform(String actionName, ActionArguments arguments) {
                        UnityPlugin.shared().setDeepLink(String.valueOf(arguments.getValue()));

                        Intent launch = getPackageManager().getLaunchIntentForPackage(UAirship.getPackageName());
                        launch.addCategory(Intent.CATEGORY_LAUNCHER);
                        launch.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_SINGLE_TOP);
                        startActivity(launch);

                        return ActionResult.newEmptyResult();
                    }

                    @Override
                    public boolean acceptsArguments(ActionArguments arguments) {
                        return Situation.PUSH_OPENED == arguments.getSituation();
                    }
                });
            }
        });
    }
}
