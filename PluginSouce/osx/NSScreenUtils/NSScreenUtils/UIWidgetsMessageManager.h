#import <Foundation/Foundation.h>

@interface UIWidgetsMessageManager : NSObject

@property (nonatomic, retain) NSString *someProperty;

+ (id) getInstance;
- (void) UIWidgetsMethodMessage:(NSString *)channel :(NSString*) method :(NSArray *)args;

#ifdef __cplusplus
extern "C" {
#endif
    void UIWidgetsMessageSetObjectName(const char *name);
#ifdef __cplusplus
}
#endif

@end
