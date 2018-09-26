using System;
using System.Linq;
using UIWidgets.editor;
using UIWidgets.painting;
using UIWidgets.rendering;
using UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Color = UIWidgets.ui.Color;

namespace UIWidgets.Tests {
    public class SceneViewTests {
        public static void show() {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            EditorApplication.update += Update;
            
            SceneView.RepaintAll();
            
            _options = new Func<Widget>[] {
                none,
                listView,
                eventsPage,
            };
            _optionStrings = _options.Select(x => x.Method.Name).ToArray();
            _selected = 0;

        }

        public static void hide() {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            EditorApplication.update -= Update;
            SceneView.RepaintAll();
        }

        private static Func<Widget>[] _options;
        
        private static string[] _optionStrings;

        private static int _selected;
        
        [NonSerialized] private static bool hasInvoked = false;

        private static void OnSceneGUI(SceneView sceneView) {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Handles.BeginGUI();
            
            if (windowAdapter == null) {
                windowAdapter = new WindowAdapter(sceneView);
            } else if (windowAdapter != null && windowAdapter.editorWindow != sceneView) {
                windowAdapter = new WindowAdapter(sceneView);
            }
            
            var selected = EditorGUILayout.Popup("test case", _selected, _optionStrings);
            if (selected != _selected || !hasInvoked) {
                _selected = selected;
                hasInvoked = true;

                var widget = _options[_selected]();
                windowAdapter.attachRootWidget(widget);
            }

            windowAdapter.OnGUI();

            Handles.EndGUI();
        }

        private static void Update() {
            if (windowAdapter != null) {
                windowAdapter.Update();
            }
        }

        private static WindowAdapter windowAdapter;

        public static Widget none() {
            return null;
        }
        
        public static Widget listView() {
            return ListView.builder(
                itemExtent: 20.0,
                itemBuilder: (context, index) => {
                    return new Container(
                        color: Color.fromARGB(255, (index * 10) % 256, (index * 10) % 256, (index * 10) % 256)
                    );
                }
            );
        }
        
        public static Widget eventsPage() { 
            return new Container(
                color: CLColors.primary,
                margin: EdgeInsets.all(50),
                child:new Text(
                    "Today",
                    style: new TextStyle(
                        fontSize: 34,
                        color: CLColors.white
                    )
                )
            );
            //return new EventsWaterfallScreen();
        }
        
        public static RenderBox flex() {
            var flexbox = new RenderFlex(
                direction: Axis.horizontal,
                crossAxisAlignment: CrossAxisAlignment.center);

            flexbox.add(new RenderConstrainedBox(
                additionalConstraints: new BoxConstraints(minWidth: 300, minHeight: 200),
                child: new RenderDecoratedBox(
                    decoration: new BoxDecoration(
                        color: new Color(0xFF00FF00)
                    )
                )));

            flexbox.add(new RenderConstrainedBox(
                additionalConstraints: new BoxConstraints(minWidth: 100, minHeight: 300),
                child: new RenderDecoratedBox(
                    decoration: new BoxDecoration(
                        color: new Color(0xFF00FFFF)
                    )
                )));

            flexbox.add(new RenderConstrainedBox(
                additionalConstraints: new BoxConstraints(minWidth: 50, minHeight: 100),
                child: new RenderDecoratedBox(
                    decoration: new BoxDecoration(
                        color: new Color(0xFF0000FF)
                    )
                )));


            return flexbox;
        }
    }
}