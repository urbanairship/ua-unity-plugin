/*
 Copyright 2016 Urban Airship and Contributors
 */

#import "UAUnityPlugin.h"
#import "UnityInterface.h"
#import "UAPush.h"
#import "UAirship.h"
#import "NSJSONSerialization+UAAdditions.h"
#import "UAAction+Operators.h"
#import "UAActionArguments.h"
#import "UAActionRunner.h"
#import "UAActionResult.h"
#import "UALocation.h"
#import "UAConfig.h"
#import "UAAnalytics.h"
#import "UACustomEvent.h"
#import "UAUtils.h"
#import "UADefaultMessageCenter.h"
#import "UAInbox.h"
#import "UAInboxMessageList.h"
#import "UAAssociatedIdentifiers.h"

static UAUnityPlugin *shared_;
static dispatch_once_t onceToken_;

@implementation UAUnityPlugin

+ (void)load {
    UA_LDEBUG(@"UnityPlugin class loaded");
    NSNotificationCenter *center = [NSNotificationCenter defaultCenter];
    [center addObserver:[UAUnityPlugin class] selector:@selector(performTakeOff:) name:UIApplicationDidFinishLaunchingNotification object:nil];
}

+ (void)performTakeOff:(NSNotification *)notification {
    UA_LDEBUG(@"UnityPlugin taking off");
    [UAirship takeOff];

    // UAPush delegate and UAActionRegistry need to be set at load so that cold start launches get deeplinks
    [UAirship push].pushNotificationDelegate = [UAUnityPlugin shared];
    [UAirship push].registrationDelegate = [UAUnityPlugin shared];

    // Check if the config specified default foreground presentation options
    UAConfig *airshipConfig = [UAirship shared].config;
    NSDictionary *customOptions = [airshipConfig customConfig];

    if (customOptions) {
        UNNotificationPresentationOptions options = UNNotificationPresentationOptionNone;

        if ([customOptions[@"notificationPresentationOptionAlert"] boolValue]) {
            options = options | UNNotificationPresentationOptionAlert;
        }
        if ([customOptions[@"notificationPresentationOptionBadge"] boolValue]) {
            options = options | UNNotificationPresentationOptionBadge;
        }
        if ([customOptions[@"notificationPresentationOptionSound"] boolValue]) {
            options = options | UNNotificationPresentationOptionSound;
        }

        UA_LDEBUG(@"Foreground presentation options from the config: %lu", (unsigned long)options);

        [UAirship push].defaultPresentationOptions = options;
    }

    UAAction *customDLA = [UAAction actionWithBlock: ^(UAActionArguments *args, UAActionCompletionHandler handler)  {
        UA_LDEBUG(@"Setting dl to: %@", args.value);
        [UAUnityPlugin shared].storedDeepLink = args.value;

        id listener = [UAUnityPlugin shared].listener;
        if (listener) {
            UnitySendMessage(MakeStringCopy([listener UTF8String]),
                             "OnDeepLinkReceived",
                             MakeStringCopy([args.value UTF8String]));
        }

        handler([UAActionResult emptyResult]);
    } acceptingArguments:^BOOL(UAActionArguments *arg)  {
        if (arg.situation == UASituationBackgroundPush) {
            return NO;
        }

        return [arg.value isKindOfClass:[NSString class]];
    }];

    // Replace the display inbox and landing page actions with modified versions that pause the game before display
    UAAction *dia = [[UAirship shared].actionRegistry registryEntryWithName:kUADisplayInboxActionDefaultRegistryName].action;
    UAAction *customDIA = [dia preExecution:^(UAActionArguments *args) {
        // This will ultimately trigger the OnApplicationPause event
        UnityWillPause();
    }];

    UAAction *lpa = [[UAirship shared].actionRegistry registryEntryWithName:kUALandingPageActionDefaultRegistryName].action;
    UAAction *customLPA = [lpa preExecution:^(UAActionArguments *args) {
        // This will ultimately trigger the OnApplicationPause event
        UnityWillPause();
    }];


    [[UAirship shared].actionRegistry updateAction:customDLA forEntryWithName:kUADeepLinkActionDefaultRegistryName];
    [[UAirship shared].actionRegistry updateAction:customDIA forEntryWithName:kUADisplayInboxActionDefaultRegistryName];
    [[UAirship shared].actionRegistry updateAction:customLPA forEntryWithName:kUALandingPageActionDefaultRegistryName];
}

