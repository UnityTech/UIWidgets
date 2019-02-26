#import <UIKit/UIKit.h>
extern "C"
{
    int IOSDeviceScaleFactor()
    {
        return [[UIScreen mainScreen] scale];
    }
}
