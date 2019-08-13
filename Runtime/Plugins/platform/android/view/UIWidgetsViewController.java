package com.unity.uiwidgets.plugin;

import android.util.Log;

import com.unity3d.player.UnityPlayer;
import android.graphics.Rect;
import android.os.Build;
import android.util.Log;
import android.util.TypedValue;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewTreeObserver;
import android.app.Activity;
import android.content.res.Configuration;
import android.content.res.Resources;
import android.view.Surface;
import java.util.Arrays;
import android.view.WindowManager;
import android.os.Handler;
import java.lang.reflect.Method;

public class UIWidgetsViewController {

    private static UIWidgetsViewController _instance;

    public static UIWidgetsViewController getInstance() {
        if (_instance == null) {
            _instance = new UIWidgetsViewController();
            _instance.setup();
        }
        return _instance;
    }
    
    private UIWidgetsViewMetrics viewMetrics;
    private boolean keyboardOpen;
    
    private float statusHeight;
    private float navigationBarHeight;
    
    private void setup() {
        //Log.i("tag", "On Setup 2");
        
        keyboardOpen = false;
        viewMetrics = new UIWidgetsViewMetrics();
        setupHeights();
        updateViewMetrics();
        
        setupViewMetricsChangedListener();
    }
    
    private void setupHeights() {
        final View unityView = ((ViewGroup)UnityPlayer.currentActivity.findViewById(android.R.id.content)).getChildAt(0);
        Rect rect = new Rect();
        unityView.getWindowVisibleDisplayFrame(rect);
        
        statusHeight = rect.top;
        navigationBarHeight = unityView.getRootView().getHeight() - rect.bottom;
    }
    
    public static UIWidgetsViewMetrics getMetrics() {
        UIWidgetsViewController controller = getInstance();
        return controller.viewMetrics;
    }
    
    enum ZeroSides { NONE, LEFT, RIGHT, BOTH }
    ZeroSides calculateShouldZeroSides(View unityView) {
        Activity activity = UnityPlayer.currentActivity;
        int orientation = activity.getResources().getConfiguration().orientation;
        int rotation = activity.getWindowManager().getDefaultDisplay().getRotation();
        
        if (orientation == Configuration.ORIENTATION_LANDSCAPE) {
            if (rotation == Surface.ROTATION_90) {
                return ZeroSides.RIGHT;
            }
            else if (rotation == Surface.ROTATION_270) {
                return Build.VERSION.SDK_INT >= 23 ? ZeroSides.LEFT : ZeroSides.RIGHT;
            } else if (rotation == Surface.ROTATION_0 || rotation == Surface.ROTATION_180) {
                return ZeroSides.BOTH;
            }
        }
        
        return ZeroSides.NONE;
    }
    
    public boolean lastStatusBarHidden = true;
    
    private int calculateBottomKeyboardInset(Rect insets) {
        if (keyboardOpen) {
            return insets.bottom;
        } else {
            return 0;
        }
    }
    
    
    private int getNavigationBarHeight() {
         Resources resources = UnityPlayer.currentActivity.getResources();
         int resourceId = resources.getIdentifier("navigation_bar_height", "dimen", "android");
         if (resourceId > 0)
         {
            return hasNavigationBar() ? resources.getDimensionPixelSize(resourceId) : 0;
         }
         
         return 0;
    }
            
    private boolean hasNavigationBar() {
        boolean hasBar = false;
        Resources resources = UnityPlayer.currentActivity.getResources();
        int id = resources.getIdentifier("config_showNavigationBar", "bool", "android");
        if (id > 0) {
            hasBar = resources.getBoolean(id);
        }
        try {
                Class systemPropertiesClass = Class.forName("android.os.SystemProperties");
                Method m = systemPropertiesClass.getMethod("get", String.class);
                String navBarOverride = (String) m.invoke(systemPropertiesClass, "qemu.hw.mainkeys");
                if ("1".equals(navBarOverride)) {
                    hasBar = false;
                } else if ("0".equals(navBarOverride)) {
                    hasBar = true;
                }
        } catch (Exception e) {
            e.printStackTrace();
        }
        //Log.i("UIWidgetsDebug", " hasBar: " + hasBar);
        return hasBar;
    }
    
