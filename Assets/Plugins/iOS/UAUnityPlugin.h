/*
 Copyright 2015 Urban Airship and Contributors
*/

#import <Foundation/Foundation.h>
#import "UAPush.h"

extern void UnitySendMessage(const char *, const char *, const char *);

#pragma mark -
#pragma mark Listeners

void UAUnityPlugin_addListener(const char* listener);
void UAUnityPlugin_removeListener(const char* listener);

#pragma mark -
#pragma mark Deep Links

const char* UAUnityPlugin_getDeepLink(bool clear);

#pragma mark -
#pragma mark UA Push Functions
const char* UAUnityPlugin_getIncomingPush(bool clear);
bool UAUnityPlugin_isPushEnabled();

void UAUnityPlugin_enablePush();
void UAUnityPlugin_disablePush();
const char* UAUnityPlugin_getTags();
void UAUnityPlugin_addTag(const char* tag);
void UAUnityPlugin_removeTag(const char* tag);
const char* UAUnityPlugin_getAlias();
void UAUnityPlugin_setAlias(const char* alias);
const char* UAUnityPlugin_getChannelId();

#pragma mark -
#pragma mark UA Location Functions

bool UAUnityPlugin_isLocationEnabled();
void UAUnityPlugin_enableLocation();
void UAUnityPlugin_disableLocation();

bool UAUnityPlugin_isBackgroundLocationEnabled();
void UAUnityPlugin_enableBackgroundLocation();
void UAUnityPlugin_disableBackgroundLocation();

#pragma mark -
#pragma mark Custom Events
void UAUnityPlugin_addCustomEvent(const char *customEvent);

#pragma mark -
#pragma mark Named User
void UAUnityPlugin_setNamedUserID(const char *namedUserID);
const char* UAUnityPlugin_getNamedUserID();


#pragma mark -
#pragma mark Message Center
void UAUnityPlugin_displayMessageCenter();


@interface UAUnityPlugin : NSObject <UAPushNotificationDelegate>

+ (UAUnityPlugin *)shared;

@property (nonatomic,strong) NSMutableSet* listeners;
@property (nonatomic,strong) NSMutableArray* receivePushes;
@property (nonatomic,strong) NSDictionary* storedNotification;
@property (nonatomic,strong) NSString* storedDeepLink;

@end
