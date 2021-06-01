//
//  NSScreenUtils.m
//  NSScreenUtils
//
//  Created by Justin Fincher on 21/5/2019.
//  Copyright © 2019 Justin Fincher. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <AppKit/AppKit.h>
#import "NSScreenUtils.h"


static UnityOSXCallback callback = NULL;

void LinkUnityOSXCallback(UnityOSXCallback externalCallback)
{
    callback = externalCallback;
}

void UnityOSXSendMessage(const char *name,const char *method,const char *arg)
{
    if (callback) {
        callback(name,method,arg);
    }
}


/**
 Get Screen Scale Factor using NSScreen.backingScaleFactor

 @return scale
 */
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
