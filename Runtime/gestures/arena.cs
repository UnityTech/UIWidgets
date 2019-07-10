using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public enum GestureDisposition {
        accepted,
        rejected,
    }

    public interface GestureArenaMember {
        void acceptGesture(int pointer);
        void rejectGesture(int pointer);
    }

    public class GestureArenaEntry {
        public GestureArenaEntry(
            GestureArenaManager arena = null,
            int pointer = 0,
            GestureArenaMember member = null) {
            this._arena = arena;
            this._pointer = pointer;
            this._member = member;
        }

        readonly GestureArenaManager _arena;
        readonly int _pointer;
        readonly GestureArenaMember _member;

        public virtual void resolve(GestureDisposition disposition) {
            this._arena._resolve(this._pointer, this._member, disposition);
        }
    }

    class _GestureArena {
        public readonly List<GestureArenaMember> members = new List<GestureArenaMember>();
        public bool isOpen = true;
        public bool isHeld = false;
        public bool hasPendingSweep = false;

        public GestureArenaMember eagerWinner;

        public void add(GestureArenaMember member) {
            D.assert(this.isOpen);
            this.members.Add(member);
        }

        public override string ToString() {
            StringBuilder buffer = new StringBuilder();
            if (this.members.isEmpty()) {
                buffer.Append("<empty>");
            }
            else {
                buffer.Append(string.Join(", ", this.members.Select(
                    member => member == this.eagerWinner
                        ? $"{member} (eager winner)"
                        : member.ToString()).ToArray()));
            }

            if (this.isOpen) {
                buffer.Append(" [open]");
            }

            if (this.isHeld) {
                buffer.Append(" [held]");
            }

            if (this.hasPendingSweep) {
                buffer.Append(" [hasPendingSweep]");
            }

            return buffer.ToString();
        }
    }

    public class GestureArenaManager {
        readonly Dictionary<int, _GestureArena> _arenas = new Dictionary<int, _GestureArena>();

        public GestureArenaEntry add(int pointer, GestureArenaMember member) {
            _GestureArena state = this._arenas.putIfAbsent(pointer, () => {
                D.assert(this._debugLogDiagnostic(pointer, () => "★ Opening new gesture arena."));
                return state = new _GestureArena();
            });

            state.add(member);

            D.assert(this._debugLogDiagnostic(pointer, () => $"Adding: {member}"));
            return new GestureArenaEntry(this, pointer, member);
        }

        public void close(int pointer) {
            _GestureArena state;
            if (!this._arenas.TryGetValue(pointer, out state)) {
                return;
            }

            state.isOpen = false;
            D.assert(this._debugLogDiagnostic(pointer, () => "Closing", state));
            this._tryToResolveArena(pointer, state);
        }

        public void sweep(int pointer) {
            _GestureArena state;
            if (!this._arenas.TryGetValue(pointer, out state)) {
                return;
            }

            D.assert(!state.isOpen);
            if (state.isHeld) {
                state.hasPendingSweep = true;
                D.assert(this._debugLogDiagnostic(pointer, () => "Delaying sweep", state));
                return;
            }

            D.assert(this._debugLogDiagnostic(pointer, () => "Sweeping", state));
            this._arenas.Remove(pointer);
            if (state.members.isNotEmpty()) {
                D.assert(this._debugLogDiagnostic(
                    pointer, () => $"Winner: {state.members.First()}"));

                state.members[0].acceptGesture(pointer);
                for (int i = 1; i < state.members.Count; i++) {
                    state.members[i].rejectGesture(pointer);
                }
            }
        }

        public void hold(int pointer) {
            _GestureArena state;
            if (!this._arenas.TryGetValue(pointer, out state)) {
                return;
            }

            state.isHeld = true;
            D.assert(this._debugLogDiagnostic(pointer, () => "Holding", state));
        }

        public void release(int pointer) {
            _GestureArena state;
            if (!this._arenas.TryGetValue(pointer, out state)) {
                return;
            }

            state.isHeld = false;
            D.assert(this._debugLogDiagnostic(pointer, () => "Releasing", state));
            if (state.hasPendingSweep) {
                this.sweep(pointer);
            }
        }

        internal void _resolve(int pointer, GestureArenaMember member, GestureDisposition disposition) {
            _GestureArena state;
            if (!this._arenas.TryGetValue(pointer, out state)) {
                return;
            }

            D.assert(this._debugLogDiagnostic(pointer,
                () => $"{(disposition == GestureDisposition.accepted ? "Accepting" : "Rejecting")}: {member}"));

            D.assert(state.members.Contains(member));
            if (disposition == GestureDisposition.rejected) {
                state.members.Remove(member);
                member.rejectGesture(pointer);
                if (!state.isOpen) {
                    this._tryToResolveArena(pointer, state);
                }
            }
            else {
                if (state.isOpen) {
                    state.eagerWinner = state.eagerWinner ?? member;
                }
                else {
                    D.assert(this._debugLogDiagnostic(pointer,
                        () => $"Self-declared winner: {member}"));
                    this._resolveInFavorOf(pointer, state, member);
                }
            }
        }

        void _tryToResolveArena(int pointer, _GestureArena state) {
            D.assert(this._arenas[pointer] == state);
            D.assert(!state.isOpen);

            if (state.members.Count == 1) {
                Window.instance.scheduleMicrotask(() => this._resolveByDefault(pointer, state));
            }
            else if (state.members.isEmpty()) {
                this._arenas.Remove(pointer);
                D.assert(this._debugLogDiagnostic(pointer, () => "Arena empty."));
            }
            else if (state.eagerWinner != null) {
                D.assert(this._debugLogDiagnostic(pointer,
                    () => $"Eager winner: {state.eagerWinner}"));
                this._resolveInFavorOf(pointer, state, state.eagerWinner);
            }
        }

        void _resolveByDefault(int pointer, _GestureArena state) {
            if (!this._arenas.ContainsKey(pointer)) {
                return;
            }

            D.assert(this._arenas[pointer] == state);
            D.assert(!state.isOpen);
            D.assert(state.members.Count == 1);
            this._arenas.Remove(pointer);
            D.assert(this._debugLogDiagnostic(pointer,
                () => $"Default winner: {state.members.First()}"));
            state.members[0].acceptGesture(pointer);
        }

        void _resolveInFavorOf(int pointer, _GestureArena state, GestureArenaMember member) {
            D.assert(state == this._arenas[pointer]);
            D.assert(state != null);
            D.assert(state.eagerWinner == null || state.eagerWinner == member);
            D.assert(!state.isOpen);

            this._arenas.Remove(pointer);
            foreach (GestureArenaMember rejectedMember in state.members) {
                if (rejectedMember != member) {
                    rejectedMember.rejectGesture(pointer);
                }
            }

            member.acceptGesture(pointer);
        }

        bool _debugLogDiagnostic(int pointer, Func<string> message, _GestureArena state = null) {
            D.assert(() => {
                if (D.debugPrintGestureArenaDiagnostics) {
                    int? count = state?.members.Count;
                    string s = count != 1 ? "s" : "";
                    Debug.LogFormat("Gesture arena {0} ❙ {1}{2}",
                        pointer.ToString().PadRight(4),
                        message(),
                        count != null ? $" with {count} member{s}." : "");
                }

                return true;
            });

            return true;
        }
    }
}