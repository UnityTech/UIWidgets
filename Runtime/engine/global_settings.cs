using System;
using Unity.UIWidgets.async;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    public class UIWidgetsGlobalSettings {
        UIWidgetsGlobalSettings() {
            this.speedUpFrameRate = () => { this.defaultSpeedUp(); };
            this.coolDownFrameRate = () => { this.defaultCoolDown(); };
        }

        public Action speedUpFrameRate;
        public Action coolDownFrameRate;

        public static UIWidgetsGlobalSettings instance {
            get { return _instance; }
        }
        
        static UIWidgetsGlobalSettings _instance = new UIWidgetsGlobalSettings();
        
        Timer scheduleFrameTimer;
        public const int defaultMaxTargetFrameRate = 60;
        public const int defaultMinTargetFrameRate = 15;
        
        void defaultSpeedUp() {
            Application.targetFrameRate = defaultMaxTargetFrameRate;
        }

        void defaultCoolDown() {
            this.scheduleFrameTimer?.cancel();
            this.scheduleFrameTimer = Window.instance.run(
                new TimeSpan(0, 0, 0, 0, 200),
                () => {
                    Application.targetFrameRate = defaultMinTargetFrameRate;
                    this.scheduleFrameTimer = null;
                });
        }
    }
}