+ (UAUnityPlugin *)shared {
    dispatch_once(&onceToken_, ^{
        shared_ = [[UAUnityPlugin alloc] init];
    });

    return shared_;
}

- (id)init {
    self = [super init];
    return self;
}


#pragma mark -
#pragma mark Listeners

void UAUnityPlugin_setListener(const char* listener) {
    [UAUnityPlugin shared].listener = [NSString stringWithUTF8String:listener];
    UA_LDEBUG(@"UAUnityPlugin_setListener %@",[UAUnityPlugin shared].listener);
}

#pragma mark -
#pragma mark Deep Links

const char* UAUnityPlugin_getDeepLink(bool clear) {
    UA_LDEBUG(@"UnityPlugin getDeepLink clear %d",clear);

    const char* dl = [UAUnityPlugin convertToJson:[UAUnityPlugin shared].storedDeepLink];
    if (clear) {
        [UAUnityPlugin shared].storedDeepLink = nil;
    }
    return dl;
}

#pragma mark -
#pragma mark UA Push Functions
const char* UAUnityPlugin_getIncomingPush(bool clear) {
    UA_LDEBUG(@"UnityPlugin getIncomingPush clear %d",clear);

    if (![UAUnityPlugin shared].storedNotification) {
        return nil;
    }

    const char* payload = [UAUnityPlugin convertPushToJson:[UAUnityPlugin shared].storedNotification];

    if (clear) {
        [UAUnityPlugin shared].storedNotification = nil;
    }

    return payload;
}

bool UAUnityPlugin_getUserNotificationsEnabled() {
    UA_LDEBUG(@"UnityPlugin getUserNotificationsEnabled");
    return [UAirship push].userPushNotificationsEnabled ? true : false;
}

void UAUnityPlugin_setUserNotificationsEnabled(bool enabled) {
    UA_LDEBUG(@"UnityPlugin setUserNotificationsEnabled: %d", enabled);
    [UAirship push].userPushNotificationsEnabled = enabled ? YES : NO;
}

const char* UAUnityPlugin_getTags() {
    UA_LDEBUG(@"UnityPlugin getTags");
    return [UAUnityPlugin convertToJson:[UAirship push].tags];
}

void UAUnityPlugin_addTag(const char* tag) {
    NSString *tagString = [NSString stringWithUTF8String:tag];

    UA_LDEBUG(@"UnityPlugin addTag %@", tagString);
    [[UAirship push] addTag:tagString];
    [[UAirship push] updateRegistration];
}

void UAUnityPlugin_removeTag(const char* tag) {
    NSString *tagString = [NSString stringWithUTF8String:tag];

    UA_LDEBUG(@"UnityPlugin removeTag %@", tagString);
    [[UAirship push] removeTag:tagString];
    [[UAirship push] updateRegistration];
}

const char* UAUnityPlugin_getAlias() {
    UA_LDEBUG(@"UnityPlugin getAlias");
    return MakeStringCopy([[UAirship push].alias UTF8String]);
}

void UAUnityPlugin_setAlias(const char* alias) {
    NSString *aliasString = [NSString stringWithUTF8String:alias];

    UA_LDEBUG(@"UnityPlugin setAlias %@", aliasString);
    [UAirship push].alias = aliasString;
    [[UAirship push] updateRegistration];
}

const char* UAUnityPlugin_getChannelId() {
    UA_LDEBUG(@"UnityPlugin getChannelId");
    return MakeStringCopy([[UAirship push].channelID UTF8String]);
}

#pragma mark -
#pragma mark UA Location Functions

bool UAUnityPlugin_isLocationEnabled() {
    UA_LDEBUG(@"UnityPlugin isLocationEnabled");
    return [UAirship location].locationUpdatesEnabled ? true : false;
}

void UAUnityPlugin_setLocationEnabled(bool enabled) {
    UA_LDEBUG(@"UnityPlugin setLocationEnabled: %d", enabled);
    [UAirship location].locationUpdatesEnabled = enabled;
}

bool UAUnityPlugin_isBackgroundLocationAllowed() {
    UA_LDEBUG(@"UnityPlugin isBackgroundLocationAllowed");
    return [UAirship location].backgroundLocationUpdatesAllowed ? true : false;
}

