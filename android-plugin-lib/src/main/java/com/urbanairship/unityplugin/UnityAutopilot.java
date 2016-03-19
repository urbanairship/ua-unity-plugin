/*
 Copyright 2016 Urban Airship and Contributors
*/

package com.urbanairship.unityplugin;

import android.content.Context;
import android.content.Intent;
import android.content.res.TypedArray;
import android.content.res.XmlResourceParser;
import android.os.Handler;
import android.os.Looper;
import android.util.AttributeSet;
import android.util.Log;
import android.util.Xml;

import com.urbanairship.AirshipConfigOptions;
import com.urbanairship.Autopilot;
import com.urbanairship.Logger;
import com.urbanairship.UAirship;
import com.urbanairship.actions.Action;
import com.urbanairship.actions.ActionArguments;
import com.urbanairship.actions.ActionRegistry;
import com.urbanairship.actions.ActionResult;
import com.urbanairship.actions.DeepLinkAction;
import com.urbanairship.push.notifications.DefaultNotificationFactory;

import org.xmlpull.v1.XmlPullParser;
import org.xmlpull.v1.XmlPullParserException;

import java.io.IOException;


public class UnityAutopilot extends Autopilot {

    private int notificationAccentColor;
    private int notificationIcon;

    @Override
    public void onAirshipReady(UAirship airship) {

        DefaultNotificationFactory factory = new DefaultNotificationFactory(UAirship.getApplicationContext());
        factory.setColor(notificationAccentColor);

        if (notificationIcon > 0) {
            factory.setSmallIconId(notificationIcon);
        }

        airship.getPushManager().setNotificationFactory(factory);

        ActionRegistry.Entry entry = airship.getActionRegistry().getEntry(DeepLinkAction.DEFAULT_REGISTRY_NAME);
        entry.setDefaultAction(new Action() {
            @Override
            public ActionResult perform(ActionArguments arguments) {
                UnityPlugin.shared().setDeepLink(arguments.getValue().getString());


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

        XmlResourceParser parser = context.getResources().getXml(resourceId);
        AttributeSet attributeSet = null;

        int state;
        do {
            try {
                state = parser.next();
            } catch (XmlPullParserException | IOException e) {
                Logger.error("Failed to parse airship_config.xml");
                parser.close();
                return null;
            }

            if (state == XmlPullParser.START_TAG && parser.getName().equals("UrbanAirshipConfig")) {
                attributeSet = Xml.asAttributeSet(parser);
                break;
            }
        } while (state != XmlPullParser.END_DOCUMENT);


        if (attributeSet == null) {
            Logger.error("airship_config.xml does not define UrbanAirshipConfig element");
            parser.close();
            return null;
        }

        TypedArray typedArray = context.obtainStyledAttributes(attributeSet, R.styleable.UrbanAirshipConfig);

        AirshipConfigOptions configOptions = new AirshipConfigOptions.Builder()
                .setProductionAppKey(typedArray.getString(R.styleable.UrbanAirshipConfig_urbanAirshipProductionAppKey))
                .setProductionAppSecret(typedArray.getString(R.styleable.UrbanAirshipConfig_urbanAirshipProductionAppSecret))
                .setProductionLogLevel(typedArray.getInt(R.styleable.UrbanAirshipConfig_urbanAirshipProductionLogLevel, Log.ERROR))
                .setDevelopmentAppKey(typedArray.getString(R.styleable.UrbanAirshipConfig_urbanAirshipDevelopmentAppKey))
                .setDevelopmentAppSecret(typedArray.getString(R.styleable.UrbanAirshipConfig_urbanAirshipDevelopmentAppSecret))
                .setDevelopmentLogLevel(typedArray.getInt(R.styleable.UrbanAirshipConfig_urbanAirshipDevelopmentLogLevel, Log.DEBUG))
                .setInProduction(typedArray.getBoolean(R.styleable.UrbanAirshipConfig_urbanAirshipInProduction, false))
                .setGcmSender(typedArray.getString(R.styleable.UrbanAirshipConfig_urbanAirshipGcmSender))
                .build();

        notificationAccentColor = typedArray.getColor(R.styleable.UrbanAirshipConfig_urbanAirshipNotificationAccentColor, 0);
        notificationIcon = typedArray.getResourceId(R.styleable.UrbanAirshipConfig_urbanAirshipNotificationIcon, 0);

        parser.close();
        typedArray.recycle();

        return configOptions;
    }
}
