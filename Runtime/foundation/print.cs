using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Unity.UIWidgets.foundation {
    enum _WordWrapParseMode {
        inSpace,
        inWord,
        atBreak
    }

    public static class DebugPrint {
        static readonly Regex _indentPattern = new Regex("^ *(?:[-+*] |[0-9]+[.):] )?");

        public static IEnumerable<string> debugWordWrap(string message, int width, string wrapIndent = "") {
            if (message.Length < width || message.TrimStart()[0] == '#') {
                yield return message;
                yield break;
            }

            Match prefixMatch = _indentPattern.Match(message);
            string prefix = wrapIndent + new string(' ', prefixMatch.Groups[0].Value.Length);
            int start = 0;
            int startForLengthCalculations = 0;
            bool addPrefix = false;
            int index = prefix.Length;
            _WordWrapParseMode mode = _WordWrapParseMode.inSpace;
            int lastWordStart = 0;
            int? lastWordEnd = null;
            while (true) {
                switch (mode) {
                    case _WordWrapParseMode.inSpace:
                        // at start of break point (or start of line); can"t break until next break
                        while ((index < message.Length) && (message[index] == ' ')) {
                            index += 1;
                        }

                        lastWordStart = index;
                        mode = _WordWrapParseMode.inWord;
                        break;
                    case _WordWrapParseMode.inWord: // looking for a good break point
                        while ((index < message.Length) && (message[index] != ' ')) {
                            index += 1;
                        }

                        mode = _WordWrapParseMode.atBreak;
                        break;
                    case _WordWrapParseMode.atBreak: // at start of break point
                        if ((index - startForLengthCalculations > width) || (index == message.Length)) {
                            // we are over the width line, so break
                            if ((index - startForLengthCalculations <= width) || (lastWordEnd == null)) {
                                // we should use this point, because either it doesn"t actually go over the
                                // end (last line), or it does, but there was no earlier break point
                                lastWordEnd = index;
                            }

                            if (addPrefix) {
                                yield return prefix + message.Substring(start, lastWordEnd.Value - start);
                            }
                            else {
                                yield return message.Substring(start, lastWordEnd.Value - start);
                                addPrefix = true;
                            }

                            if (lastWordEnd >= message.Length) {
                                yield break;
                            }

                            // just yield returned a line
                            if (lastWordEnd == index) {
                                // we broke at current position
                                // eat all the spaces, then set our start point
                                while ((index < message.Length) && (message[index] == ' ')) {
                                    index += 1;
                                }

                                start = index;
                                mode = _WordWrapParseMode.inWord;
                            }
                            else {
                                // we broke at the previous break point, and we"re at the start of a new one
                                D.assert(lastWordStart > lastWordEnd);
                                start = lastWordStart;
                                mode = _WordWrapParseMode.atBreak;
                            }

                            startForLengthCalculations = start - prefix.Length;
                            D.assert(addPrefix);
                            lastWordEnd = null;
                        }
                        else {
                            // save this break point, we"re not yet over the line width
                            lastWordEnd = index;
                            // skip to the end of this break point
                            mode = _WordWrapParseMode.inSpace;
                        }

                        break;
                }
            }
        }
    }
}