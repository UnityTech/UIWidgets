using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;

namespace Unity.UIWidgets.widgets {
    public class TickerMode : InheritedWidget {
        public TickerMode(
            Key key = null,
            bool enabled = true,
            Widget child = null)
            : base(key, child) {
            this.enabled = enabled;
        }

        public readonly bool enabled;

        public static bool of(BuildContext context) {
            var widget = (TickerMode) context.inheritFromWidgetOfExactType(typeof(TickerMode));
            return widget != null ? widget.enabled : true;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return this.enabled != ((TickerMode) oldWidget).enabled;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("mode", value: this.enabled, ifTrue: "enabled", ifFalse: "disabled",
                showName: true));
        }
    }

    public abstract class SingleTickerProviderStateMixin<T> : State<T>, TickerProvider where T : StatefulWidget {
        Ticker _ticker;

        public Ticker createTicker(TickerCallback onTick) {
            D.assert(() => {
                if (this._ticker == null) {
                    return true;
                }

                throw new UIWidgetsError(
                    this.GetType() + " is a SingleTickerProviderStateMixin but multiple tickers were created.\n" +
                    "A SingleTickerProviderStateMixin can only be used as a TickerProvider once. If a " +
                    "State is used for multiple AnimationController objects, or if it is passed to other " +
                    "objects and those objects might use it more than one time in total, then instead of " +
                    "mixing in a SingleTickerProviderStateMixin, use a regular TickerProviderStateMixin."
                );
            });
            
            Func<string> debugLabel = null;
            D.assert(() => {
                debugLabel = () => "created by " + this;
                return true;
            });
            this._ticker = new Ticker(onTick, debugLabel: debugLabel);
            return this._ticker;
        }

        public override void dispose() {
            D.assert(() => {
                if (this._ticker == null || !this._ticker.isActive) {
                    return true;
                }

                throw new UIWidgetsError(
                    this + " was disposed with an active Ticker.\n" +
                    this.GetType() + " created a Ticker via its SingleTickerProviderStateMixin, but at the time " +
                    "dispose() was called on the mixin, that Ticker was still active. The Ticker must " +
                    "be disposed before calling base.dispose(). Tickers used by AnimationControllers " +
                    "should be disposed by calling dispose() on the AnimationController itself. " +
                    "Otherwise, the ticker will leak.\n" +
                    "The offending ticker was: " + this._ticker.toString(debugIncludeStack: true)
                );
            });
            base.dispose();
        }

        public override void didChangeDependencies() {
            if (this._ticker != null) {
                this._ticker.muted = !TickerMode.of(this.context);
            }

            base.didChangeDependencies();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            string tickerDescription = null;
            if (this._ticker != null) {
                if (this._ticker.isActive && this._ticker.muted) {
                    tickerDescription = "active but muted";
                }
                else if (this._ticker.isActive) {
                    tickerDescription = "active";
                }
                else if (this._ticker.muted) {
                    tickerDescription = "inactive and muted";
                }
                else {
                    tickerDescription = "inactive";
                }
            }

            properties.add(new DiagnosticsProperty<Ticker>("ticker", this._ticker, description: tickerDescription,
                showSeparator: false, defaultValue: Diagnostics.kNullDefaultValue));
        }
    }

    // There is a copy of the implementation of this mixin at widgets/automatic_keep_alive.cs,
    // in AutomaticKeepAliveClientWithTickerProviderStateMixin, remember to keep the copy up to date
    public abstract class TickerProviderStateMixin<T> : State<T>, TickerProvider where T : StatefulWidget {
        HashSet<Ticker> _tickers;

        public Ticker createTicker(TickerCallback onTick) {
            this._tickers = this._tickers ?? new HashSet<Ticker>();

            Func<string> debugLabel = null;
            D.assert(() => {
                debugLabel = () => "created by " + this;
                return true;
            });
            var result = new _WidgetTicker<T>(onTick, this, debugLabel: debugLabel);
            this._tickers.Add(result);
            return result;
        }

        internal void _removeTicker(_WidgetTicker<T> ticker) {
            D.assert(this._tickers != null);
            D.assert(this._tickers.Contains(ticker));
            this._tickers.Remove(ticker);
        }

        public override void dispose() {
            D.assert(() => {
                if (this._tickers != null) {
                    foreach (Ticker ticker in this._tickers) {
                        if (ticker.isActive) {
                            throw new UIWidgetsError(
                                this + " was disposed with an active Ticker.\n" +
                                this.GetType() +
                                " created a Ticker via its TickerProviderStateMixin, but at the time " +
                                "dispose() was called on the mixin, that Ticker was still active. All Tickers must " +
                                "be disposed before calling base.dispose(). Tickers used by AnimationControllers " +
                                "should be disposed by calling dispose() on the AnimationController itself. " +
                                "Otherwise, the ticker will leak.\n" +
                                "The offending ticker was: " + ticker.toString(debugIncludeStack: true)
                            );
                        }
                    }
                }

                return true;
            });
            base.dispose();
        }

        public override void didChangeDependencies() {
            bool muted = !TickerMode.of(this.context);
            if (this._tickers != null) {
                foreach (Ticker ticker in this._tickers) {
                    ticker.muted = muted;
                }
            }

            base.didChangeDependencies();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<HashSet<Ticker>>(
                "tickers",
                this._tickers,
                description: this._tickers != null ? "tracking " + this._tickers.Count + " tickers" : null,
                defaultValue: Diagnostics.kNullDefaultValue
            ));
        }
    }

    class _WidgetTicker<T> : Ticker where T : StatefulWidget {
        internal _WidgetTicker(
            TickerCallback onTick,
            TickerProviderStateMixin<T> creator,
            Func<string> debugLabel = null) :
            base(onTick: onTick, debugLabel: debugLabel) {
            this._creator = creator;
        }

        readonly TickerProviderStateMixin<T> _creator;

        public override void dispose() {
            this._creator._removeTicker(this);
            base.dispose();
        }
    }
}