#include "UIWidgetsViewController.h"
#include "UIWidgetsMessageManager.h"
#include "UIWidgetsDevice.h"
#include <Foundation/Foundation.h>
#include <UIKit/UIKit.h>

@implementation UIWidgetsViewController {
}

@synthesize viewInsets;
@synthesize padding;

- (instancetype)init {
    self = [super init];
    if (self) {
        viewInsets.bottom = 0;
        viewInsets.top = 0;
        viewInsets.left = 0;
        viewInsets.right = 0;

        CGFloat scale = [[UIScreen mainScreen] scale];
        if (@available(iOS 11, *)) {
            padding.bottom = [UIApplication sharedApplication].keyWindow.safeAreaInsets.bottom * scale;
            padding.top = [UIApplication sharedApplication].keyWindow.safeAreaInsets.top * scale;
            padding.left = [UIApplication sharedApplication].keyWindow.safeAreaInsets.left * scale;
            padding.right = [UIApplication sharedApplication].keyWindow.safeAreaInsets.right * scale;
        } else {
            CGRect statusFrame = [UIApplication sharedApplication].statusBarFrame;
            padding.bottom = 0;
            padding.top = statusFrame.size.height * scale;
            padding.left = 0;
            padding.right = 0;
        }

        NSNotificationCenter* center = [NSNotificationCenter defaultCenter];
        [center addObserver:self
                   selector:@selector(keyboardWillChangeFrame:)
                       name:UIKeyboardWillChangeFrameNotification
                     object:nil];
        [center addObserver:self
                   selector:@selector(keyboardWillBeHidden:)
                       name:UIKeyboardWillHideNotification
                     object:nil];
    }

    return self;
}

-(void)keyboardWillBeHidden:(NSNotification*)notification {
    viewInsets.bottom = 0;
    CGFloat scale = [UIScreen mainScreen].scale;
    if (@available(iOS 11, *)) {
        CGFloat cur_padding = [UIApplication sharedApplication].keyWindow.safeAreaInsets.bottom * scale;
        padding.bottom = cur_padding;
    } else {
        CGRect statusFrame = [UIApplication sharedApplication].statusBarFrame;
        CGFloat cur_padding = statusFrame.size.height * scale;
        padding.top = cur_padding;
    }

    UIWidgetsMethodMessage(@"ViewportMatricsChanged", @"UIWidgetViewController.keyboardChanged", @[]);
}

-(void)keyboardWillChangeFrame:(NSNotification*)notification {
    NSDictionary* info = [notification userInfo];
    CGFloat bottom = CGRectGetHeight([[info objectForKey:UIKeyboardFrameEndUserInfoKey] CGRectValue]);
    CGFloat scale = [UIScreen mainScreen].scale;

    viewInsets.bottom = bottom * scale;
    padding.bottom = 0;

    UIWidgetsMethodMessage(@"ViewportMatricsChanged", @"UIWidgetViewController.keyboardChanged", @[]);
}

-(void)tryLaunch {

}

+ (instancetype)sharedInstance {
    static UIWidgetsViewController *sharedInstance = nil;
    static dispatch_once_t onceToken;

    dispatch_once(&onceToken, ^{
        sharedInstance = [[UIWidgetsViewController alloc] init];
    });
    return sharedInstance;
}

@end

extern "C"
{
    viewMetrics IOSGetViewportPadding()
    {
        viewMetrics metrics;
        viewPadding insets = [[UIWidgetsViewController sharedInstance] viewInsets];
        viewPadding padding = [[UIWidgetsViewController sharedInstance] padding];

        BOOL needDownsample = [UIWidgetsDevice NeedScreenDownSample];

        metrics.insets_bottom = needDownsample ? insets.bottom * 0.8696 : insets.bottom;
        metrics.insets_top = needDownsample ? insets.top * 0.8696 : insets.top;
        metrics.insets_left = needDownsample ? insets.left * 0.8696 : insets.left;
        metrics.insets_right = needDownsample ? insets.right * 0.8696 : insets.right;
        metrics.padding_bottom = needDownsample ? padding.bottom * 0.8696 : padding.bottom;
        metrics.padding_top = needDownsample ? padding.top * 0.8696 : padding.top;
        metrics.padding_left = needDownsample ? padding.left * 0.8696 : padding.left;
        metrics.padding_right = needDownsample ? padding.right * 0.8696 : padding.right;

        return metrics;
    }
}
