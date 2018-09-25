/* Copyright 2018 Urban Airship and Contributors */
package com.urbanairship.unity.firebasecompat;

import com.urbanairship.push.fcm.AirshipFirebaseInstanceIdService;

public class CompatInstanceIdService extends com.google.firebase.messaging.cpp.FcmInstanceIDListenerService {
    @Override
    public void onTokenRefresh() {
        super.onTokenRefresh();
        AirshipFirebaseInstanceIdService.processTokenRefresh(getApplicationContext());
    }
}
