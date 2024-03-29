/* Copyright Airship and Contributors */

#import "UAUnityPlugin.h"
#import "UnityInterface.h"
#import "UAUnityMessageViewController.h"

static UAUnityPlugin *shared_;
static dispatch_once_t onceToken_;

NSString *const UAUnityAutoLaunchMessageCenterKey = @"com.urbanairship.auto_launch_message_center";
NSString *const UADisplayInboxActionDefaultRegistryName = @"display_inbox_action";
NSString *const UAUnityPluginVersionKey = @"UAUnityPluginVersion";

@interface UAUnityPlugin()
@property (nonatomic, strong) UAUnityMessageViewController *messageViewController;
@end

@implementation UAUnityPlugin

+ (void)load {
    UA_LDEBUG(@"UnityPlugin class loaded");
    [[NSNotificationCenter defaultCenter] addObserverForName:UIApplicationDidFinishLaunchingNotification
                                                          object:nil
                                                           queue:nil usingBlock:^(NSNotification * _Nonnull note) {
        [UAUnityPlugin performTakeOffWithLaunchOptions:note.userInfo];
    }];
}
+ (void)performTakeOffWithLaunchOptions:(NSDictionary *)launchOptions {
    UA_LDEBUG(@"UnityPlugin taking off");
    [UAirship takeOffWithLaunchOptions: launchOptions];

    NSString *version = [NSBundle mainBundle].infoDictionary[UAUnityPluginVersionKey] ?: @"0.0.0";
    [[UAirship analytics] registerSDKExtension:UASDKExtensionUnity version:version];

    // UAPush delegate and UAActionRegistry need to be set at load so that cold start launches get deeplinks
    [UAirship push].pushNotificationDelegate = [UAUnityPlugin shared];
    [UAirship shared].deepLinkDelegate = [UAUnityPlugin shared];

    // Check if the config specified default foreground presentation options
    NSDictionary *customOptions = [UAirship shared].config.customConfig;

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

    // Add observer for inbox updated event
    [[NSNotificationCenter defaultCenter] addObserver:[self shared]
                                             selector:@selector(inboxUpdated)
                                                 name:UAInboxMessageListUpdatedNotification
                                               object:nil];

    [[NSNotificationCenter defaultCenter] addObserver:[self shared]
                                             selector:@selector(channelUpdated:)
                                                 name:UAChannel.channelUpdatedEvent
                                               object:nil];

    [UAMessageCenter shared].displayDelegate = [self shared];
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

// getter and setter for auto-launch message center flag
- (BOOL)autoLaunchMessageCenter {
    if ([[NSUserDefaults standardUserDefaults] objectForKey:UAUnityAutoLaunchMessageCenterKey] == nil) {
        [[NSUserDefaults standardUserDefaults] setBool:YES forKey:UAUnityAutoLaunchMessageCenterKey];
        return YES;
    }

    return [[NSUserDefaults standardUserDefaults] boolForKey:UAUnityAutoLaunchMessageCenterKey];
}

- (void)setAutoLaunchMessageCenter:(BOOL)autoLaunchMessageCenter {
    [[NSUserDefaults standardUserDefaults] setBool:autoLaunchMessageCenter forKey:UAUnityAutoLaunchMessageCenterKey];
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
    return [UAUnityPlugin convertToJson:[UAirship channel].tags];
}

void UAUnityPlugin_addTag(const char* tag) {
    NSString *tagString = [NSString stringWithUTF8String:tag];

    UA_LDEBUG(@"UnityPlugin addTag %@", tagString);
    [[UAirship channel] addTag:tagString];
    [[UAirship push] updateRegistration];
}

void UAUnityPlugin_removeTag(const char* tag) {
    NSString *tagString = [NSString stringWithUTF8String:tag];

    UA_LDEBUG(@"UnityPlugin removeTag %@", tagString);
    [[UAirship channel] removeTag:tagString];
    [[UAirship push] updateRegistration];
}

const char* UAUnityPlugin_getChannelId() {
    UA_LDEBUG(@"UnityPlugin getChannelId");
    return MakeStringCopy([[UAirship channel].identifier UTF8String]);
}

double UAUnityPlugin_getInAppAutomationDisplayInterval() {
    UA_LDEBUG(@"UnityPlugin getInAppAutomationDisplayInterval");
    return [UAInAppAutomation shared].inAppMessageManager.displayInterval;
}

void UAUnityPlugin_setInAppAutomationDisplayInterval(double value) {
    UA_LDEBUG(@"UnityPlugin setBackgroundLocationAllowed %f", value);
    [UAInAppAutomation shared].inAppMessageManager.displayInterval = value;
}

bool UAUnityPlugin_isInAppAutomationPaused() {
    UA_LDEBUG(@"UnityPlugin isInAppAutomationPaused");
    return [UAInAppAutomation shared].paused;
}

void UAUnityPlugin_setInAppAutomationPaused(bool paused) {
    UA_LDEBUG(@"UnityPlugin setInAppAutomationPaused: %d", paused);
    [UAInAppAutomation shared].paused = paused;
}

#pragma mark -
#pragma mark Analytics

void UAUnityPlugin_addCustomEvent(const char *customEvent) {
    NSString *customEventString = [NSString stringWithUTF8String:customEvent];
    UA_LDEBUG(@"UnityPlugin addCustomEvent");
    id obj = [UAJSONUtils objectWithString:customEventString];

    UACustomEvent *ce = [UACustomEvent eventWithName:[UAUnityPlugin stringOrNil:obj[@"eventName"]]];

    NSString *valueString = [UAUnityPlugin stringOrNil:obj[@"eventValue"]];
    if (valueString) {
        ce.eventValue = [NSDecimalNumber decimalNumberWithString:valueString];
    }

    ce.interactionID = [UAUnityPlugin stringOrNil:obj[@"interactionId"]];
    ce.interactionType = [UAUnityPlugin stringOrNil:obj[@"interactionType"]];
    ce.transactionID = [UAUnityPlugin stringOrNil:obj[@"transactionID"]];

    NSMutableDictionary *properties = [NSMutableDictionary dictionary];

    for (id property in obj[@"properties"]) {
        NSString *name = [UAUnityPlugin stringOrNil:property[@"name"]];
        id value;

        NSString *type = property[@"type"];
        if ([type isEqualToString:@"s"]) {
            value = property[@"stringValue"];
        } else if ([type isEqualToString:@"d"]) {
            value = property[@"doubleValue"];
        } else if ([type isEqualToString:@"b"]) {
            value = property[@"boolValue"];
        } else if ([type isEqualToString:@"sa"]) {
            value = property[@"stringArrayValue"];
        }

        [properties setValue:value forKey:name];
    }

    ce.properties = properties.copy;

    [[UAAnalytics shared] addEvent:ce];
}

void UAUnityPlugin_trackScreen(const char *screenName) {
    NSString *screenNameString = [NSString stringWithUTF8String:screenName];
    UA_LDEBUG(@"UnityPlugin trackScreen: %@", screenNameString);

    [[UAAnalytics shared] trackScreen:screenNameString];
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

    UAAssociatedIdentifiers *identifiers = [UAirship.analytics currentAssociatedDeviceIdentifiers];
    [identifiers setIdentifier:identifierString forKey:keyString];
    [UAirship.analytics associateDeviceIdentifiers:identifiers];
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
    [[UAMessageCenter shared] display];
}

void UAUnityPlugin_displayInboxMessage(const char *messageID) {
    NSString *messageIDString = [NSString stringWithUTF8String:messageID];
    UA_LDEBUG(@"UnityPlugin displayInboxMessage %@", messageIDString);
    UnityWillPause();
    [[UAUnityPlugin shared] displayInboxMessage:messageIDString];
}

void UAUnityPlugin_refreshInbox() {
    UA_LDEBUG(@"UnityPlugin refreshInbox");
    UnityWillPause();
    [[UAMessageCenter shared].messageList retrieveMessageListWithSuccessBlock:^(){} withFailureBlock:^(){}];
}

const char* UAUnityPlugin_getInboxMessages() {
    UA_LDEBUG(@"UnityPlugin getInboxMessages");
    return [UAUnityPlugin convertInboxMessagesToJson:[UAMessageCenter shared].messageList.messages];
}

void UAUnityPlugin_markInboxMessageRead(const char *messageID) {
    NSString *messageIDString = [NSString stringWithUTF8String:messageID];
    UA_LDEBUG(@"UnityPlugin markInboxMessageRead %@", messageIDString);
    UAInboxMessage *message = [[UAMessageCenter shared].messageList messageForID:messageIDString];
    [[UAMessageCenter shared].messageList markMessagesRead:@[message] completionHandler:nil];
}

void UAUnityPlugin_deleteInboxMessage(const char *messageID) {
    NSString *messageIDString = [NSString stringWithUTF8String:messageID];
    UA_LDEBUG(@"UnityPlugin deleteInboxMessage %@", messageIDString);
    UAInboxMessage *message = [[UAMessageCenter shared].messageList messageForID:messageIDString];
    [[UAMessageCenter shared].messageList markMessagesDeleted:@[message] completionHandler:nil];
}

void UAUnityPlugin_setAutoLaunchDefaultMessageCenter(bool enabled) {
    UA_LDEBUG(@"UnityPlugin UAUnityPlugin_setAutoLaunchDefaultMessageCenter %@", enabled ? @"YES" : @"NO");
    [UAUnityPlugin shared].autoLaunchMessageCenter = enabled;
}

int UAUnityPlugin_getMessageCenterUnreadCount() {
    int unreadCount = (int)[UAMessageCenter shared].messageList.unreadCount;
    UA_LDEBUG(@"UnityPlugin getMessageCenterUnreadCount: %d", unreadCount);
    return unreadCount;
}

int UAUnityPlugin_getMessageCenterCount() {
    int messageCount = (int)[UAMessageCenter shared].messageList.messageCount;
    UA_LDEBUG(@"UnityPlugin getMessageCenterCount: %d", messageCount);
    return messageCount;
}

#pragma mark -
#pragma mark Tag Groups

void UAUnityPlugin_editChannelTagGroups(const char *payload) {
    UA_LDEBUG(@"UnityPlugin editChannelTagGroups");
    id payloadMap = [UAJSONUtils objectWithString:[NSString stringWithUTF8String:payload]];
    id operations = payloadMap[@"values"];

    for (NSDictionary *operation in operations) {
        NSString *group = operation[@"tagGroup"];
        if ([operation[@"operation"] isEqualToString:@"add"]) {
            [[UAirship channel] addTags:operation[@"tags"] group:group];
        } else if ([operation[@"operation"] isEqualToString:@"remove"]) {
            [[UAirship channel] removeTags:operation[@"tags"] group:group];
        }
    }

    [[UAirship push] updateRegistration];
}

void UAUnityPlugin_editNamedUserTagGroups(const char *payload) {
    UA_LDEBUG(@"UnityPlugin editNamedUserTagGroups");
    id payloadMap = [UAJSONUtils objectWithString:[NSString stringWithUTF8String:payload]];
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
#pragma mark Attributes

void UAUnityPlugin_editChannelAttributes(const char *payload) {
    UA_LDEBUG(@"UnityPlugin editChannelAttributes");
    id payloadMap = [UAJSONUtils objectWithString:[NSString stringWithUTF8String:payload]];
    id operations = payloadMap[@"values"];

    UAAttributeMutations *mutations = [[UAUnityPlugin shared] mutationsWithOperations:operations];

    [[UAirship channel] applyAttributeMutations:mutations];
}

void UAUnityPlugin_editNamedUserAttributes(const char *payload) {
    UA_LDEBUG(@"UnityPlugin editNamedUserAttributes");
    id payloadMap = [UAJSONUtils objectWithString:[NSString stringWithUTF8String:payload]];
    id operations = payloadMap[@"values"];

    UAAttributeMutations *mutations = [[UAUnityPlugin shared] mutationsWithOperations:operations];

    [[UAirship namedUser] applyAttributeMutations:mutations];
}

#pragma mark -
#pragma mark Actions!

#pragma mark -
#pragma mark UAPushNotificationDelegate
/**
 * Called when a push notification is received while the app is running in the foreground.
 *
 * @param userInfo The NSDictionary object representing the notification info.
 */
- (void)receivedForegroundNotification:(NSDictionary *)userInfo completionHandler:(void (^)(void))completionHandler {
    UA_LDEBUG(@"receivedForegroundNotification %@",userInfo);

    if (self.listener) {
        UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
                     "OnPushReceived",
                     [UAUnityPlugin convertPushToJson:userInfo]);
        completionHandler();
    }
}


/**
 * Called when the app is started or resumed because a user opened a notification.
 *
 * @param notificationResponse UNNotificationResponse object representing the user's response
 */
- (void)receivedNotificationResponse:(UNNotificationResponse *)notificationResponse completionHandler:(void (^)(void))completionHandler {
    UA_LDEBUG(@"receivedNotificationResponse %@",notificationResponse);
    self.storedNotification = notificationResponse.notification.request.content.userInfo;

    if (self.listener) {
        UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
                         "OnPushOpened",
                         [UAUnityPlugin convertPushToJson:notificationResponse.notification.request.content.userInfo]);
        completionHandler();
    }
}

