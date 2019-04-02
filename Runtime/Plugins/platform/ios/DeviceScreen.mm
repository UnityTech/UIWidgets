#import <UIKit/UIKit.h>
extern "C"
{
    int IOSDeviceScaleFactor()
    {
        return [[UIScreen mainScreen] scale];
    }


    struct viewPadding
    {
        float top;
        float bottom;
        float left;
        float right;
    };

    viewPadding IOSGetViewportPadding()
    {
        viewPadding _viewPadding;
        CGFloat scale = [[UIScreen mainScreen] scale];
        if (@available(iOS 11, *)) {
            _viewPadding.bottom = [UIApplication sharedApplication].keyWindow.safeAreaInsets.bottom * scale;
            _viewPadding.top = [UIApplication sharedApplication].keyWindow.safeAreaInsets.top * scale;
            _viewPadding.left = [UIApplication sharedApplication].keyWindow.safeAreaInsets.left * scale;
            _viewPadding.right = [UIApplication sharedApplication].keyWindow.safeAreaInsets.right * scale;
        } else {
            CGRect statusFrame = [UIApplication sharedApplication].statusBarFrame;
            _viewPadding.bottom = 0;
            _viewPadding.top = statusFrame.size.height * scale;
            _viewPadding.left = 0;
            _viewPadding.right = 0;
        }

        return _viewPadding;
    }
}
