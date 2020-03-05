/* Copyright Airship and Contributors */

#import <Foundation/Foundation.h>
#import "AirshipLib.h"

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
#pragma mark UA Location Functions

bool UAUnityPlugin_isLocationEnabled();
void UAUnityPlugin_setLocationEnabled(bool enabled);

bool UAUnityPlugin_isBackgroundLocationAllowed();
void UAUnityPlugin_setBackgroundLocationAllowed(bool allowed);

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
#pragma mark Tag Groups

void UAUnityPlugin_editNamedUserTagGroups(const char *payload);
void UAUnityPlugin_editChannelTagGroups(const char *payload);

#pragma mark -
#pragma mark Attributes

void UAUnityPlugin_editChannelAttributes(const char *payload);

@interface UAUnityPlugin : NSObject <UAPushNotificationDelegate, UARegistrationDelegate, UADeepLinkDelegate, UAInboxDelegate>

+ (UAUnityPlugin *)shared;

@property (nonatomic, copy) NSString* listener;
@property (nonatomic, strong) NSDictionary* storedNotification;
@property (nonatomic, copy) NSString* storedDeepLink;

@end