#pragma mark -
#pragma mark Channel Registration Events


- (void)channelUpdated:(NSNotification *)notification {
    NSString *channelID = notification.userInfo[UAChannel.channelIdentifierKey];
    UA_LDEBUG(@"channelUpdated: %@", channelID);
    if (self.listener && channelID) {
        UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
                         "OnChannelUpdated",
                         MakeStringCopy([channelID UTF8String]));
    }
}

#pragma mark -
#pragma mark UADeepLinkDelegate
-(void)receivedDeepLink:(NSURL *_Nonnull)url completionHandler:(void (^_Nonnull)(void))completionHandler {
    UA_LDEBUG(@"Setting dl to: %@", url);
    NSString *deepLinkString = url.absoluteString;
    self.storedDeepLink = deepLinkString;
    id listener = [UAUnityPlugin shared].listener;
    if (listener) {
        UnitySendMessage(MakeStringCopy([listener UTF8String]),
                         "OnDeepLinkReceived",
                         MakeStringCopy([deepLinkString UTF8String]));
    }

    completionHandler();
}

#pragma mark -
#pragma mark UAMessageCenterDisplayDelegate

- (void)displayMessageCenterForMessageID:(NSString *)messageID animated:(BOOL)animated {
    if (self.autoLaunchMessageCenter) {
        [[UAMessageCenter shared].defaultUI displayMessageCenterForMessageID:messageID animated:true];
    } else {
        UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
        "OnShowInbox",
        MakeStringCopy([messageID UTF8String]));
    }
}

