using System;
using System.Linq;
using System.Reflection;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgets.Tests {
    public class SceneViewTests {
        public static void show() {
            onPreSceneGUIDelegate += OnPreSceneGUI;
#pragma warning disable 0618
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#pragma warning restore 0618
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
            onPreSceneGUIDelegate -= OnPreSceneGUI;
#pragma warning disable 0618
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#pragma warning restore 0618
            EditorApplication.update -= Update;
            SceneView.RepaintAll();
        }

#pragma warning disable 0618
        public static SceneView.OnSceneFunc onPreSceneGUIDelegate {
            get {
                var field = typeof(SceneView).GetField("onPreSceneGUIDelegate",
                    BindingFlags.Static | BindingFlags.NonPublic);

                return (SceneView.OnSceneFunc) field.GetValue(null);
            }

            set {
                var field = typeof(SceneView).GetField("onPreSceneGUIDelegate",
                    BindingFlags.Static | BindingFlags.NonPublic);

                field.SetValue(null, value);
            }
        }
#pragma warning restore 0618

        static Func<Widget>[] _options;

        static string[] _optionStrings;

        static int _selected;

        [NonSerialized] static bool hasInvoked = false;

        static EventType _lastEventType;

        static void OnPreSceneGUI(SceneView sceneView) {
            _lastEventType = Event.current.rawType;
        }

        static void OnSceneGUI(SceneView sceneView) {
            //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Handles.BeginGUI();

            if (windowAdapter == null) {
                windowAdapter = new EditorWindowAdapter(sceneView);
            }
            else if (windowAdapter != null && windowAdapter.editorWindow != sceneView) {
                windowAdapter = new EditorWindowAdapter(sceneView);
            }

            var selected = EditorGUILayout.Popup("test case", _selected, _optionStrings);
            if (selected != _selected || !hasInvoked) {
                _selected = selected;
                hasInvoked = true;

                var widget = _options[_selected]();
                windowAdapter.attachRootWidget(widget);
            }

            if (Event.current.type == EventType.Used) {
                Event.current.type = _lastEventType;
                windowAdapter.OnGUI();
                Event.current.type = EventType.Used;
            }
            else {
                windowAdapter.OnGUI();
            }

            Handles.EndGUI();
        }

        static void Update() {
            if (windowAdapter != null) {
                windowAdapter.Update();
            }
        }

        static EditorWindowAdapter windowAdapter;

        public static Widget none() {
            return null;
        }

        public static Widget listView() {
            return ListView.builder(
                itemExtent: 20.0f,
                itemBuilder: (context, index) => {
                    return new Container(
                        color: Color.fromARGB(255, (index * 10) % 256, (index * 10) % 256, (index * 10) % 256)
                    );
                }
            );
        }

        public static Widget eventsPage() {
            return new EventsWaterfallScreen();
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