void UAUnityPlugin_setBackgroundLocationAllowed(bool enabled) {
    UA_LDEBUG(@"UnityPlugin setBackgroundLocationAllowed: %d", enabled);
    [UAirship location].backgroundLocationUpdatesAllowed = enabled ? YES : NO;
}

void UAUnityPlugin_addCustomEvent(const char *customEvent) {
    NSString *customEventString = [NSString stringWithUTF8String:customEvent];
    UA_LDEBUG(@"UnityPlugin addCustomEvent");
    id obj = [NSJSONSerialization objectWithString:customEventString];

    UACustomEvent *ce = [UACustomEvent eventWithName:[UAUnityPlugin stringOrNil:obj[@"eventName"]]];

    NSString *valueString = [UAUnityPlugin stringOrNil:obj[@"eventValue"]];
    if (valueString) {
        ce.eventValue = [NSDecimalNumber decimalNumberWithString:valueString];
    }

    ce.interactionID = [UAUnityPlugin stringOrNil:obj[@"interactionId"]];
    ce.interactionType = [UAUnityPlugin stringOrNil:obj[@"interactionType"]];
    ce.transactionID = [UAUnityPlugin stringOrNil:obj[@"transactionID"]];

    for (id property in obj[@"properties"]) {
        NSString *name = [UAUnityPlugin stringOrNil:property[@"name"]];
        id value;
        NSString *type = property[@"type"];
        if ([type isEqualToString:@"s"]) {
            value = property[@"stringValue"];
            [ce setStringProperty:value forKey:name];
        } else if ([type isEqualToString:@"d"]) {
            value = property[@"doubleValue"];
            [ce setNumberProperty:value forKey:name];
        } else if ([type isEqualToString:@"b"]) {
            value = property[@"boolValue"];
            [ce setBoolProperty:value forKey:name];
        } else if ([type isEqualToString:@"sa"]) {
            value = property[@"stringArrayValue"];
            [ce setStringArrayProperty:value forKey:name];
        }
    }

    [[UAirship shared].analytics addEvent:ce];
}

void UAUnityPlugin_associateIdentifier(const char *key, const char *identifier) {
    if (!key) {
        UA_LDEBUG(@"UnityPlugin associateIdentifier failed, key cannot be nil");
        return;
    }

    NSString *keyString = [UAUnityPlugin stringOrNil:[NSString stringWithUTF8String:key]];
    NSString *identifierString = nil;

    if (!identifier) {
        UA_LDEBUG(@"UnityPlugin associateIdentifier removed identifier for key: %@", keyString);
    } else {
        identifierString = [UAUnityPlugin stringOrNil:[NSString stringWithUTF8String:identifier]];
        UA_LDEBUG(@"UnityPlugin associateIdentifier with identifier: %@ for key: %@", identifierString, keyString);
    }

    UAAssociatedIdentifiers *identifiers = [[UAirship shared].analytics currentAssociatedDeviceIdentifiers];
    [identifiers setIdentifier:identifierString forKey:keyString];
    [[UAirship shared].analytics associateDeviceIdentifiers:identifiers];
}

void UAUnityPlugin_setNamedUserID(const char *namedUserID) {
    NSString *namedUserIDString = [NSString stringWithUTF8String:namedUserID];
    UA_LDEBUG(@"UnityPlugin setNamedUserID %@", namedUserIDString);
    [UAirship namedUser].identifier = namedUserIDString;
}

const char* UAUnityPlugin_getNamedUserID() {
    return MakeStringCopy([[UAirship namedUser].identifier UTF8String]);
}


#pragma mark -
#pragma mark MessageCenter

void UAUnityPlugin_displayMessageCenter() {
    UA_LDEBUG(@"UnityPlugin displayMessageCenter");
    UnityWillPause();
    [[UAirship defaultMessageCenter] display];
}

int UAUnityPlugin_getMessageCenterUnreadCount() {
    UA_LDEBUG(@"UnityPlugin getMessageCenterUnreadCount");
    return [UAirship inbox].messageList.unreadCount;
}

#pragma mark -
#pragma mark Tag Groups