- (void)displayMessageCenterAnimated:(BOOL)animated {
    if (self.autoLaunchMessageCenter) {
        [[UAMessageCenter shared].defaultUI displayMessageCenterAnimated:animated];
    } else {
        UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
        "OnShowInbox",
        MakeStringCopy([@"" UTF8String]));
    }
}

- (void)dismissMessageCenterAnimated:(BOOL)animated {
    if (self.autoLaunchMessageCenter) {
        [[UAMessageCenter shared].defaultUI dismissMessageCenterAnimated:animated];
    }
}

#pragma mark -
#pragma mark UAInboxMessageListUpdatedNotification
- (void)inboxUpdated {
    NSDictionary *counts = @{
        @"unread" : @([UAMessageCenter shared].messageList.unreadCount),
        @"total" : @([UAMessageCenter shared].messageList.messageCount)
    };
    UA_LDEBUG(@"UnityPlugin inboxUpdated(unread = %@, total = %@)", counts[@"unread"], counts[@"total"]);
    UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
                     "OnInboxUpdated",
                     [UAUnityPlugin convertToJson:counts]);
}

#pragma mark -
#pragma mark Data Collection

bool UAUnityPlugin_isFeatureEnabled(const char *features) {
    NSString *featureString = [NSString stringWithUTF8String:features];
    NSArray *featureArray = [featureString componentsSeparatedByString: @","];
    if ([[UAUnityPlugin shared] isValidFeature:featureArray]) {
        UA_LDEBUG(@"UAUnityPlugin isFeatureEnabled %@", featureString);
        return [[UAirship shared].privacyManager isEnabled:[[UAUnityPlugin shared] stringToFeature:featureArray]];
    } else {
        UA_LERR(@"UAUnityPlugin Invalid feature %@", featureString);
        return false;
    }
}

