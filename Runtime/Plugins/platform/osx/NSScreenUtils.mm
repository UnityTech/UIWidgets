//
//  NSScreenUtils.m
//  NSScreenUtils
//
//  Created by Justin Fincher on 21/5/2019.
//  Copyright © 2019 Justin Fincher. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <AppKit/AppKit.h>

float OSXDeviceScaleFactor()
{
    NSArray *ar = [NSApp orderedWindows];
    NSWindow *window = [ar objectAtIndex:0];
    NSScreen *screen = window.screen;
    if (!screen) {
        screen = [NSScreen mainScreen];
    }
    if (screen)
    {
        return screen.backingScaleFactor;
    }
    return 1;
}
