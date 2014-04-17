//
//  UAUnityPlugin.h
//  unity-plugin
//
//  Created by Neel Banerjee on 4/16/14.
//  Copyright (c) 2014 Neel Banerjee. All rights reserved.
//

#import <Foundation/Foundation.h>

void UAUnityPlugin_takeOff();
void UAUnityPlugin_enablePush();
void UAUnityPlugin_disablePush();
const char* UAUnityPlugin_getTags();
void UAUnityPlugin_addTag(const char* tag);
void UAUnityPlugin_removeTag(const char* tag);
const char* UAUnityPlugin_getAlias();
void UAUnityPlugin_setAlias(const char* alias);
const char* UAUnityPlugin_getPushIDs();

@interface UAUnityPlugin : NSObject
+ (UAUnityPlugin*)sharedInstance;

@end
