using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.widgets {
    public class WidgetsApp : StatefulWidget {
        public readonly Window window;

        public readonly Widget home;

        public WidgetsApp(
            Key key = null,
            Window window = null,
            Widget home = null
        ) : base(key) {
            D.assert(window != null);
            this.window = window;
            this.home = home;
        }

        public override State createState() {
            return new _WidgetsAppState();
        }
    }

    public class WindowProvider : InheritedWidget {
        public WindowProvider(Key key = null, Window window = null, Widget child = null) :
            base(key: key, child: child) {
            D.assert(window != null);
            this.window = window;
        }

        public readonly Window window;

        public static Window of(BuildContext context) {
            WindowProvider provider = (WindowProvider) context.inheritFromWidgetOfExactType(typeof(WindowProvider));
            if (provider == null) {
                throw new UIWidgetsError("WindowProvider is missing");
            }

            return provider.window;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            D.assert(this.window == ((WindowProvider) oldWidget).window);
            return false;
        }
    }

    class _WidgetsAppState : State<WidgetsApp>, WidgetsBindingObserver {
        public override void initState() {
            base.initState();
            D.assert(() => {
                WidgetInspectorService.instance.inspectorShowCallback += inspectorShowChanged;
                return true;
            });
            
            WidgetsBinding.instance.addObserver(this);
        }

        public override void dispose() {
            WidgetsBinding.instance.removeObserver(this);
            
            D.assert(() => {
                WidgetInspectorService.instance.inspectorShowCallback -= inspectorShowChanged;
                return true;
            });
            base.dispose();
        }

        private void inspectorShowChanged() {
            this.setState();
        }

        public void didChangeMetrics() {
            this.setState();
        }

        public void didChangeTextScaleFactor() {
            this.setState();
        }

        public void didChangeLocales(List<Locale> locale) {
            // TODO: support locales.
        }

        public override Widget build(BuildContext context) {
            Widget result = this.widget.home;

            D.assert(() => {
                if (WidgetInspectorService.instance.debugShowInspector) {
                    result = new WidgetInspector(null, result, this._InspectorSelectButtonBuilder);
                }

                return true;
            });

            result = new WindowProvider(
                window: this.widget.window,
                child: result
            );

            result = new MediaQuery(
                data: MediaQueryData.fromWindow(this.widget.window),
                child: result
            );

            return result;
        }

        private Widget _InspectorSelectButtonBuilder(BuildContext context, VoidCallback onPressed) {
            return new _InspectorSelectButton(onPressed: onPressed);
        }
    }

    class _InspectorSelectButton : StatelessWidget {
        public _InspectorSelectButton(
            VoidCallback onPressed,
            Key key = null
        ) : base(key: key) {
            this.onPressed = () => onPressed();
        }

        public readonly GestureTapCallback onPressed;

        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onTap: this.onPressed,
                child: new Container(
                    color: Color.fromARGB(255, 0, 0, 255),
                    padding: EdgeInsets.all(10),
                    child: new Text("Select", style: new painting.TextStyle(color: Color.fromARGB(255, 255, 255, 255)))
                )
            );
        }
    }
}