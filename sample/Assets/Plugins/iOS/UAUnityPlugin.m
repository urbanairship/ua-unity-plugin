//
//  UAUnityPlugin.m
//  unity-plugin
//
//  Created by Neel Banerjee on 4/16/14.
//  Copyright (c) 2014 Neel Banerjee. All rights reserved.
//

#import "UAUnityPlugin.h"
#import "UAPush.h"
#import "UAirship.h"
#import "UAConfig.h"
#import "NSJSONSerialization+UAAdditions.h"
#import "UAActionArguments.h"
#import "UAActionRunner.h"
#import "UAActionResult.h"
#import "UALocationService.h"

//TODO: where to put takeoff? config files?
//TODO: do we need to call registerDevice?

@implementation UAUnityPlugin

#pragma mark -
#pragma mark ObjC methods
+ (UAUnityPlugin*)sharedInstance
{
    static dispatch_once_t pred = 0;
    __strong static id _sharedObject = nil;
    dispatch_once(&pred, ^{
        _sharedObject = [[self alloc] init];
    });
    
    return _sharedObject;
}
+ (void)load {
    NSNotificationCenter *center = [NSNotificationCenter defaultCenter];
    [center addObserver:[UAUnityPlugin class] selector:@selector(performTakeOff:) name:UIApplicationDidFinishLaunchingNotification object:nil];
}

+ (void)performTakeOff:(NSNotification *)notification {
    NSLog(@"TAKING OFFFFFF!!!!!!");
    
    [UAirship takeOff:[UAConfig configWithContentsOfFile:[[NSBundle mainBundle] pathForResource:@"AirshipConfig" ofType:@"txt"]]];
    [[UAirship class] performSelector:NSSelectorFromString(@"handleAppDidFinishLaunchingNotification:") withObject:notification];
    //[UAirship handleAppDidFinishLaunchingNotification:notification];//currently internal-only -- use perform selector?
    [UAPush shared].pushNotificationDelegate = [UAUnityPlugin sharedInstance];
}

#pragma mark -
#pragma mark UAPushNotificationDelegate
- (void)displayNotificationAlert:(NSString *)alertMessage {
    
}

/**
 * Called when an alert notification is received in the foreground with additional localization info.
 * @param alertDict a dictionary containing the alert and localization info
 */
- (void)displayLocalizedNotificationAlert:(NSDictionary *)alertDict {
    
}

/**
 * Called when a push notification is received in the foreground with a sound associated
 * @param soundFilename The sound file to play or `default` for the standard notification sound.
 *        This file must be included in the application bundle.
 */
- (void)playNotificationSound:(NSString *)soundFilename {
    
}


/**
 * Called when a push notification is received in the foreground with a badge number.
 * @param badgeNumber The badge number to display
 */
- (void)handleBadgeUpdate:(NSInteger)badgeNumber {
    
}

/**
 * Called when a push notification is received while the app is running in the foreground.
 *
 * @param notification The notification dictionary.
 */
- (void)receivedForegroundNotification:(NSDictionary *)notification {
    
}


/**
 * Called when a push notification is received while the app is running in the foreground
 * for applications with the "remote-notification" background mode.
 *
 * @param notification The notification dictionary.
 * @param completionHandler Should be called with a UIBackgroundFetchResult as soon as possible, so the system can accurately estimate its power and data cost.
 */
- (void)receivedForegroundNotification:(NSDictionary *)notification fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))completionHandler {
    
}

/**
 * Called when a push notification is received while the app is running in the background
 * for applications with the "remote-notification" background mode.
 *
 * @param notification The notification dictionary.
 * @param completionHandler Should be called with a UIBackgroundFetchResult as soon as possible, so the system can accurately estimate its power and data cost.
 */
- (void)receivedBackgroundNotification:(NSDictionary *)notification fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))completionHandler {
    
}

/**
 * Called when a push notification is received while the app is running in the background.
 *
 * @param notification The notification dictionary.
 */
- (void)receivedBackgroundNotification:(NSDictionary *)notification {
    
}


/**
 * Called when the app is started or resumed because a user opened a notification.
 *
 * @param notification The notification dictionary.
 */
- (void)launchedFromNotification:(NSDictionary *)notification {
    
}


/**
 * Called when the app is started or resumed because a user opened a notification
 * for applications with the "remote-notification" background mode.
 *
 * @param notification The notification dictionary.
 * @param completionHandler Should be called with a UIBackgroundFetchResult as soon as possible, so the system can accurately estimate its power and data cost.
 */
- (void)launchedFromNotification:(NSDictionary *)notification fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))completionHandler {
    
}



#pragma mark -
#pragma mark UA Push Functions

void UAUnityPlugin_takeOff() {
    [UAirship takeOff];
}

bool UAUnityPlugin_isPushEnabled() {
    return [UAPush shared].pushEnabled ? true : false;
}

