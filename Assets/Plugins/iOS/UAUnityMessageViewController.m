/* Copyright Urban Airship and Contributors */

#import "UAUnityMessageViewController.h"

@implementation UAUnityMessageViewController

- (void) viewDidLoad {
    [super viewDidLoad];
    
    UIBarButtonItem *done = [[UIBarButtonItem alloc]
                             initWithBarButtonSystemItem:UIBarButtonSystemItemDone
                             target:self
                             action:@selector(dismissMessageViewController:)];
    
    self.navigationItem.rightBarButtonItem = done;
    
    __weak UAUnityMessageViewController *weakSelf = self;
    self.closeBlock = ^(BOOL animated) {
        [weakSelf dismissViewControllerAnimated:animated completion:nil];
    };
}

- (void) dismissMessageViewController:(id)sender {
    [self dismissViewControllerAnimated:true completion:nil];
}

@end
