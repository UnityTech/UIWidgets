#import <Foundation/Foundation.h>

@interface UIWidgetsMessageManager : NSObject

@property (nonatomic, retain) NSString *someProperty;

/**
 Get Instance of UIWidgetsMessageManager Singleton

 @return instance
 */
+ (id) getInstance;


/**
 Send Message From Objective-C to C# & UIWidgets

 @param channel Channel Name
 @param method Method Name
 @param args Arg Array
 */
- (void) UIWidgetsMethodMessage:(NSString *)channel :(NSString*) method :(NSArray *)args;

#ifdef __cplusplus
extern "C" {
#endif
    
    /**
     Set Object Name From C# to Objective-C, the value will be stored within UIWidgetsMessageManager

     @param name Object name
     */
    void UIWidgetsMessageSetObjectName(const char *name);
#ifdef __cplusplus
}
#endif

@end
