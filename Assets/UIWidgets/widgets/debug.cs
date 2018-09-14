using System;
using System.Collections.Generic;
using UIWidgets.foundation;

namespace UIWidgets.widgets {
    public static class WidgetsD {
        public static bool debugPrintRebuildDirtyWidgets = false;

        public static bool debugPrintBuildScope = false;

        public static bool debugPrintGlobalKeyedWidgetLifecycle = false;

        public static bool debugPrintScheduleBuildForStacks = false;

        static Key _firstNonUniqueKey(IEnumerable<Widget> widgets) {
            var keySet = new HashSet<Key>();
            foreach (Widget widget in widgets) {
                D.assert(widget != null);
                if (widget.key == null) {
                    continue;
                }

                if (!keySet.Add(widget.key)) {
                    return widget.key;
                }
            }

            return null;
        }

        public static bool debugChildrenHaveDuplicateKeys(Widget parent, IEnumerable<Widget> children) {
            D.assert(() => {
                Key nonUniqueKey = _firstNonUniqueKey(children);
                if (nonUniqueKey != null) {
                    throw new UIWidgetsError(
                        "Duplicate keys found.\n" +
                        "If multiple keyed nodes exist as children of another node, they must have unique keys.\n" +
                        parent + " has multiple children with key " + children + "."
                    );
                }

                return true;
            });
            return false;
        }


        public static void debugWidgetBuilderValue(Widget widget, Widget built) {
            D.assert(() => {
                if (built == null) {
                    throw new UIWidgetsError(
                        "A build function returned null.\n" +
                        "The offending widget is: " + widget + "\n" +
                        "Build functions must never return null. " +
                        "To return an empty space that causes the building widget to fill available room, return \"new Container()\". " +
                        "To return an empty space that takes as little room as possible, return \"new Container(width: 0.0, height: 0.0)\".");
                }

                return true;
            });
        }

        internal static UIWidgetsErrorDetails _debugReportException(
            string context,
            Exception exception,
            InformationCollector informationCollector = null
        ) {
            var details = new UIWidgetsErrorDetails(
                exception: exception,
                library: "widgets library",
                context: context,
                informationCollector: informationCollector
            );
            UIWidgetsError.reportError(details);
            return details;
        }
    }
}