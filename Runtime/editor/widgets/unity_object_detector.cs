using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unity.UIWidgets.editor {
    public delegate void DragFromEditorEnterCallback();

    public delegate void DragFromEditorHoverCallback();

    public delegate void DragFromEditorExitCallback();

    public delegate void DragFromEditorReleaseCallback(DragFromEditorDetails details);

    public class DragFromEditorDetails {
        public DragFromEditorDetails(Object[] objectReferences) {
            this.objectReferences = objectReferences;
        }

        public readonly Object[] objectReferences;
    }

    public class UnityObjectDetector : StatefulWidget {
        public UnityObjectDetector(
            Key key = null,
            Widget child = null,
            DragFromEditorEnterCallback onEnter = null,
            DragFromEditorHoverCallback onHover = null,
            DragFromEditorExitCallback onExit = null,
            DragFromEditorReleaseCallback onRelease = null,
            HitTestBehavior? behavior = null
        ) : base(key: key) {
            this.child = child;
            this.onDragFromEditorEnter = onEnter;
            this.onDragFromEditorHover = onHover;
            this.onDragFromEditorExit = onExit;
            this.onDragFromEditorRelease = onRelease;
            this.behavior = behavior;
        }

        public readonly Widget child;

        public readonly DragFromEditorEnterCallback onDragFromEditorEnter;
        public readonly DragFromEditorHoverCallback onDragFromEditorHover;
        public readonly DragFromEditorExitCallback onDragFromEditorExit;
        public readonly DragFromEditorReleaseCallback onDragFromEditorRelease;

        public readonly HitTestBehavior? behavior;

        public override State createState() {
            return new UnityObjectDetectorState();
        }
    }

    public class UnityObjectDetectorState : State<UnityObjectDetector> {
        HitTestBehavior _defaultBehavior {
            get { return this.widget.child == null ? HitTestBehavior.translucent : HitTestBehavior.deferToChild; }
        }

        public override Widget build(BuildContext context) {
            Widget result = new Listener(
                child: this.widget.child,
                onPointerDragFromEditorEnter: this.widget.onDragFromEditorEnter == null
                    ? ((PointerDragFromEditorEnterEventListener) null)
                    : (evt) => { this.widget.onDragFromEditorEnter.Invoke(); },
                onPointerDragFromEditorHover: this.widget.onDragFromEditorHover == null
                    ? ((PointerDragFromEditorHoverEventListener) null)
                    : (evt) => { this.widget.onDragFromEditorHover.Invoke(); },
                onPointerDragFromEditorExit: this.widget.onDragFromEditorExit == null
                    ? ((PointerDragFromEditorExitEventListener) null)
                    : (evt) => { this.widget.onDragFromEditorExit.Invoke(); },
                onPointerDragFromEditorRelease: this.widget.onDragFromEditorRelease == null
                    ? ((PointerDragFromEditorReleaseEventListener) null)
                    : (evt) => {
                        this.widget.onDragFromEditorRelease.Invoke(new DragFromEditorDetails(evt.objectReferences));
                    },
                behavior: this.widget.behavior ?? this._defaultBehavior
            );
            return result;
        }
    }
}