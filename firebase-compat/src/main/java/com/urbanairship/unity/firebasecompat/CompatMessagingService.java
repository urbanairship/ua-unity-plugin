/* Copyright 2018 Urban Airship and Contributors */
package com.urbanairship.unity.firebasecompat;

import com.google.firebase.messaging.RemoteMessage;
import com.urbanairship.push.fcm.AirshipFirebaseMessagingService;

public class CompatMessagingService extends com.google.firebase.messaging.cpp.ListenerService {
    @Override
    public void onMessageReceived(RemoteMessage message) {
        super.onMessageReceived(message);
        AirshipFirebaseMessagingService.processMessageSync(getApplicationContext(), message);
    }
}