void UAUnityPlugin_editChannelTagGroups(const char *payload) {
    UA_LDEBUG(@"UnityPlugin editChannelTagGroups");
    id payloadMap = [NSJSONSerialization objectWithString:[NSString stringWithUTF8String:payload]];
    id operations = payloadMap[@"values"];

    for (NSDictionary *operation in operations) {
        NSString *group = operation[@"tagGroup"];
        if ([operation[@"operation"] isEqualToString:@"add"]) {
            [[UAirship push] addTags:operation[@"tags"] group:group];
        } else if ([operation[@"operation"] isEqualToString:@"remove"]) {
            [[UAirship push] removeTags:operation[@"tags"] group:group];
        }
    }

    [[UAirship push] updateRegistration];
}

void UAUnityPlugin_editNamedUserTagGroups(const char *payload) {
    UA_LDEBUG(@"UnityPlugin editNamedUserTagGroups");
    id payloadMap = [NSJSONSerialization objectWithString:[NSString stringWithUTF8String:payload]];
    id operations = payloadMap[@"values"];

    for (NSDictionary *operation in operations) {
        NSString *group = operation[@"tagGroup"];
        if ([operation[@"operation"] isEqualToString:@"add"]) {
            [[UAirship namedUser] addTags:operation[@"tags"] group:group];
        } else if ([operation[@"operation"] isEqualToString:@"remove"]) {
            [[UAirship namedUser] removeTags:operation[@"tags"] group:group];
        }
    }

    [[UAirship namedUser] updateTags];
}


#pragma mark -
#pragma mark Actions!

#pragma mark -
#pragma mark UAPushNotificationDelegate
/**
 * Called when a push notification is received while the app is running in the foreground.
 *
 * @param notificationContent The UANotificationContent object representing the notification info.
 */
- (void)receivedForegroundNotification:(UANotificationContent *)notificationContent completionHandler:(void(^)())completionHandler {
    UA_LDEBUG(@"receivedForegroundNotification %@",notificationContent);

    if (self.listener) {
        UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
                     "OnPushReceived",
                     [UAUnityPlugin convertPushToJson:notificationContent.notificationInfo]);
        completionHandler();
    }
}


/**
 * Called when the app is started or resumed because a user opened a notification.
 *
 * @param notificationResponse UANotificationResponse object representing the user's response
 */
- (void)receivedNotificationResponse:(UANotificationResponse *)notificationResponse completionHandler:(void(^)())completionHandler {
    UA_LDEBUG(@"receivedNotificationResponse %@",notificationResponse);
    self.storedNotification = notificationResponse.notificationContent.notificationInfo;
    completionHandler();
}

#pragma mark -
#pragma mark UARegistrationDelegate


/**
 * Called when the device channel registers with Urban Airship. Successful
 * registrations could be disabling push, enabling push, or updating the device
 * registration settings.
 *
 * The device token will only be available once the application successfully
 * registers with APNS.
 *
 * When registration finishes in the background, any async tasks that are triggered
 * from this call should request a background task.
 * @param channelID The channel ID string.
 * @param deviceToken The device token string.
 */
- (void)registrationSucceededForChannelID:(NSString *)channelID deviceToken:(NSString *)deviceToken {
    UA_LDEBUG(@"registrationSucceededForChannelID: %@", channelID);
    if (self.listener) {
        UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
                         "OnChannelUpdated",
                         MakeStringCopy([channelID UTF8String]));
    }
}

#pragma mark -
#pragma mark Helpers

+ (NSString *)stringOrNil:(NSString *)string {
    return string.length > 0 ? string : nil;
}

+ (const char *) convertPushToJson:(NSDictionary *)push {
    NSString *alert = push[@"aps"][@"alert"];
    NSString *identifier = push[@"_"];
    NSMutableDictionary *extras = [NSMutableDictionary dictionary];
    for (NSString *key in push) {
        if (![key isEqualToString:@"_"] && ! [key isEqualToString:@"aps"]) {
            id value = push[key];
            if ([value isKindOfClass:[NSString class]]) {
                [extras setValue:value forKey:key];
            } else {
                [extras setValue:[NSJSONSerialization stringWithObject:value] forKey:key];
            }
        }
    }

    NSMutableDictionary *serializedPayload = [NSMutableDictionary dictionary];
    [serializedPayload setValue:alert forKey:@"alert"];
    [serializedPayload setValue:identifier forKey:@"identifier"];

    if (extras.count) {
        [serializedPayload setValue:extras forKey:@"extras"];
    }

    return [UAUnityPlugin convertToJson:serializedPayload];
}

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
