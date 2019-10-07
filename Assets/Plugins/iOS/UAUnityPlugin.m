/* Copyright Airship and Contributors */

#import "UAUnityPlugin.h"
#import "UnityInterface.h"
#import "AirshipLib.h"
#import "AirshipLocationLib.h"
#import "UAUnityMessageViewController.h"

static UAUnityPlugin *shared_;
static dispatch_once_t onceToken_;

NSString *const UAUnityAutoLaunchMessageCenterKey = @"com.urbanairship.auto_launch_message_center";

@interface UAUnityPlugin()
@property (nonatomic, weak) UAInAppMessageHTMLAdapter *htmlAdapter;
@property (nonatomic, assign) BOOL factoryBlockAssigned;
@property (nonatomic, strong) UAUnityMessageViewController *messageViewController;

- (BOOL)autoLaunchMessageCenter;
@end

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

    [[UAirship shared].actionRegistry updateAction:customDIA forEntryWithName:kUADisplayInboxActionDefaultRegistryName];
    [[UAirship shared].actionRegistry updateAction:customLPA forEntryWithName:kUALandingPageActionDefaultRegistryName];
    
    // Add observer for inbox updated event
    [[NSNotificationCenter defaultCenter] addObserver:[self shared]
                                             selector:@selector(inboxUpdated)
                                                 name:UAInboxMessageListUpdatedNotification
                                               object:nil];

    [UAirship inbox].delegate = [self shared];
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

// getter and setter for component enabled flag
- (BOOL)autoLaunchMessageCenter {
    if ([[NSUserDefaults standardUserDefaults] objectForKey:UAUnityAutoLaunchMessageCenterKey] == nil) {
        [[NSUserDefaults standardUserDefaults] setBool:YES forKey:UAUnityAutoLaunchMessageCenterKey];
        [[NSUserDefaults standardUserDefaults] synchronize];
    }
    
    return [[NSUserDefaults standardUserDefaults] boolForKey:UAUnityAutoLaunchMessageCenterKey];
}

