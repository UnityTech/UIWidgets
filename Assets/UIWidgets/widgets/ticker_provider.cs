using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UIWidgets.rendering;
using UIWidgets.ui;
using UnityEngine.Assertions;

namespace UIWidgets.widgets {
    public class TickerMode : InheritedWidget {
        public TickerMode(Key key, bool enabled, Widget child) : base(key, child) {
            this.enabled = enabled;
        }

        public readonly bool enabled;

        public static bool of(BuildContext context) {
            var widget = context.inheritFromWidgetOfExactType(typeof(TickerMode));
            return widget is TickerMode ? (widget as TickerMode).enabled : true;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return this.enabled != ((TickerMode)oldWidget).enabled;
        }
        
        public override Element createElement() {
            throw new NotImplementedException();
        }
    }
}