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
    void LinkUnityOSXCallback(UnityOSXCallback callback);
    void UnityOSXSendMessage(const char *name,const char *method,const char *arg);
#ifdef __cplusplus
}
#endif


#endif /* NSScreenUtils_h */