- (void)setAutoLaunchMessageCenter:(BOOL)autoLaunchMessageCenter {
    [[NSUserDefaults standardUserDefaults] setBool:autoLaunchMessageCenter forKey:UAUnityAutoLaunchMessageCenterKey];
    [[NSUserDefaults standardUserDefaults] synchronize];
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

const char* UAUnityPlugin_getChannelId() {
    UA_LDEBUG(@"UnityPlugin getChannelId");
    return MakeStringCopy([[UAirship push].channelID UTF8String]);
}

#pragma mark -
#pragma mark UA Location Functions

bool UAUnityPlugin_isLocationEnabled() {
    UA_LDEBUG(@"UnityPlugin isLocationEnabled");
    return [UALocation sharedLocation].locationUpdatesEnabled ? true : false;
}

void UAUnityPlugin_setLocationEnabled(bool enabled) {
    UA_LDEBUG(@"UnityPlugin setLocationEnabled: %d", enabled);
    [UALocation sharedLocation].locationUpdatesEnabled = enabled;
}

bool UAUnityPlugin_isBackgroundLocationAllowed() {
    UA_LDEBUG(@"UnityPlugin isBackgroundLocationAllowed");
    return [UALocation sharedLocation].backgroundLocationUpdatesAllowed ? true : false;
}

void UAUnityPlugin_setBackgroundLocationAllowed(bool enabled) {
    UA_LDEBUG(@"UnityPlugin setBackgroundLocationAllowed: %d", enabled);
    [UALocation sharedLocation].backgroundLocationUpdatesAllowed = enabled ? YES : NO;
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
    [[UAirship messageCenter] display];
}

void UAUnityPlugin_displayInboxMessage(const char *messageID, bool overlay) {
    NSString *messageIDString = [NSString stringWithUTF8String:messageID];
    UA_LDEBUG(@"UnityPlugin displayInboxMessage %@, overlay = %@ ", messageIDString, overlay ? @"YES" : @"NO");
    UnityWillPause();
    if (overlay) {
        [[UAUnityPlugin shared] displayOverlayMessage:messageIDString];
    } else {
        [[UAUnityPlugin shared] displayInboxMessage:messageIDString];
    }
}

void UAUnityPlugin_refreshInbox() {
    UA_LDEBUG(@"UnityPlugin refreshInbox");
    UnityWillPause();
    [[UAirship inbox].messageList retrieveMessageListWithSuccessBlock:^(){} withFailureBlock:^(){}];
}

const char* UAUnityPlugin_getInboxMessages() {
    UA_LDEBUG(@"UnityPlugin getInboxMessages");
    return [UAUnityPlugin convertInboxMessagesToJson:[UAirship inbox].messageList.messages];
}

void UAUnityPlugin_markInboxMessageRead(const char *messageID) {
    NSString *messageIDString = [NSString stringWithUTF8String:messageID];
    UA_LDEBUG(@"UnityPlugin markInboxMessageRead %@", messageIDString);
    UAInboxMessage *message = [[UAirship inbox].messageList messageForID:messageIDString];
    [[UAirship inbox].messageList markMessagesRead:@[message] completionHandler:nil];
}

void UAUnityPlugin_deleteInboxMessage(const char *messageID) {
    NSString *messageIDString = [NSString stringWithUTF8String:messageID];
    UA_LDEBUG(@"UnityPlugin deleteInboxMessage %@", messageIDString);
    UAInboxMessage *message = [[UAirship inbox].messageList messageForID:messageIDString];
    [[UAirship inbox].messageList markMessagesDeleted:@[message] completionHandler:nil];
}

void UAUnityPlugin_setAutoLaunchDefaultMessageCenter(bool enabled) {
    UA_LDEBUG(@"UnityPlugin UAUnityPlugin_setAutoLaunchDefaultMessageCenter %@", enabled ? @"YES" : @"NO");
    [UAUnityPlugin shared].autoLaunchMessageCenter = enabled;
}

int UAUnityPlugin_getMessageCenterUnreadCount() {
    int unreadCount = (int)[UAirship inbox].messageList.unreadCount;
    UA_LDEBUG(@"UnityPlugin getMessageCenterUnreadCount: %d", unreadCount);
    return unreadCount;
}

int UAUnityPlugin_getMessageCenterCount() {
    int messageCount = (int)[UAirship inbox].messageList.messageCount;
    UA_LDEBUG(@"UnityPlugin getMessageCenterCount: %d", messageCount);
    return messageCount;
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
- (void)receivedForegroundNotification:(UANotificationContent *)notificationContent completionHandler:(void (^)(void))completionHandler {
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
- (void)receivedNotificationResponse:(UANotificationResponse *)notificationResponse completionHandler:(void (^)(void))completionHandler {
    UA_LDEBUG(@"receivedNotificationResponse %@",notificationResponse);
    self.storedNotification = notificationResponse.notificationContent.notificationInfo;

    if (self.listener) {
        UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
                         "OnPushOpened",
                         [UAUnityPlugin convertPushToJson:notificationResponse.notificationContent.notificationInfo]);
        completionHandler();
    }
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
#pragma mark UAInboxDelegate
- (void)showMessageForID:(NSString *)messageID {
    UA_LDEBUG(@"showMessageForID: %@", messageID);
    if ([[NSUserDefaults standardUserDefaults] boolForKey:UAUnityAutoLaunchMessageCenterKey]) {
        [[UAirship messageCenter] displayMessageForID:messageID];
    } else {
        UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
                         "OnShowInbox",
                         MakeStringCopy([messageID UTF8String]));
    }
}

- (void)showInbox {
    UA_LDEBUG(@"showInbox");
    if ([[NSUserDefaults standardUserDefaults] boolForKey:UAUnityAutoLaunchMessageCenterKey]) {
        [[UAirship messageCenter] display];
    } else {
        UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
                         "OnShowInbox",
                         MakeStringCopy([@"" UTF8String]));
    }
}

#pragma mark -
#pragma mark UAInboxMessageListUpdatedNotification
- (void)inboxUpdated {
    NSDictionary *counts = @{
        @"unread" : @([UAirship inbox].messageList.unreadCount),
        @"total" : @([UAirship inbox].messageList.messageCount)
    };
    UA_LDEBUG(@"UnityPlugin inboxUpdated(unread = %@, total = %@)", counts[@"unread"], counts[@"total"]);
    UnitySendMessage(MakeStringCopy([self.listener UTF8String]),
                     "OnInboxUpdated",
                     [UAUnityPlugin convertToJson:counts]);
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
            value = [NSJSONSerialization stringWithObject:value acceptingFragments:YES];
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
    NSString *JSONString = [NSJSONSerialization stringWithObject:obj acceptingFragments:YES];
    return MakeStringCopy([JSONString UTF8String]);
}

+ (const char *)convertInboxMessagesToJson:(NSArray<UAInboxMessage *> *)messages {
    NSMutableArray<NSDictionary *> *convertedMessages = [NSMutableArray arrayWithCapacity:messages.count];
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
    UAUnityMessageViewController *mvc = [[UAUnityMessageViewController alloc] initWithNibName:@"UAMessageCenterMessageViewController" bundle:[UAirship resources]];
    [mvc loadMessageForID:messageId onlyIfChanged:YES onError:nil];

    UINavigationController *navController =  [[UINavigationController alloc] initWithRootViewController:mvc];
    self.messageViewController = mvc;

    dispatch_async(dispatch_get_main_queue(), ^{
        [[UIApplication sharedApplication].keyWindow.rootViewController presentViewController:navController animated:YES completion:nil];
    });
}

- (void)displayOverlayMessage:(NSString *)messageId {
    if (!self.factoryBlockAssigned) {
        [[UAirship inAppMessageManager] setFactoryBlock:^id<UAInAppMessageAdapterProtocol> _Nonnull(UAInAppMessage * _Nonnull message) {
            UAInAppMessageHTMLAdapter *adapter = [UAInAppMessageHTMLAdapter adapterForMessage:message];
            UAInAppMessageHTMLDisplayContent *displayContent = (UAInAppMessageHTMLDisplayContent *) message.displayContent;
            NSURL *url = [NSURL URLWithString:displayContent.url];

            if ([url.scheme isEqualToString:@"message"]) {
                self.htmlAdapter = adapter;
            }

            return adapter;
        } forDisplayType:UAInAppMessageDisplayTypeHTML];

        self.factoryBlockAssigned = YES;
    }

    [UAActionRunner runActionWithName:kUAOverlayInboxMessageActionDefaultRegistryName
                                value:messageId
                            situation:UASituationManualInvocation];
}

- (void)closeOverlayMessage {
    UIViewController *vc = [self.htmlAdapter valueForKey:@"htmlViewController"];
# pragma clang diagnostic push
# pragma clang diagnostic ignored "-Warc-performSelector-leaks"
    [vc performSelector:NSSelectorFromString(@"dismissWithoutResolution")];
# pragma clang diagnostic pop
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
