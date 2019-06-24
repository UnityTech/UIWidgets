//
//  UIWidgetsDevice.h
//  Unity-iPhone
//
//  Created by Xingwei Zhu on 2019/6/24.
//

#ifndef UIWidgetsDevice_h
#define UIWidgetsDevice_h

@interface UIWidgetsDevice : NSObject

+(NSString *) deviceName;

+(BOOL) NeedScreenDownSample;

@end

#endif /* UIWidgetsDevice_h */