bool UAUnityPlugin_isAnyFeatureEnabled() {
   return [[UAirship shared].privacyManager isAnyFeatureEnabled];
}

void UAUnityPlugin_disableFeatures(const char *features) {
    NSString *featureString = [NSString stringWithUTF8String:features];
    NSArray *featureArray = [featureString componentsSeparatedByString: @","];
    if ([[UAUnityPlugin shared] isValidFeature:featureArray]) {
        UA_LDEBUG(@"UAUnityPlugin disableFeatures");
        [[UAirship shared].privacyManager disableFeatures:[[UAUnityPlugin shared] stringToFeature:featureArray]];
    } else {
        UA_LERR(@"UAUnityPlugin Invalid features, cancelling disableFeatures");
    }
}

void UAUnityPlugin_enableFeatures(const char *features) {
    NSString *featureString = [NSString stringWithUTF8String:features];
    NSArray *featureArray = [featureString componentsSeparatedByString: @","];
    if ([[UAUnityPlugin shared] isValidFeature:featureArray]) {
        UA_LDEBUG(@"UAUnityPlugin enableFeatures");
        [[UAirship shared].privacyManager enableFeatures:[[UAUnityPlugin shared] stringToFeature:featureArray]];
    } else {
        UA_LERR(@"UAUnityPlugin Invalid features, cancelling enableFeatures");
    }
}

