/*
 Copyright 2015 Urban Airship and Contributors
*/

#import "UAUnityPlugin.h"
#import "UAPush.h"
#import "UAirship.h"
#import "NSJSONSerialization+UAAdditions.h"
#import "UAActionArguments.h"
#import "UAActionRunner.h"
#import "UAActionResult.h"
#import "UALocationService.h"
#import "UAConfig.h"

static UAUnityPlugin *shared_;
static dispatch_once_t onceToken_;

@implementation UAUnityPlugin

+ (void)load {
    NSLog(@"UnityPlugin class loaded");
    NSNotificationCenter *center = [NSNotificationCenter defaultCenter];
    [center addObserver:[UAUnityPlugin class] selector:@selector(performTakeOff:) name:UIApplicationDidFinishLaunchingNotification object:nil];
}

+ (void)performTakeOff:(NSNotification *)notification {
    NSLog(@"UnityPlugin taking off");
    [UAirship takeOff];

    //UAPush delegate and UAActionRegistry need to be set at load so that cold start launches get deeplinks
    [UAirship push].pushNotificationDelegate = [UAUnityPlugin shared];

    UAAction *customAction = [UAAction actionWithBlock: ^(UAActionArguments *args, UAActionCompletionHandler handler)  {
        NSLog(@"Setting dl to: %@", args.value);
        [UAUnityPlugin shared].storedDeepLink = args.value;
        handler([UAActionResult emptyResult]);
    } acceptingArguments:^BOOL(UAActionArguments *arg)  {
        if (arg.situation == UASituationBackgroundPush) {
            return NO;
        }

        return [arg.value isKindOfClass:[NSString class]];
    }];

    [[UAirship shared].actionRegistry updateAction:customAction forEntryWithName:kUADeepLinkActionDefaultRegistryName];
}

+ (UAUnityPlugin *)shared {
    dispatch_once(&onceToken_, ^{
        shared_ = [[UAUnityPlugin alloc] init];
    });

    return shared_;
}

-(id) init {
    self = [super init];
    if (self) {
        self.listeners = [NSMutableSet setWithCapacity:10];
        self.receivePushes = [NSMutableArray array];
    }
    return self;
}


#pragma mark -
#pragma mark Listeners

void UAUnityPlugin_addListener(const char* listener) {
    
    NSString *listenerObj = [NSString stringWithUTF8String:listener];
    NSLog(@"UAUnityPlugin_addListener %@",listenerObj);
    [[UAUnityPlugin shared].listeners addObject:listenerObj];
    
    if (![UAUnityPlugin shared].receivePushes.count) {
        for (NSDictionary* notification in [UAUnityPlugin shared].receivePushes) {
            [[UAUnityPlugin shared] notifyRecievedPush:notification];
        }
        
        [[UAUnityPlugin shared].receivePushes removeAllObjects];
    }
}

void UAUnityPlugin_removeListener(const char* listener) {
    NSString *listenerObj = [NSString stringWithUTF8String:listener];
    NSLog(@"UAUnityPlugin_removeListener %@",listenerObj);
    [[UAUnityPlugin shared].listeners removeObject:listenerObj];
}

#pragma mark -
#pragma mark Deep Links

const char* UAUnityPlugin_getDeepLink(bool clear) {
    NSLog(@"UnityPlugin getDeepLink clear %d",clear);
    
    const char* dl = [UAUnityPlugin convertToJson:[UAUnityPlugin shared].storedDeepLink];
    if (clear) {
        [UAUnityPlugin shared].storedDeepLink = nil;
    }
    return dl;
}



#pragma mark -
#pragma mark UA Push Functions
const char* UAUnityPlugin_getIncomingPush(bool clear) {
    NSLog(@"UnityPlugin getIncomingPush clear %d",clear);
    
    const char* push = [UAUnityPlugin convertToJson:[UAUnityPlugin shared].storedNotification];
    if (clear) {
        [UAUnityPlugin shared].storedNotification = nil;
    }
    return push;
}

bool UAUnityPlugin_isPushEnabled() {
    NSLog(@"UnityPlugin isPushEnabled");
    return [UAirship push].userPushNotificationsEnabled ? true : false;
}

void UAUnityPlugin_enablePush() {
    NSLog(@"UnityPlugin enablePush");
    [UAirship push].userPushNotificationsEnabled = YES;
}

void UAUnityPlugin_disablePush() {
    NSLog(@"UnityPlugin disablePush");
    [UAirship push].userPushNotificationsEnabled = NO;
}