void UAUnityPlugin_enablePush() {
    [[UAPush shared] setPushEnabled:YES];
    NSLog(@"DEBUG: enablePush");
}




void UAUnityPlugin_disablePush() {
    [[UAPush shared] setPushEnabled:NO];
    NSLog(@"DEBUG: disablePush");
}

const char* UAUnityPlugin_getTags() {
    NSLog(@"DEBUG: getTags");
    return [[UAUnityPlugin sharedInstance] convertToJson:[UAPush shared].tags];
}

void UAUnityPlugin_addTag(const char* tag) {
    NSLog(@"DEBUG: addtags");
    NSString *_tag = [NSString stringWithUTF8String:tag];
    [[UAPush shared] addTagToCurrentDevice:_tag];
}

void UAUnityPlugin_removeTag(const char* tag) {
    NSLog(@"DEBUG: removetag");
    NSString *_tag = [NSString stringWithUTF8String:tag];
    [[UAPush shared] removeTagFromCurrentDevice:_tag];
}

const char* UAUnityPlugin_getAlias() {
    NSLog(@"DEBUG: getalias");
    return [[UAUnityPlugin sharedInstance] convertToJson:[UAPush shared].alias];
}

void UAUnityPlugin_setAlias(const char* alias) {
    NSLog(@"DEBUG: setalias");
    NSString *_alias = [NSString stringWithUTF8String:alias];
    [UAPush shared].alias = _alias;
}

void UAUnityPlugin_launchdefaultLandingPage() {
    UAActionArguments * args = [UAActionArguments argumentsWithValue:@"http://wwww.urbanairship.com"
                                                       withSituation:UASituationManualInvocation];
    
    // Optional completion handler
    UAActionCompletionHandler completionHandler = ^(UAActionResult *result) {
        UA_LDEBUG("Action finished!");
    };
    
    [UAActionRunner runActionWithName:@"landing_page_action" withArguments:args withCompletionHandler:completionHandler];
}

/* Return JSON dictionary of pushids
 {
 "device_token":"6A007D7A25E4223D4CFE956E78A42E09C9BD5026FF761979B7524EAB077B36AB",
 "channel":"9f86fac5-81e9-4aee-8d4e-7549c3a940ea"
 
 }
 */
const char* UAUnityPlugin_getPushIDs() {
    NSLog(@"DEBUG: getpushIDs");
    NSString* deviceToken = [UAPush shared].deviceToken;
    NSString* channelID = [UAPush shared].channelID;
    
    if (!channelID) {
        channelID = @"";
    }
    
    if (!deviceToken) {
        deviceToken = @"";
    }
    NSDictionary *dict = [NSDictionary dictionaryWithObjectsAndKeys:deviceToken,@"deviceToken",channelID,@"channelID",nil];
    return [[UAUnityPlugin sharedInstance] convertToJson:dict];
}

#pragma mark -
#pragma mark UA Location Functions

bool UAUnityPlugin_isLocationEnabled() {
    return [UALocationService locationServicesEnabled] ? true : false;
}

void UAUnityPlugin_enableLocation() {
    [[[UAirship shared] locationService] startReportingSignificantLocationChanges];
}

void UAUnityPlugin_disableLocation() {
    [[[UAirship shared] locationService] stopReportingSignificantLocationChanges];
}

bool UAUnityPlugin_isForegroundLocationEnabled() {
    //TODO: is this the correct one?
    return UAUnityPlugin_isLocationEnabled();
}

void UAUnityPlugin_enableForegroundLocation() {
    //TODO: what typpe of location event do we want?
    UAUnityPlugin_enableLocation();
    
}

void UAUnityPlugin_disableForegroundLocation() {
    UAUnityPlugin_disableLocation();
}

bool UAUnityPlugin_isBackgroundLocationEnabled() {
    return [[UAirship shared] locationService].backgroundLocationServiceEnabled ? true : false;
}

void UAUnityPlugin_enableBackgroundLocation() {
    [[UAirship shared] locationService].backgroundLocationServiceEnabled = YES;
}

void UAUnityPlugin_disableBackgroundLocation() {
    [[UAirship shared] locationService].backgroundLocationServiceEnabled = NO;
}

#pragma mark -
#pragma mark Actions!



#pragma mark -
#pragma mark Helper C Functions

- (const char *) convertToJson:(NSObject*) obj {
    
    NSString *JSONString = [NSJSONSerialization stringWithObject:obj acceptingFragments:YES];
    NSLog(@"JSON OUTPUT: %@",JSONString);
    
    return MakeStringCopy([JSONString UTF8String]);
    
}

// Helper method to create C string copy
char* MakeStringCopy (const char* string)
{
	if (string == NULL)
		return NULL;
	
	char* res = (char*)malloc(strlen(string) + 1);
	strcpy(res, string);
	return res;
}


@end