void UAUnityPlugin_setEnabledFeatures(const char *features) {
    NSString *featureString = [NSString stringWithUTF8String:features];
    NSArray *featureArray = [featureString componentsSeparatedByString: @","];
    if ([[UAUnityPlugin shared] isValidFeature:featureArray]) {
        UA_LDEBUG(@"UAUnityPlugin setEnabledFeatures");
        [[UAirship shared].privacyManager setEnabledFeatures:[[UAUnityPlugin shared] stringToFeature:featureArray]];
    } else {
        UA_LERR(@"UAUnityPlugin Invalid features, cancelling setEnabledFeatures");
    }
}

const char* UAUnityPlugin_getEnabledFeatures() {
    UA_LDEBUG(@"UAUnityPlugin getEnabledFeatures");
    NSError *error = nil;
    NSArray *featureArray = [[UAUnityPlugin shared] featureToString:[[UAirship shared].privacyManager enabledFeatures]];
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:featureArray options:NSJSONWritingPrettyPrinted error:&error];
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];

    return MakeStringCopy([jsonString UTF8String]);
}

#pragma mark -
#pragma mark Preference Center

void UAUnityPlugin_openPreferenceCenter(NSString *preferenceCenterId) {
    UA_LDEBUG(@"UAUnityPlugin openPreferenceCenter");
    [[UAPreferenceCenter shared] openPreferenceCenter:preferenceCenterId];
}



#pragma mark -
#pragma mark Helpers

+ (NSString *)stringOrNil:(NSString *)string {
    return string.length > 0 ? string : nil;
}

