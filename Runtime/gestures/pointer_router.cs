using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public delegate void PointerRoute(PointerEvent evt);

    public class PointerRouter {
        readonly Dictionary<int, HashSet<PointerRoute>> _routeMap = new Dictionary<int, HashSet<PointerRoute>>();

        readonly HashSet<PointerRoute> _globalRoutes = new HashSet<PointerRoute>();

        public void addRoute(int pointer, PointerRoute route) {
            var routes = this._routeMap.putIfAbsent(pointer, () => new HashSet<PointerRoute>());
            D.assert(!routes.Contains(route));
            routes.Add(route);
        }

        public void removeRoute(int pointer, PointerRoute route) {
            D.assert(this._routeMap.ContainsKey(pointer));
            var routes = this._routeMap[pointer];
            routes.Remove(route);
            if (routes.isEmpty()) {
                this._routeMap.Remove(pointer);
            }
        }
                
        public bool acceptScroll() {
            return this._routeMap.Count == 0;
        }

        public void clearScrollRoute(int pointer) {
            if (this._routeMap.ContainsKey(pointer)) {
                this._routeMap.Remove(pointer);
            }
        }

        public void addGlobalRoute(PointerRoute route) {
            D.assert(!this._globalRoutes.Contains(route));
            this._globalRoutes.Add(route);
        }

        public void removeGlobalRoute(PointerRoute route) {
            D.assert(this._globalRoutes.Contains(route));
            this._globalRoutes.Remove(route);
        }

        void _dispatch(PointerEvent evt, PointerRoute route) {
            try {
                route(evt);
            }
            catch (Exception ex) {
                D.logError("Error while routing a pointer event: ", ex);
            }
        }

        public void route(PointerEvent evt) {
            HashSet<PointerRoute> routes;
            this._routeMap.TryGetValue(evt.pointer, out routes);
            if (routes != null) {
                foreach (PointerRoute route in new List<PointerRoute>(routes)) {
                    if (routes.Contains(route)) {
                        this._dispatch(evt, route);
                    }
                }
            }

            foreach (PointerRoute route in new List<PointerRoute>(this._globalRoutes)) {
                if (this._globalRoutes.Contains(route)) {
                    this._dispatch(evt, route);
                }
            }
        }
    }
}