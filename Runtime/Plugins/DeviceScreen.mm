#import <UIKit/UIKit.h>
extern "C"
{
    int IOSDeviceSaleFactor()
    {
        return [[UIScreen mainScreen] scale];
    }
}
