using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    class _Pending {
        public _Pending(LocalizationsDelegate del, IPromise<object> futureValue) {
            this.del = del;
            this.futureValue = futureValue;
        }

        public readonly LocalizationsDelegate del;
        public readonly IPromise<object> futureValue;

        internal static IPromise<Dictionary<Type, object>> _loadAll(Locale locale,
            IEnumerable<LocalizationsDelegate> allDelegates) {
            Dictionary<Type, object> output = new Dictionary<Type, object>();
            List<_Pending> pendingList = null;

            HashSet<Type> types = new HashSet<Type>();
            List<LocalizationsDelegate> delegates = new List<LocalizationsDelegate>();
            foreach (LocalizationsDelegate del in allDelegates) {
                if (!types.Contains(del.type) && del.isSupported(locale)) {
                    types.Add(del.type);
                    delegates.Add(del);
                }
            }

            foreach (LocalizationsDelegate del in delegates) {
                IPromise<object> inputValue = del.load(locale);
                object completedValue = null;
                IPromise<object> futureValue = inputValue.Then(value => { return completedValue = value; });
                if (completedValue != null) {
                    Type type = del.type;
                    D.assert(!output.ContainsKey(type));
                    output[type] = completedValue;
                }
                else {
                    pendingList = pendingList ?? new List<_Pending>();
                    pendingList.Add(new _Pending(del, futureValue));
                }
            }

            if (pendingList == null) {
                return Promise<Dictionary<Type, object>>.Resolved(output);
            }

            return Promise<object>.All(pendingList.Select(p => p.futureValue))
                .Then(values => {
                    var list = values.ToList();
                    D.assert(list.Count == pendingList.Count);
                    for (int i = 0; i < list.Count; i += 1) {
                        Type type = pendingList[i].del.type;
                        D.assert(!output.ContainsKey(type));
                        output[type] = list[i];
                    }

                    return output;
                });
        }
    }

    public abstract class LocalizationsDelegate {
        protected LocalizationsDelegate() {
        }

        public abstract bool isSupported(Locale locale);

        public abstract IPromise<object> load(Locale locale);

        public abstract bool shouldReload(LocalizationsDelegate old);

        public abstract Type type { get; }

        public override string ToString() {
            return $"{this.GetType()}[{this.type}]";
        }
    }

    public abstract class LocalizationsDelegate<T> : LocalizationsDelegate {
        public override Type type {
            get { return typeof(T); }
        }
    }

    public abstract class WidgetsLocalizations {
        static WidgetsLocalizations of(BuildContext context) {
            return Localizations.of<WidgetsLocalizations>(context, typeof(WidgetsLocalizations));
        }
    }

    class _WidgetsLocalizationsDelegate : LocalizationsDelegate<WidgetsLocalizations> {
        public _WidgetsLocalizationsDelegate() {
        }

        public override bool isSupported(Locale locale) {
            return true;
        }

        public override IPromise<object> load(Locale locale) {
            return DefaultWidgetsLocalizations.load(locale);
        }

        public override bool shouldReload(LocalizationsDelegate old) {
            return false;
        }

        public override string ToString() {
            return "DefaultWidgetsLocalizations.delegate(en_US)";
        }
    }

    public class DefaultWidgetsLocalizations : WidgetsLocalizations {
        public DefaultWidgetsLocalizations() {
        }

        public static IPromise<object> load(Locale locale) {
            return Promise<object>.Resolved(new DefaultWidgetsLocalizations());
        }

        public static readonly LocalizationsDelegate<WidgetsLocalizations> del = new _WidgetsLocalizationsDelegate();
    }

    class _LocalizationsScope : InheritedWidget {
        public _LocalizationsScope(
            Key key,
            Locale locale,
            _LocalizationsState localizationsState,
            Dictionary<Type, object> typeToResources,
            Widget child
        ) : base(key: key, child: child) {
            D.assert(locale != null);
            D.assert(localizationsState != null);
            D.assert(typeToResources != null);
            this.locale = locale;
            this.localizationsState = localizationsState;
            this.typeToResources = typeToResources;
        }

        public readonly Locale locale;

        public readonly _LocalizationsState localizationsState;

        public readonly Dictionary<Type, object> typeToResources;

        public override bool updateShouldNotify(InheritedWidget old) {
            return this.typeToResources != ((_LocalizationsScope) old).typeToResources;
        }
    }

    public class Localizations : StatefulWidget {
        public Localizations(
            Key key = null,
            Locale locale = null,
            List<LocalizationsDelegate> delegates = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(locale != null);
            D.assert(delegates != null);
            D.assert(delegates.Any(del => del is LocalizationsDelegate<WidgetsLocalizations>));
            this.locale = locale;
            this.delegates = delegates;
            this.child = child;
        }

        public static Localizations overrides(
            Key key = null,
            BuildContext context = null,
            Locale locale = null,
            List<LocalizationsDelegate> delegates = null,
            Widget child = null
        ) {
            List<LocalizationsDelegate> mergedDelegates = _delegatesOf(context);
            if (delegates != null) {
                mergedDelegates.InsertRange(0, delegates);
            }

            return new Localizations(
                key: key,
                locale: locale ?? localeOf(context),
                delegates: mergedDelegates,
                child: child
            );
        }

        public readonly Locale locale;

        public readonly List<LocalizationsDelegate> delegates;

        public readonly Widget child;

        public static Locale localeOf(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            _LocalizationsScope scope =
                (_LocalizationsScope) context.inheritFromWidgetOfExactType(typeof(_LocalizationsScope));
            if (nullOk && scope == null) {
                return null;
            }

            D.assert((bool) (scope != null), () => "a Localizations ancestor was not found");
            return scope.localizationsState.locale;
        }

        public static List<LocalizationsDelegate> _delegatesOf(BuildContext context) {
            D.assert(context != null);
            _LocalizationsScope scope =
                (_LocalizationsScope) context.inheritFromWidgetOfExactType(typeof(_LocalizationsScope));
            D.assert(scope != null, () => "a Localizations ancestor was not found");
            return new List<LocalizationsDelegate>(scope.localizationsState.widget.delegates);
        }

        public static T of<T>(BuildContext context, Type type) {
            D.assert(context != null);
            D.assert(type != null);
            _LocalizationsScope scope =
                (_LocalizationsScope) context.inheritFromWidgetOfExactType(typeof(_LocalizationsScope));
            if (scope != null && scope.localizationsState != null) {
                return scope.localizationsState.resourcesFor<T>(type);
            }

            return default;
        }

        public override State createState() {
            return new _LocalizationsState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Locale>("locale", this.locale));
            properties.add(new EnumerableProperty<LocalizationsDelegate>("delegates", this.delegates));
        }
    }


    class _LocalizationsState : State<Localizations> {
        readonly GlobalKey _localizedResourcesScopeKey = GlobalKey.key();
        Dictionary<Type, object> _typeToResources = new Dictionary<Type, object>();

        public Locale locale {
            get { return this._locale; }
        }

        Locale _locale;

        public override void initState() {
            base.initState();
            this.load(this.widget.locale);
        }

        bool _anyDelegatesShouldReload(Localizations old) {
            if (this.widget.delegates.Count != old.delegates.Count) {
                return true;
            }

            List<LocalizationsDelegate> delegates = this.widget.delegates.ToList();
            List<LocalizationsDelegate> oldDelegates = old.delegates.ToList();
            for (int i = 0; i < delegates.Count; i += 1) {
                LocalizationsDelegate del = delegates[i];
                LocalizationsDelegate oldDelegate = oldDelegates[i];
                if (del.GetType() != oldDelegate.GetType() || del.shouldReload(oldDelegate)) {
                    return true;
                }
            }

            return false;
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            Localizations old = (Localizations) oldWidget;
            base.didUpdateWidget(old);
            if (this.widget.locale != old.locale
                || (this.widget.delegates == null && old.delegates != null)
                || (this.widget.delegates != null && old.delegates == null)
                || (this.widget.delegates != null && this._anyDelegatesShouldReload(old))) {
                this.load(this.widget.locale);
            }
        }

        void load(Locale locale) {
            var delegates = this.widget.delegates;
            if (delegates == null || delegates.isEmpty()) {
                this._locale = locale;
                return;
            }

            Dictionary<Type, object> typeToResources = null;
            IPromise<Dictionary<Type, object>> typeToResourcesFuture = _Pending._loadAll(locale, delegates)
                .Then(value => { return typeToResources = value; });

            if (typeToResources != null) {
                this._typeToResources = typeToResources;
                this._locale = locale;
            }
            else {
                // WidgetsBinding.instance.deferFirstFrameReport();
                typeToResourcesFuture.Then(value => {
                    // WidgetsBinding.instance.allowFirstFrameReport();
                    if (!this.mounted) {
                        return;
                    }

                    this.setState(() => {
                        this._typeToResources = value;
                        this._locale = locale;
                    });
                });
            }
        }

        public T resourcesFor<T>(Type type) {
            D.assert(type != null);
            T resources = (T) this._typeToResources.getOrDefault(type);
            return resources;
        }

        public override Widget build(BuildContext context) {
            if (this._locale == null) {
                return new Container();
            }

            return new _LocalizationsScope(
                key: this._localizedResourcesScopeKey,
                locale: this._locale,
                localizationsState: this,
                typeToResources: this._typeToResources,
                child: this.widget.child
            );
        }
    }
}