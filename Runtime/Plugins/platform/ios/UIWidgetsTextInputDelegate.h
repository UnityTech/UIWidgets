#ifndef PLATFORM_IOS_FRAMEWORK_SOURCE_UIWIDGETSTEXTINPUTDELEGATE_H_
#define PLATFORM_IOS_FRAMEWORK_SOURCE_UIWIDGETSTEXTINPUTDELEGATE_H_
#import <Foundation/Foundation.h>

typedef NS_ENUM(NSInteger, UIWidgetsTextInputAction) {
    UIWidgetsTextInputActionUnspecified,
    UIWidgetsTextInputActionDone,
    UIWidgetsTextInputActionGo,
    UIWidgetsTextInputActionSend,
    UIWidgetsTextInputActionSearch,
    UIWidgetsTextInputActionNext,
    UIWidgetsTextInputActionContinue,
    UIWidgetsTextInputActionJoin,
    UIWidgetsTextInputActionRoute,
    UIWidgetsTextInputActionEmergencyCall,
    UIWidgetsTextInputActionNewline,
};

@protocol UIWidgetsTextInputDelegate <NSObject>

- (void)updateEditingClient:(int)client withState:(NSDictionary*)state;
- (void)performAction:(UIWidgetsTextInputAction)action withClient:(int)client;

@end


@interface DefaultUIWidgetsTextInputDelegate : NSObject <UIWidgetsTextInputDelegate>
- (void)updateEditingClient:(int)client withState:(NSDictionary*)state;
- (void)performAction:(UIWidgetsTextInputAction)action withClient:(int)client;
@end

#endif  // PLATFORM_IOS_FRAMEWORK_SOURCE_UIWIDGETSTEXTINPUTDELEGATE_H_
