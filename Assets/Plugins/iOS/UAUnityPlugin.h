/* Copyright Airship and Contributors */

#import <Foundation/Foundation.h>
#if __has_include("UAirship.h")
#import "UAirship.h"
#import "UAMessageCenter.h"
#import "UAPush.h"
#import "UAInboxMessage.h"
#import "UAAnalytics.h"
#import "UAInboxMessageList.h"
#import "UAInAppAutomation.h"
#import "UADefaultMessageCenterUI.h"
#import "UAAssociatedIdentifiers.h"
#else
@import AirshipKit;
#endif

extern void UnitySendMessage(const char *, const char *, const char *);

#pragma mark -
#pragma mark Listener

void UAUnityPlugin_setListener(const char* listener);

#pragma mark -
#pragma mark Deep Links

const char* UAUnityPlugin_getDeepLink(bool clear);

#pragma mark -
#pragma mark UA Push Functions

const char* UAUnityPlugin_getIncomingPush(bool clear);

bool UAUnityPlugin_getUserNotificationsEnabled();
void UAUnityPlugin_setUserNotificationsEnabled(bool enabled);

const char* UAUnityPlugin_getTags();
void UAUnityPlugin_addTag(const char* tag);
void UAUnityPlugin_removeTag(const char* tag);

const char* UAUnityPlugin_getChannelId();

#pragma mark -
#pragma mark Custom Events
void UAUnityPlugin_addCustomEvent(const char *customEvent);

#pragma mark -
#pragma mark Associated Identifier
void UAUnityPlugin_associateIdentifier(const char *key, const char *identifier);

#pragma mark -
#pragma mark Named User
void UAUnityPlugin_setNamedUserID(const char *namedUserID);
const char* UAUnityPlugin_getNamedUserID();

#pragma mark -
#pragma mark Message Center
void UAUnityPlugin_displayMessageCenter();
void UAUnityPlugin_displayInboxMessage(const char *messageId);
void UAUnityPlugin_refreshInbox();
const char* UAUnityPlugin_getInboxMessages();
void UAUnityPlugin_markInboxMessageRead(const char *messageID);
void UAUnityPlugin_deleteInboxMessage(const char *messageID);
void UAUnityPlugin_setAutoLaunchDefaultMessageCenter(bool enabled);
int UAUnityPlugin_getMessageCenterUnreadCount();
int UAUnityPlugin_getMessageCenterCount();

#pragma mark -
#pragma mark In-app

double UAUnityPlugin_getInAppAutomationDisplayInterval();
void UAUnityPlugin_setInAppAutomationDisplayInterval(double value);
bool UAUnityPlugin_isInAppAutomationPaused();
void UAUnityPlugin_setInAppAutomationPaused(bool paused);

#pragma mark -
#pragma mark Tag Groups

void UAUnityPlugin_editNamedUserTagGroups(const char *payload);
void UAUnityPlugin_editChannelTagGroups(const char *payload);

#pragma mark -
#pragma mark Attributes

void UAUnityPlugin_editChannelAttributes(const char *payload);
void UAUnityPlugin_editNamedUserAttributes(const char *payload);

#pragma mark -
#pragma mark Data Collection
void UAUnityPlugin_enableFeature(NSString *feature);
void UAUnityPlugin_setEnabledFeatures(NSArray* features);
void UAUnityPlugin_disableFeatures(NSArray* features);
void UAUnityPlugin_enableFeatures(NSArray* features);
bool UAUnityPlugin_isFeatureEnabled(NSString *feature);
NSArray* UAUnityPlugin_getEnabledFeatures();

#pragma mark -
#pragma mark Preference Center

void UAUnityPlugin_openPreferenceCenter(NSString *preferenceCenterId);

#pragma mark -
#pragma mark Helpers
bool isValidFeature(NSArray *features);
UAFeatures stringToFeature(NSArray *features);
NSArray * featureToString(UAFeatures features);

@interface UAUnityPlugin : NSObject <UAPushNotificationDelegate, UADeepLinkDelegate,  UAMessageCenterDisplayDelegate>

+ (UAUnityPlugin *)shared;

@property (nonatomic, copy) NSString* listener;
@property (nonatomic, strong) NSDictionary* storedNotification;
@property (nonatomic, copy) NSString* storedDeepLink;

@end
