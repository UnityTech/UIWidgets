using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unity.UIWidgets.foundation {
    public delegate void UIWidgetsExceptionHandler(UIWidgetsErrorDetails details);

    public delegate void InformationCollector(StringBuilder information);

    public class UIWidgetsErrorDetails {
        public UIWidgetsErrorDetails(
            Exception exception = null,
            string library = "UIWidgets framework",
            string context = null,
            EnumerableFilter<string> stackFilter = null,
            InformationCollector informationCollector = null,
            bool silent = false
        ) {
            this.exception = exception;
            this.library = library;
            this.context = context;
            this.stackFilter = stackFilter;
            this.informationCollector = informationCollector;
            this.silent = silent;
        }

        public readonly Exception exception;
        public readonly string library;
        public readonly string context;
        public readonly EnumerableFilter<string> stackFilter;
        public readonly InformationCollector informationCollector;
        public readonly bool silent;

        public string exceptionAsString() {
            string longMessage = null;

            if (this.exception != null) {
                longMessage = this.exception.Message;
            }

            if (longMessage != null) {
                longMessage = longMessage.TrimEnd();
            }

            if (longMessage.isEmpty()) {
                longMessage = "<no message available>";
            }

            return longMessage;
        }

        public override string ToString() {
            var buffer = new StringBuilder();
            if (this.library.isNotEmpty() || this.context.isNotEmpty()) {
                if (this.library.isNotEmpty()) {
                    buffer.AppendFormat("Error caught by {0}", this.library);
                    if (this.context.isNotEmpty()) {
                        buffer.Append(", ");
                    }
                }
                else {
                    buffer.Append("Exception ");
                }

                if (this.context.isNotEmpty()) {
                    buffer.AppendFormat("thrown {0}", this.context);
                }

                buffer.Append(". ");
            }
            else {
                buffer.Append("An error was caught. ");
            }

            buffer.AppendLine(this.exceptionAsString());
            if (this.informationCollector != null) {
                this.informationCollector(buffer);
            }

            if (this.exception.StackTrace != null) {
                IEnumerable<string> stackLines = this.exception.StackTrace.TrimEnd().Split('\n');
                if (this.stackFilter != null) {
                    stackLines = this.stackFilter(stackLines);
                }
                else {
                    stackLines = UIWidgetsError.defaultStackFilter(stackLines);
                }

                buffer.Append(string.Join("\n", stackLines.ToArray()));
            }

            return buffer.ToString().TrimEnd();
        }
    }

    public class UIWidgetsError : Exception {
        public UIWidgetsError(string message) : base(message) {
        }

        public static UIWidgetsExceptionHandler onError = dumpErrorToConsole;

        public static void dumpErrorToConsole(UIWidgetsErrorDetails details) {
            dumpErrorToConsole(details, forceReport: false);
        }

        public static void dumpErrorToConsole(UIWidgetsErrorDetails details, bool forceReport = false) {
            D.assert(details != null);
            D.assert(details.exception != null);
            bool reportError = !details.silent;
            D.assert(() => {
                reportError = true;
                return true;
            });
            if (!reportError && !forceReport) {
                return;
            }

            D.logError(details.ToString(), details.exception);
        }

        public static IEnumerable<string> defaultStackFilter(IEnumerable<string> frames) {
            return frames;
        }

        public static void reportError(UIWidgetsErrorDetails details) {
            D.assert(details != null);
            D.assert(details.exception != null);
            if (onError != null) {
                onError(details);
            }
        }
    }
}