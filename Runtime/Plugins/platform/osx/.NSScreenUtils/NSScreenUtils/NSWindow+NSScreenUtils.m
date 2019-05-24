//
//  NSWindow+NSScreenUtils.m
//  NSScreenUtils
//
//  Created by Justin Fincher on 24/5/2019.
//  Copyright © 2019 Justin Fincher. All rights reserved.
//

#import "NSWindow+NSScreenUtils.h"
#import <objc/runtime.h>
#import "UIWidgetsMessageManager.h"

@implementation NSWindow (NSScreenUtils)


/**
 Using method swizzling to inject notification on library load. Notice this bundle should be marked as 'Load on Start' in Unity inspector panel
 */
+ (void)load {
    NSLog(@"NSWindow + NSScreenUtils Load");
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^
                  {
                      NSString *bundleIdentifier = [[NSBundle mainBundle] bundleIdentifier];
                      if ([bundleIdentifier isEqualToString:@"com.unity3d.UnityEditor5.x"]) {
                      }
                      else
                      {
                          [UIWidgetsMessageManager getInstance];
                          NSLog(@"NSWindow + NSScreenUtils Enabled for %@",bundleIdentifier);
                          [self exchangeClassMethodMethod:@selector(initWithContentRect:styleMask:backing:defer:) with:@selector(utils_initWithContentRect:styleMask:backing:defer:)];
                          [self exchangeClassMethodMethod:@selector(initWithContentRect:styleMask:backing:defer:screen:) with:@selector(utils_initWithContentRect:styleMask:backing:defer:screen:)];
                      }
                  });
}


/**
 Swizzling Helper

 @param originalSelector original method
 @param swizzledSelector replace method
 */
+ (void)exchangeClassMethodMethod:(SEL)originalSelector with:(SEL)swizzledSelector
{
    Class class = object_getClass((id)self);
    Method originalMethod = class_getClassMethod(class, originalSelector);
    Method swizzledMethod = class_getClassMethod(class, swizzledSelector);
    BOOL didAddMethod = class_addMethod(class,
                                        originalSelector,
                                        method_getImplementation(swizzledMethod),
                                        method_getTypeEncoding(swizzledMethod));
    
    if (didAddMethod) {
        class_replaceMethod(class,
                            swizzledSelector,
                            method_getImplementation(originalMethod),
                            method_getTypeEncoding(originalMethod));
    } else {
        method_exchangeImplementations(originalMethod, swizzledMethod);
    }
}

#pragma mark - Method Swizzling

- (instancetype)utils_initWithContentRect:(NSRect)contentRect styleMask:(NSWindowStyleMask)style backing:(NSBackingStoreType)backingStoreType defer:(BOOL)flag screen:(NSScreen *)screen
{
    NSWindow * instance = [self utils_initWithContentRect:contentRect styleMask:style backing:backingStoreType defer:flag screen:screen];
    if (instance) {
        NSLog(@"utils_initWithContentRect:styleMask:backing:defer:screen:");
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onWindowDidChangeBackingProperties:) name:NSWindowDidChangeBackingPropertiesNotification object:nil];
    }
    return instance;
}
- (instancetype)utils_initWithContentRect:(NSRect)contentRect styleMask:(NSWindowStyleMask)style backing:(NSBackingStoreType)backingStoreType defer:(BOOL)flag
{
    NSWindow * instance = [self utils_initWithContentRect:contentRect styleMask:style backing:backingStoreType defer:flag];
    if (instance) {
        NSLog(@"utils_initWithContentRect:styleMask:backing:defer:");
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(onWindowDidChangeBackingProperties:) name:NSWindowDidChangeBackingPropertiesNotification object:nil];
    }
    return instance;
}

#pragma mark - Callback


- (void)onWindowDidChangeBackingProperties:(NSNotification *)notification
{
    NSLog(@"onWindowDidChangeBackingProperties");
    [[UIWidgetsMessageManager getInstance] UIWidgetsMethodMessage:@"ViewportMatricsChanged" :@"UIWidgetViewController.keyboardChanged" :[NSArray array]];
}
@end
