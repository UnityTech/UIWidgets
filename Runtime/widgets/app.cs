using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.widgets
{
    public class WidgetsApp : StatefulWidget
    {

        public readonly Widget child;

        public readonly Window window;
        
        public WidgetsApp(Key key, Widget child, Window window) : base(key)
        {
            D.assert(window != null);
            this.child = child;
            this.window = window;
        }
       

        public override State createState()
        {
            return new _WidgetsAppState();
        }
    }

    public class WindowProvider : InheritedWidget {
        readonly Window _window;

         public WindowProvider(Window window, Widget child, Key key = null) : base(key: key, child: child) {
            D.assert(window != null);
            _window = window;
        }

        public static Window of(BuildContext context) {
            WindowProvider provider = context.inheritFromWidgetOfExactType(typeof(WindowProvider)) as WindowProvider;
            if (provider == null) {
                throw new UIWidgetsError("WindowProvider is missing");
            }
            return provider._window;
        }
        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            D.assert(_window == ((WindowProvider)oldWidget)._window);
            return false;
        }
    }

    class _WidgetsAppState : State<WidgetsApp>
    {
        
        public override void initState() {
            base.initState();
            D.assert(() =>
            {
                WidgetInspectorService.instance.inspectorShowCallback += inspectorShowChanged;
                return true;
            });
        }

        public override void dispose() {
            
            D.assert(() =>
            {
                WidgetInspectorService.instance.inspectorShowCallback -= inspectorShowChanged;
                return true;
            });
            base.dispose();
        }

        private void inspectorShowChanged()
        {
            setState(() => {});
        }
        
        public override Widget build(BuildContext context)
        {
            Widget result = widget.child;
            result = new WindowProvider(widget.window, result);
            D.assert(() =>
            {
                if (WidgetInspectorService.instance.debugShowInspector)
                {
                    result = new WidgetInspector(null, result, _InspectorSelectButtonBuilder);
                }
                return true;
            });
            return result;
        }

        private Widget _InspectorSelectButtonBuilder(BuildContext context, VoidCallback onPressed)
        {
            return new _InspectorSelectButton(onPressed: onPressed);
        }
    }
    
    class _InspectorSelectButton : StatelessWidget {
        public _InspectorSelectButton(
            VoidCallback onPressed,
            Key key = null
        ) : base(key: key)
        {
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