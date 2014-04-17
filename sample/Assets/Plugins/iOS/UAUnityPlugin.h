//
//  UAUnityPlugin.h
//  unity-plugin
//
//  Created by Neel Banerjee on 4/16/14.
//  Copyright (c) 2014 Neel Banerjee. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "UAPush.h"

extern void UnitySendMessage(const char *, const char *, const char *);

#pragma mark -
#pragma mark UA Push Functions
void UAUnityPlugin_takeOff();
bool UAUnityPlugin_isPushEnabled();

void UAUnityPlugin_enablePush();
void UAUnityPlugin_disablePush();
const char* UAUnityPlugin_getTags();
void UAUnityPlugin_addTag(const char* tag);
void UAUnityPlugin_removeTag(const char* tag);
const char* UAUnityPlugin_getAlias();
void UAUnityPlugin_setAlias(const char* alias);
const char* UAUnityPlugin_getPushIDs();

#pragma mark -
#pragma mark UA Location Functions

bool UAUnityPlugin_isLocationEnabled();
void UAUnityPlugin_enableLocation();
void UAUnityPlugin_disableLocation();
bool UAUnityPlugin_isForegroundLocationEnabled();
void UAUnityPlugin_enableForegroundLocation();
void UAUnityPlugin_disableForegroundLocation();

bool UAUnityPlugin_isBackgroundLocationEnabled();
void UAUnityPlugin_enableBackgroundLocation();
void UAUnityPlugin_disableBackgroundLocation();


@interface UAUnityPlugin : NSObject <UAPushNotificationDelegate>
+ (UAUnityPlugin*)sharedInstance;
@end
