//
//  UIWidgetsDevice.m
//  Unity-iPhone
//
//  Created by Xingwei Zhu on 2019/6/24.
//
#include "UIWidgetsDevice.h"
#import <sys/utsname.h>

static NSString* _deviceName = nil;

@implementation UIWidgetsDevice

+ (NSString *) deviceName
{
    if (_deviceName != nil) {
        return _deviceName;
    }
    
    struct utsname systemInfo;
    uname(&systemInfo);
    NSString* code = [NSString stringWithCString:systemInfo.machine encoding:NSUTF8StringEncoding];
    
    NSDictionary* deviceNamesByCode = @{
                                        @"i386"      : @"Simulator",
                                        @"x86_64"    : @"Simulator",
                                        @"iPod1,1"   : @"iPod Touch",        // (Original)
                                        @"iPod2,1"   : @"iPod Touch",        // (Second Generation)
                                        @"iPod3,1"   : @"iPod Touch",        // (Third Generation)
                                        @"iPod4,1"   : @"iPod Touch",        // (Fourth Generation)
                                        @"iPod7,1"   : @"iPod Touch",        // (6th Generation)
                                        @"iPhone1,1" : @"iPhone",            // (Original)
                                        @"iPhone1,2" : @"iPhone",            // (3G)
                                        @"iPhone2,1" : @"iPhone",            // (3GS)
                                        @"iPad1,1"   : @"iPad",              // (Original)
                                        @"iPad2,1"   : @"iPad 2",            //
                                        @"iPad3,1"   : @"iPad",              // (3rd Generation)
                                        @"iPhone3,1" : @"iPhone 4",          // (GSM)
                                        @"iPhone3,3" : @"iPhone 4",          // (CDMA/Verizon/Sprint)
                                        @"iPhone4,1" : @"iPhone 4S",         //
                                        @"iPhone5,1" : @"iPhone 5",          // (model A1428, AT&T/Canada)
                                        @"iPhone5,2" : @"iPhone 5",          // (model A1429, everything else)
                                        @"iPad3,4"   : @"iPad",              // (4th Generation)
                                        @"iPad2,5"   : @"iPad Mini",         // (Original)
                                        @"iPhone5,3" : @"iPhone 5C",         // (model A1456, A1532 | GSM)
                                        @"iPhone5,4" : @"iPhone 5C",         // (model A1507, A1516, A1526 (China), A1529 | Global)
                                        @"iPhone6,1" : @"iPhone 5S",         // (model A1433, A1533 | GSM)
                                        @"iPhone6,2" : @"iPhone 5S",         // (model A1457, A1518, A1528 (China), A1530 | Global)
                                        @"iPhone7,1" : @"iPhone 6 Plus",     //
                                        @"iPhone7,2" : @"iPhone 6",          //
                                        @"iPhone8,1" : @"iPhone 6S",         //
                                        @"iPhone8,2" : @"iPhone 6S Plus",    //
                                        @"iPhone8,4" : @"iPhone SE",         //
                                        @"iPhone9,1" : @"iPhone 7",          //
                                        @"iPhone9,3" : @"iPhone 7",          //
                                        @"iPhone9,2" : @"iPhone 7 Plus",     //
                                        @"iPhone9,4" : @"iPhone 7 Plus",     //
                                        @"iPhone10,1": @"iPhone 8",          // CDMA
                                        @"iPhone10,4": @"iPhone 8",          // GSM
                                        @"iPhone10,2": @"iPhone 8 Plus",     // CDMA
                                        @"iPhone10,5": @"iPhone 8 Plus",     // GSM
                                        @"iPhone10,3": @"iPhone X",          // CDMA
                                        @"iPhone10,6": @"iPhone X",          // GSM
                                        @"iPhone11,2": @"iPhone XS",         //
                                        @"iPhone11,4": @"iPhone XS Max",     //
                                        @"iPhone11,6": @"iPhone XS Max",     // China
                                        @"iPhone11,8": @"iPhone XR",         //
                                        
                                        @"iPad4,1"   : @"iPad Air",          // 5th Generation iPad (iPad Air) - Wifi
                                        @"iPad4,2"   : @"iPad Air",          // 5th Generation iPad (iPad Air) - Cellular
                                        @"iPad4,4"   : @"iPad Mini",         // (2nd Generation iPad Mini - Wifi)
                                        @"iPad4,5"   : @"iPad Mini",         // (2nd Generation iPad Mini - Cellular)
                                        @"iPad4,7"   : @"iPad Mini",         // (3rd Generation iPad Mini - Wifi (model A1599))
                                        @"iPad6,7"   : @"iPad Pro (12.9\")", // iPad Pro 12.9 inches - (model A1584)
                                        @"iPad6,8"   : @"iPad Pro (12.9\")", // iPad Pro 12.9 inches - (model A1652)
                                        @"iPad6,3"   : @"iPad Pro (9.7\")",  // iPad Pro 9.7 inches - (model A1673)
                                        @"iPad6,4"   : @"iPad Pro (9.7\")"
                                        };
    _deviceName = [deviceNamesByCode objectForKey:code];
    
    if (!_deviceName) {
        if ([code rangeOfString:@"iPod"].location != NSNotFound) {
            _deviceName = @"iPod Touch";
        }
        else if([code rangeOfString:@"iPad"].location != NSNotFound) {
            _deviceName = @"iPad";
        }
        else if([code rangeOfString:@"iPhone"].location != NSNotFound){
            _deviceName = @"iPhone";
        }
        else {
            _deviceName = @"Unknown";
        }
    }
    
    if ([_deviceName isEqualToString:@"Simulator"]) {
        if([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPhone) {
            switch ((int)[[UIScreen mainScreen] nativeBounds].size.height) {
                    //iPhone 5 or 5S or 5C
                case 1136:
                    _deviceName = @"iPhone 5 Simulator";
                    break;
                    //iPhone 6 or 6S or 7 or 8
                case 1334:
                    _deviceName = @"iPhone 6 Simulator";
                    break;
                    //iPhone 6+ or 6S+ or 7+ or 8+
                case 1920:
                    _deviceName = @"iPhone 6 Plus Simulator";
                    break;
                    //iPhone 6+ or 6S+ or 7+ or 8+
                case 2208:
                    _deviceName = @"iPhone 6 Plus Simulator";
                    break;
                    //iPhone X or XS
                case 2436:
                    _deviceName = @"iPhone X Simulator";
                    break;
                    //iPhone XS Max
                case 2688:
                    _deviceName = @"iPhone XS Max Simulator";
                    break;
                    //iPhone XR
                case 1792:
                    _deviceName = @"iPhone XR Simulator";
                    break;
                default:
                    _deviceName = @"Unknown Simulator";
                    break;
            }
        }
    }
    
    return _deviceName;
}

+ (BOOL) NeedScreenDownSample
{
    return  [[UIWidgetsDevice deviceName] isEqualToString:@"iPhone 6 Plus"] ||
            [[UIWidgetsDevice deviceName] isEqualToString:@"iPhone 6S Plus"] ||
            [[UIWidgetsDevice deviceName] isEqualToString:@"iPhone 7 Plus"] ||
            [[UIWidgetsDevice deviceName] isEqualToString:@"iPhone 8 Plus"];
}

@end
