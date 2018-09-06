using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UIWidgets.ui;

namespace UIWidgets.gestures {
    public interface HitTestable {
        void hitTest(HitTestResult result, Offset position);
    }

    public interface HitTestDispatcher {
        void dispatchEvent(PointerEvent @event, HitTestResult result);
    }

    public interface HitTestTarget {
        void handleEvent(PointerEvent @event, HitTestEntry entry);
    }

    public class HitTestEntry {
        public HitTestEntry(HitTestTarget target) {
            this.target = target;
        }

        public readonly HitTestTarget target;

        public override string ToString() {
            return this.target.ToString();
        }
    }

    public class HitTestResult {
        public HitTestResult(List<HitTestEntry> path = null) {
            this._path = path ?? new List<HitTestEntry>();
        }

        public IList<HitTestEntry> path {
            get { return this._path.AsReadOnly(); }
        }

        readonly List<HitTestEntry> _path;

        public void add(HitTestEntry entry) {
            this._path.Add(entry);
        }

        public override string ToString() {
            return string.Format("HitTestResult({0})",
                this._path.isEmpty()
                    ? "<empty path>"
                    : string.Join(", ", this._path.Select(x => x.ToString()).ToArray()));
        }
    }
}