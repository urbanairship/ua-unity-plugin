/*
 Copyright 2016 Urban Airship and Contributors
*/

package com.urbanairship.unityplugin;

import android.app.Application;

import com.urbanairship.Autopilot;

public class UrbanAirshipApplication extends Application {

    @Override
    public void onCreate() {
        super.onCreate();
        Autopilot.automaticTakeOff(this);
    }

}