const char* UAUnityPlugin_getTags() {
    NSLog(@"UnityPlugin getTags");
    return [UAUnityPlugin convertToJson:[UAirship push].tags];
}

void UAUnityPlugin_addTag(const char* tag) {
    NSString *tagString = [NSString stringWithUTF8String:tag];
    
    NSLog(@"UnityPlugin addTag %@", tagString);
    [[UAirship push] addTag:tagString];
    [[UAirship push] updateRegistration];
}

void UAUnityPlugin_removeTag(const char* tag) {
    NSString *tagString = [NSString stringWithUTF8String:tag];
    
    NSLog(@"UnityPlugin removeTag %@", tagString);
    [[UAirship push] removeTag:tagString];
    [[UAirship push] updateRegistration];
}

const char* UAUnityPlugin_getAlias() {
    NSLog(@"UnityPlugin getAlias");
    return MakeStringCopy([[UAirship push].alias UTF8String]);
}

void UAUnityPlugin_setAlias(const char* alias) {
    NSString *aliasString = [NSString stringWithUTF8String:alias];
    
    NSLog(@"UnityPlugin setAlias %@", aliasString);
    [UAirship push].alias = aliasString;
    [[UAirship push] updateRegistration];
}

const char* UAUnityPlugin_getChannelId() {
    NSLog(@"UnityPlugin getChannelId");    
    return MakeStringCopy([[UAirship push].channelID UTF8String]);
}

#pragma mark -
#pragma mark UA Location Functions

bool UAUnityPlugin_isLocationEnabled() {
    NSLog(@"UnityPlugin isLocationEnabled");
    return [UALocationService airshipLocationServiceEnabled] ? true : false;
}

void UAUnityPlugin_enableLocation() {
    NSLog(@"UnityPlugin enableLocation");
    [UALocationService setAirshipLocationServiceEnabled:YES];
    [[UAirship shared].locationService startReportingSignificantLocationChanges];
}

void UAUnityPlugin_disableLocation() {
    NSLog(@"UnityPlugin disableLocation");
    [UALocationService setAirshipLocationServiceEnabled:NO];
    [[UAirship shared].locationService stopReportingSignificantLocationChanges];
}

bool UAUnityPlugin_isBackgroundLocationEnabled() {
    NSLog(@"UnityPlugin isBackgroundLocationEnabled");
    return [UAirship shared].locationService.backgroundLocationServiceEnabled ? true : false;
}

void UAUnityPlugin_enableBackgroundLocation() {
    NSLog(@"UnityPlugin enableBackgroundLocation");
    [UAirship shared].locationService.backgroundLocationServiceEnabled = YES;
}

void UAUnityPlugin_disableBackgroundLocation() {
    NSLog(@"UnityPlugin disableBackgroundLocation");
    [UAirship shared].locationService.backgroundLocationServiceEnabled = NO;
}

#pragma mark -
#pragma mark Actions!

#pragma mark -
#pragma mark UAPushNotificationDelegate
/**
 * Called when a push notification is received while the app is running in the foreground.
 *
 * @param notification The notification dictionary.
 */
- (void)receivedForegroundNotification:(NSDictionary *)notification {
    NSLog(@"receivedForegroundNotification %@",notification);
    if (self.listeners.count) {
        [self notifyRecievedPush:notification];
    } else {
        [self.receivePushes addObject:notification];
    }
}

/**
 * Called when the app is started or resumed because a user opened a notification.
 *
 * @param notification The notification dictionary.
 */
- (void)launchedFromNotification:(NSDictionary *)notification {
    NSLog(@"launchedFromNotification %@",notification);
    self.storedNotification = notification;
}

- (void) notifyRecievedPush:(NSDictionary *)notification {
    NSString *alertMessage = [(NSDictionary*)[notification objectForKey:@"aps"] objectForKey:@"alert"];
    
    for (NSString* listener in self.listeners) {
        NSLog(@"UnityPlugin notifying %@ push received",listener);
        UnitySendMessage(MakeStringCopy([listener UTF8String]),
                         "OnPushReceived",
                         MakeStringCopy([alertMessage UTF8String]));
    }
}


#pragma mark -
#pragma mark Helper C Functions

+ (const char *) convertToJson:(NSObject*) obj {
    NSString *JSONString = [NSJSONSerialization stringWithObject:obj acceptingFragments:YES];
    return MakeStringCopy([JSONString UTF8String]);
}

// Helper method to create C string copy
char* MakeStringCopy (const char* string) {
    if (string == NULL) {
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

@end
