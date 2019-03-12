#ifndef PLATFORM_IOS_FRAMEWORK_SOURCE_UIWIDGETSTEXTINPUTPLUGIN_H_
#define PLATFORM_IOS_FRAMEWORK_SOURCE_UIWIDGETSTEXTINPUTPLUGIN_H_

#import <UIKit/UIKit.h>
#include "UIWidgetsTextInputDelegate.h"

@interface UIWidgetsTextInputPlugin : NSObject

@property(nonatomic, assign) id<UIWidgetsTextInputDelegate> textInputDelegate;

- (UIView<UITextInput>*)textInputView;

@end

@interface UIWidgetsTextPosition : UITextPosition

@property(nonatomic, readonly) NSUInteger index;

+ (instancetype)positionWithIndex:(NSUInteger)index;
- (instancetype)initWithIndex:(NSUInteger)index;

@end

@interface UIWidgetsTextRange : UITextRange <NSCopying>

@property(nonatomic, readonly) NSRange range;

+ (instancetype)rangeWithNSRange:(NSRange)range;

@end

#endif  // PLATFORM_IOS_FRAMEWORK_SOURCE_UIWIDGETSTEXTINPUTPLUGIN_H_