    public void updateViewMetrics() {
        final View unityView = ((ViewGroup)UnityPlayer.currentActivity.findViewById(android.R.id.content)).getChildAt(0);
        Rect rect = new Rect();
        unityView.getWindowVisibleDisplayFrame(rect);
        
        //Log.i("UIWidgetsDebug", "calculation: " + unityView.getRootView().getHeight() + " " + rect.bottom + " " + rect.top);
        
        rect.bottom = unityView.getRootView().getHeight() - (rect.bottom - rect.top) - rect.top;
        rect.right = unityView.getRootView().getWidth() - (rect.right - rect.left) - rect.left;
        
        boolean statusBarHidden = (View.SYSTEM_UI_FLAG_FULLSCREEN & unityView.getWindowSystemUiVisibility()) != 0;
        boolean navigationBarHidden = (View.SYSTEM_UI_FLAG_HIDE_NAVIGATION & unityView.getWindowSystemUiVisibility()) != 0;
        
        ZeroSides zeroSides = ZeroSides.NONE;
        if (navigationBarHidden) {
            zeroSides = calculateShouldZeroSides(unityView);
        }
        
        viewMetrics.padding_top = rect.top;
        viewMetrics.padding_right = zeroSides == ZeroSides.RIGHT || zeroSides == ZeroSides.BOTH ? 0 : rect.right;
        viewMetrics.padding_bottom = 0;
        viewMetrics.padding_left = zeroSides == ZeroSides.LEFT || zeroSides == ZeroSides.BOTH ? 0 : rect.left;
        
        viewMetrics.insets_top = 0;
        viewMetrics.insets_right = 0;
        viewMetrics.insets_bottom = navigationBarHidden? calculateBottomKeyboardInset(rect) : rect.bottom;
        viewMetrics.insets_left = 0;
        
        //adjust
        viewMetrics.insets_bottom -= navigationBarHeight;
        viewMetrics.padding_top -= statusHeight;
        
        //Log.i("UIWidgetsDebug", "checks: " + navigationBarHidden + " " + rect.bottom);
        //Log.i("UIWidgetsDebug", " padding: " + viewMetrics.padding_top + " " + viewMetrics.padding_right + " " + viewMetrics.padding_bottom + " " + viewMetrics.padding_left);
        //Log.i("UIWidgetsDebug", " insets: " + viewMetrics.insets_top + " " + viewMetrics.insets_right + " " + viewMetrics.insets_bottom + " " + viewMetrics.insets_left);
    }
    
    public void setupViewMetricsChangedListener() {
        final View unityView = ((ViewGroup)UnityPlayer.currentActivity.findViewById(android.R.id.content)).getChildAt(0);

        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                unityView.setOnSystemUiVisibilityChangeListener(new View.OnSystemUiVisibilityChangeListener() {
                    @Override
                    public void onSystemUiVisibilityChange(final int visibility){
                        boolean statusBarHidden = (View.SYSTEM_UI_FLAG_FULLSCREEN & visibility) != 0;
                        unityView.setSystemUiVisibility(visibility);
                        UIWidgetsViewController controller = getInstance();
                        if (statusBarHidden != controller.lastStatusBarHidden) {
                            controller.onViewMetricsChanged();
                        }
                    }
                });
            }
        });
        
        
        unityView.getViewTreeObserver().addOnGlobalLayoutListener(new ViewTreeObserver.OnGlobalLayoutListener() {
                    private final int defaultKeyboardHeightDP = 100;
                    private final int estimatedKeyboardDP = defaultKeyboardHeightDP + (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP ? 48 : 0);
                    private final Rect rect = new Rect();
        
                    @Override
                    public void onGlobalLayout() {
                        int estimatedKeyboardHeight = (int) TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, estimatedKeyboardDP, unityView.getResources().getDisplayMetrics());
                        unityView.getWindowVisibleDisplayFrame(rect);
                        int heightDiff = unityView.getRootView().getHeight() - (rect.bottom - rect.top);
                        boolean isShown = heightDiff >= estimatedKeyboardHeight;
                        
                        if (keyboardOpen == isShown) {
                            return;
                        }
                        
                        keyboardOpen = isShown;
                        onViewMetricsChanged();
                    }
                }
           );
    }
    
    public void onViewMetricsChanged() {
        updateViewMetrics();
        UIWidgetsMessageManager.getInstance().UIWidgetsMethodMessage("ViewportMatricsChanged", "UIWidgetViewController.keyboardChanged",
                        Arrays.asList(""));
    }
}