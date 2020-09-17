/* Copyright Urban Airship and Contributors */

#if __has_include("AirshipLib.h")
#import "AirshipLib.h"
#import "AirshipMessageCenterLib.h"
#else
@import Airship;
#endif

NS_ASSUME_NONNULL_BEGIN

@interface UAUnityMessageViewController : UINavigationController

- (void)loadMessageForID:(nullable NSString *)messageID;

@end

NS_ASSUME_NONNULL_END