+ (const char *)convertPushToJson:(NSDictionary *)push {
    NSString *alert = push[@"aps"][@"alert"];
    NSString *identifier = push[@"_"];

    NSMutableArray *extras = [NSMutableArray array];
    for (NSString *key in push) {
        if ([key isEqualToString:@"_"] || [key isEqualToString:@"aps"]) {
            continue;
        }

        id value = push[key];
        if (![value isKindOfClass:[NSString class]]) {
            value = [UAJSONUtils stringWithObject:value];
        }

        if (!value) {
            continue;
        }

        [extras addObject:@{@"key": key, @"value": value}];
    }

    NSMutableDictionary *serializedPayload = [NSMutableDictionary dictionary];
    [serializedPayload setValue:alert forKey:@"alert"];
    [serializedPayload setValue:identifier forKey:@"identifier"];

    if (extras.count) {
        [serializedPayload setValue:extras forKey:@"extras"];
    }

    return [UAUnityPlugin convertToJson:serializedPayload];
}

+ (const char *)convertToJson:(NSObject*) obj {
    NSString *JSONString = [UAJSONUtils stringWithObject:obj];
    return MakeStringCopy([JSONString UTF8String]);
}

+ (const char *)convertInboxMessagesToJson:(NSArray<UAInboxMessage *> *)messages {
    NSMutableArray<NSDictionary *> *convertedMessages = [NSMutableArray array];
    for (UAInboxMessage *message in messages) {
        NSMutableDictionary *convertedMessage = [NSMutableDictionary dictionary];
        convertedMessage[@"id"] = message.messageID;
        convertedMessage[@"title"] = message.title;

        NSNumber *sentDate = @([message.messageSent timeIntervalSince1970] * 1000);
        convertedMessage[@"sentDate"] = sentDate;

        NSDictionary *icons = [message.rawMessageObject objectForKey:@"icons"];
        NSString *iconUrl = [icons objectForKey:@"list_icon"];
        convertedMessage[@"listIconUrl"] = iconUrl;

        convertedMessage[@"isRead"] = message.unread ? @NO : @YES;
        convertedMessage[@"isDeleted"] = @(message.deleted);

        if (message.extra) {
            // Unity's JsonArray doesn't support dictionaries, so break extra up into two lists.
            convertedMessage[@"extrasKeys"] = message.extra.allKeys;
            convertedMessage[@"extrasValues"] = message.extra.allValues;
        }

        [convertedMessages addObject:convertedMessage];
    }
    return [UAUnityPlugin convertToJson:convertedMessages];
}

- (void)displayInboxMessage:(NSString *)messageId {
    UAUnityMessageViewController *mvc = [[UAUnityMessageViewController alloc] init];
    [mvc loadMessageForID:messageId];
    self.messageViewController = mvc;

    dispatch_async(dispatch_get_main_queue(), ^{
        [[UIApplication sharedApplication].keyWindow.rootViewController presentViewController:mvc animated:YES completion:nil];
    });
}

