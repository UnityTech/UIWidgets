#include "UIWidgetsMessageManager.h"

static NSString *unityObjectName = NULL;

@interface UIWidgetsMessageManager()

@end

void UnityOSXSendMessage(const char *name,const char *method,const char *arg);

@implementation UIWidgetsMessageManager

#pragma mark Singleton Methods

+ (id)getInstance {
    static UIWidgetsMessageManager *sharedMyManager = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedMyManager = [[self alloc] init];
    });
    return sharedMyManager;
}

- (id)init {
    if (self = [super init])
    {
        
    }
    return self;
}

#pragma mark Message

void UIWidgetsMessageSetObjectName(const char *name)
{
    NSLog(@"UIWidgetsMessageSetObjectName %@",[NSString stringWithUTF8String:name]);
    unityObjectName = [NSString stringWithUTF8String:name];
}

- (void)UIWidgetsMethodMessage:(NSString *)channel :(NSString *)method :(NSArray *)args
{
    NSError *error;
    NSDictionary* dict = @{
                           @"channel": channel,
                           @"method": method,
                           @"args": args
                           };
    
    NSData* data = [NSJSONSerialization dataWithJSONObject:dict options:0 error:&error];
    NSString* text = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    UnityOSXSendMessage([unityObjectName UTF8String], "OnUIWidgetsMethodMessage", [text UTF8String]);
}

@end

