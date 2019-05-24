//
//  NSScreenUtils.h
//  NSScreenUtils
//
//  Created by Justin Fincher on 24/5/2019.
//  Copyright © 2019 Justin Fincher. All rights reserved.
//

#ifndef NSScreenUtils_h
#define NSScreenUtils_h

typedef void (* UnityOSXCallback)(const char *name,const char *method,const char *arg);

#ifdef __cplusplus
extern "C" {
#endif
    
    /**
     Set Custom Objective-C to C# Callback

     @param callback callback block
     */
    void LinkUnityOSXCallback(UnityOSXCallback callback);
    
    
    /**
     A UnitySendMessage clone method for OSX, if there is an offical implementation of UnitySendMessage in Unity standalone player you can replace this with the offical method.

     @param name GameObject name
     @param method Method name
     @param arg Argument
     */
    void UnityOSXSendMessage(const char *name,const char *method,const char *arg);
#ifdef __cplusplus
}
#endif


#endif /* NSScreenUtils_h */