- (UAAttributeMutations *)mutationsWithOperations:(NSArray *)operations {
    UAAttributeMutations *mutations = [UAAttributeMutations mutations];

    for (NSDictionary *operation in operations) {
        NSString *action = operation[@"action"];
        NSString *key = operation[@"key"];
        NSString *value = operation[@"value"];
        NSString *type = operation[@"type"];

        if (!action.length || !key.length) {
            UA_LERR(@"Invalid attribute operation %@", operation);
            continue;
        }

        if ([action isEqualToString:@"Set"]) {
            if (!value.length || !type.length) {
                UA_LERR(@"Invalid set operation %@", operation);
                continue;
            }

            if ([type isEqualToString:@"Double"]) {
                [mutations setNumber:@(value.doubleValue) forAttribute:key];
            } else if ([type isEqualToString:@"Float"]) {
                [mutations setNumber:@(value.floatValue) forAttribute:key];
            } else if ([type isEqualToString:@"Long"]) {
                [mutations setNumber:@(value.longLongValue) forAttribute:key];
            } else if ([type isEqualToString:@"Integer"]) {
                [mutations setNumber:@(value.intValue) forAttribute:key];
            } else if ([type isEqualToString:@"String"]) {
                [mutations setString:value forAttribute:key];
            } else if ([type isEqualToString:@"Date"]) {
                NSDate *date = [NSDate dateWithTimeIntervalSince1970:(NSTimeInterval)(value.doubleValue / 1000.0)];
                [mutations setDate:date forAttribute:key];
            }
        } else if ([action isEqualToString:@"Remove"]) {
            [mutations removeAttribute:key];
        }
    }
    return mutations;
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

// Helper method to check if features are authorized
- (BOOL)isValidFeature:(NSArray *)features {
    UA_LDEBUG(@"checking isValidFeature");
    NSLog(@"%@", features);
    if (!features || [features count] == 0) {
        return NO;
    }
    NSDictionary *authorizedFeatures = [self authorizedFeatures];
    for (NSString *feature in features) {
        if (![authorizedFeatures objectForKey:feature]) {
            return NO;
        }
    }
    return YES;
}

// Helper method to convert features as int
- (UAFeatures)stringToFeature:(NSArray *)features {
    NSDictionary *authorizedFeatures = [self authorizedFeatures];

    NSNumber *objectFeature = authorizedFeatures[[features objectAtIndex:0]];
    UAFeatures convertedFeatures = [objectFeature longValue];

    if ([features count] > 1) {
        int i;
        for (i = 1; i < [features count]; i++) {
            NSNumber *objectFeature = authorizedFeatures[[features objectAtIndex:i]];
            convertedFeatures |= [objectFeature longValue];
        }
    }
    return convertedFeatures;
}

// Helper method to convert features to string.
- (NSArray *)featureToString:(UAFeatures)features {
    NSMutableArray *convertedFeatures = [[NSMutableArray alloc] init];

    NSDictionary *authorizedFeatures = [self authorizedFeatures];

    if (features == UAFeaturesAll) {
        [convertedFeatures addObject:@"FEATURE_ALL"];
    } else if (features == UAFeaturesNone) {
        [convertedFeatures addObject:@"FEATURE_NONE"];
    } else {
        for (NSString *feature in authorizedFeatures) {
            NSNumber *objectFeature = authorizedFeatures[feature];
            long longFeature = [objectFeature longValue];
            if ((longFeature & features) && (longFeature != UAFeaturesAll)) {
                [convertedFeatures addObject:feature];
            }
        }
    }
    return convertedFeatures;
}

// Dictionary of authorized features
- (NSDictionary *)authorizedFeatures {
    NSMutableDictionary *authorizedFeatures = [[NSMutableDictionary alloc] init];
    [authorizedFeatures setValue:@(UAFeaturesNone) forKey:@"FEATURE_NONE"];
    [authorizedFeatures setValue:@(UAFeaturesInAppAutomation) forKey:@"FEATURE_IN_APP_AUTOMATION"];
    [authorizedFeatures setValue:@(UAFeaturesMessageCenter) forKey:@"FEATURE_MESSAGE_CENTER"];
    [authorizedFeatures setValue:@(UAFeaturesPush) forKey:@"FEATURE_PUSH"];
    [authorizedFeatures setValue:@(UAFeaturesChat) forKey:@"FEATURE_CHAT"];
    [authorizedFeatures setValue:@(UAFeaturesAnalytics) forKey:@"FEATURE_ANALYTICS"];
    [authorizedFeatures setValue:@(UAFeaturesTagsAndAttributes) forKey:@"FEATURE_TAGS_AND_ATTRIBUTES"];
    [authorizedFeatures setValue:@(UAFeaturesContacts) forKey:@"FEATURE_CONTACTS"];
    [authorizedFeatures setValue:@(UAFeaturesLocation) forKey:@"FEATURE_LOCATION"];
    [authorizedFeatures setValue:@(UAFeaturesAll) forKey:@"FEATURE_ALL"];
    return authorizedFeatures;
}

@end
