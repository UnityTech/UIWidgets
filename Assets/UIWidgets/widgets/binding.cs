using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UIWidgets.rendering;
using UIWidgets.ui;
using UnityEngine.Assertions;

namespace UIWidgets.widgets {
    abstract class WidgetsBinding : RendererBinding  {
        protected WidgetsBinding(Window window) : base(window) {
            this.buildOwner.onBuildScheduled = this._handleBuildScheduled;
            window.onLocaleChanged += this.handleLocaleChanged;
            window.onAccessibilityFeaturesChanged += handleAccessibilityFeaturesChanged;
        }
        
        public BuildOwner buildOwner {
            get { return this._buildOwner; }
        }

        readonly BuildOwner _buildOwner;

        public Element renderViewElement {
            get { return this._renderViewElement; }
        }
        
        Element _renderViewElement;
        
        void _handleBuildScheduled() {
            ensureVisualUpdate();
        }
        
        void handleLocaleChanged() {
            // todo
//            dispatchLocaleChanged(window.locale);
        }
        
        void handleAccessibilityFeaturesChanged() {
//            for (WidgetsBindingObserver observer in _observers) {
//                observer.didChangeAccessibilityFeatures();
//            }
        }

        protected override void drawFrame() {
            if (renderViewElement != null) {
                buildOwner.buildScope(renderViewElement);
            }
            base.drawFrame();
            buildOwner.finalizeTree();
        }
    }
}