#ifndef PLATFORM_IOS_FRAMEWORK_SOURCE_UIWIDGETSVIEWCONTROLLER_H_
#define PLATFORM_IOS_FRAMEWORK_SOURCE_UIWIDGETSVIEWCONTROLLER_H_

#import <UIKit/UIKit.h>
#include "UIWidgetsTextInputDelegate.h"


struct viewPadding
{
    float top;
    float bottom;
    float left;
    float right;
};

struct viewMetrics
{
    float insets_top;
    float insets_bottom;
    float insets_left;
    float insets_right;
    
    float padding_top;
    float padding_bottom;
    float padding_left;
    float padding_right;
};

@interface UIWidgetsViewController : NSObject
@property viewPadding padding;
@property viewPadding viewInsets;
@end

#endif  // PLATFORM_IOS_FRAMEWORK_SOURCE_UIWIDGETSVIEWCONTROLLER_H_
