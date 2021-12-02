/* Copyright Urban Airship and Contributors */

#if __has_include("UAirship.h")
#import "UAirship.h"
#import "UAMessageCenter.h"
#import "UAInboxMessage.h"
#import "UAInboxMessageList.h"
#import "UAMessageCenterMessageViewDelegate.h"
#import "UADefaultMessageCenterMessageViewController.h"
#import "UAMessageCenterResources.h"
#import "UAMessageCenterLocalization.h"
#else
@import AirshipKit;
#endif

NS_ASSUME_NONNULL_BEGIN

@interface UAUnityMessageViewController : UINavigationController

- (void)loadMessageForID:(nullable NSString *)messageID;

@end

NS_ASSUME_NONNULL_END
