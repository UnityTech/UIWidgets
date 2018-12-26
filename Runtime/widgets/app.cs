using System;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;
using Color = UIWidgets.ui.Color;

namespace UIWidgets.widgets
{
    public class WidgetsApp : StatefulWidget
    {

        public readonly Widget child;
        
        public WidgetsApp(Key key, Widget child) : base(key)
        {
            this.child = child;
        }
       

        public override State createState()
        {
            return new _WidgetsAppState();
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