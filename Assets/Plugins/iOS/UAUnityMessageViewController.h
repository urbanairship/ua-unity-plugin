/* Copyright Urban Airship and Contributors */

#if __has_include("AirshipLib.h")
#import "AirshipLib.h"
#import "AirshipMessageCenterLib.h"
#else
@import Airship;
#endif

NS_ASSUME_NONNULL_BEGIN

@interface UAUnityMessageViewController : UAMessageCenterMessageViewController

@end

NS_ASSUME_NONNULL_END
