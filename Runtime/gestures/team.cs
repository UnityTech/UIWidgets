using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.gestures {
    class _CombiningGestureArenaEntry : GestureArenaEntry {
        public _CombiningGestureArenaEntry(_CombiningGestureArenaMember _combiner, GestureArenaMember _member) {
            this._combiner = _combiner;
            this._member = _member;
        }

        readonly _CombiningGestureArenaMember _combiner;
        readonly GestureArenaMember _member;

        public override void resolve(GestureDisposition disposition) {
            this._combiner._resolve(this._member, disposition);
        }
    }

    class _CombiningGestureArenaMember : GestureArenaMember {
        public _CombiningGestureArenaMember(GestureArenaTeam _owner, int _pointer) {
            this._owner = _owner;
            this._pointer = _pointer;
        }

        readonly GestureArenaTeam _owner;
        readonly List<GestureArenaMember> _members = new List<GestureArenaMember>();
        readonly int _pointer;

        bool _resolved = false;
        GestureArenaMember _winner;
        GestureArenaEntry _entry;

        public void acceptGesture(int pointer) {
            D.assert(this._pointer == pointer);
            D.assert(this._winner != null || this._members.isNotEmpty());

            this._close();
            this._winner = this._winner ?? this._owner.captain ?? this._members[0];

            foreach (GestureArenaMember member in this._members) {
                if (member != this._winner) {
                    member.rejectGesture(pointer);
                }
            }

            this._winner.acceptGesture(pointer);
        }

        public void rejectGesture(int pointer) {
            D.assert(this._pointer == pointer);

            this._close();
            foreach (GestureArenaMember member in this._members) {
                member.rejectGesture(pointer);
            }
        }

        void _close() {
            D.assert(!this._resolved);
            this._resolved = true;

            var combiner = this._owner._combiners[this._pointer];
            D.assert(combiner == this);

            this._owner._combiners.Remove(this._pointer);
        }

        internal GestureArenaEntry _add(int pointer, GestureArenaMember member) {
            D.assert(!this._resolved);
            D.assert(this._pointer == pointer);

            this._members.Add(member);
            this._entry = this._entry ?? GestureBinding.instance.gestureArena.add(pointer, this);
            return new _CombiningGestureArenaEntry(this, member);
        }

        internal void _resolve(GestureArenaMember member, GestureDisposition disposition) {
            if (this._resolved) {
                return;
            }

            if (disposition == GestureDisposition.rejected) {
                this._members.Remove(member);
                member.rejectGesture(this._pointer);
                if (this._members.isEmpty()) {
                    this._entry.resolve(disposition);
                }
            }
            else {
                D.assert(disposition == GestureDisposition.accepted);
                this._winner = this._winner ?? this._owner.captain ?? member;
                this._entry.resolve(disposition);
            }
        }
    }

    public class GestureArenaTeam {
        internal readonly Dictionary<int, _CombiningGestureArenaMember> _combiners =
            new Dictionary<int, _CombiningGestureArenaMember>();

        public GestureArenaMember captain;

        public GestureArenaEntry add(int pointer, GestureArenaMember member) {
            _CombiningGestureArenaMember combiner;

            if (!this._combiners.TryGetValue(pointer, out combiner)) {
                combiner = new _CombiningGestureArenaMember(this, pointer);
                this._combiners[pointer] = combiner;
            }

            return combiner._add(pointer, member);
        }
    }
}