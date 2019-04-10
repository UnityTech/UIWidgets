using System;
using Unity.UIWidgets.async;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    public class UIWidgetsGlobalSettings {
        private UIWidgetsGlobalSettings() {}
        
        static UIWidgetsGlobalSettings _instance = new UIWidgetsGlobalSettings();
        Timer scheduleFrameTimer;
        public const int defaultMaxTargetFrameRate = 60;
        public const int defaultMinTargetFrameRate = 15;


        public static UIWidgetsGlobalSettings instance {
            get { return _instance; }
        }

        public virtual void speedUpFrameRate() {
            Application.targetFrameRate = defaultMaxTargetFrameRate;
        }

        public virtual void coolDownFrameRate() {
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