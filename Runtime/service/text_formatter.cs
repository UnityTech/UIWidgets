using System;
using System.Text.RegularExpressions;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.service {
    public abstract class TextInputFormatter {
        public delegate TextEditingValue TextInputFormatFunction(TextEditingValue oldValue,
            TextEditingValue newValue);

        public abstract TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue);

        static TextInputFormatter withFunction(TextInputFormatFunction formatFunction) {
            return new _SimpleTextInputFormatter(formatFunction);
        }
    }

    class _SimpleTextInputFormatter : TextInputFormatter {
        public readonly TextInputFormatFunction formatFunction;

        internal _SimpleTextInputFormatter(TextInputFormatFunction formatFunction) {
            D.assert(formatFunction != null);
            this.formatFunction = formatFunction;
        }

        public override TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
            return this.formatFunction(oldValue, newValue);
        }
    }

    public class BlacklistingTextInputFormatter : TextInputFormatter {
        public readonly Regex blacklistedPattern;

        public readonly string replacementString;

        public static readonly BlacklistingTextInputFormatter singleLineFormatter
            = new BlacklistingTextInputFormatter(new Regex(@"\n"));

        public BlacklistingTextInputFormatter(Regex blacklistedPattern, string replacementString = "") {
            D.assert(blacklistedPattern != null);
            this.blacklistedPattern = blacklistedPattern;
            this.replacementString = replacementString;
        }


        public override TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
            return Util._selectionAwareTextManipulation(newValue,
                (substring) => this.blacklistedPattern.Replace(substring, this.replacementString));
        }
    }
    
    public class LengthLimitingTextInputFormatter : TextInputFormatter {

        public LengthLimitingTextInputFormatter(int? maxLength) {
            D.assert(maxLength == null || maxLength == -1 || maxLength > 0);
            this.maxLength = maxLength;
        }

        public readonly int? maxLength;

        public override TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
            if (this.maxLength != null && this.maxLength > 0 && newValue.text.Length > this.maxLength) {
                TextSelection newSelection = newValue.selection.copyWith(
                    baseOffset: Mathf.Min(newValue.selection.start, this.maxLength.Value),
                    extentOffset: Mathf.Min(newValue.selection.end, this.maxLength.Value)
                );
                
                string truncated = newValue.text.Substring(0, this.maxLength.Value);
                return new TextEditingValue(
                    text: truncated,
                    selection: newSelection,
                    composing: TextRange.empty
                );
            }
            return newValue;
        }
}

    static class Util {
        internal static TextEditingValue _selectionAwareTextManipulation(TextEditingValue value,
            Func<string, string> substringManipulation) {
            int selectionStartIndex = value.selection.start;
            int selectionEndIndex = value.selection.end;
            string manipulatedText;
            TextSelection manipulatedSelection = null;
            if (selectionStartIndex < 0 || selectionEndIndex < 0) {
                manipulatedText = substringManipulation(value.text);
            }
            else {
                var beforeSelection = substringManipulation(
                    value.text.Substring(0, selectionStartIndex)
                );
                var inSelection = substringManipulation(
                    value.text.Substring(selectionStartIndex, selectionEndIndex - selectionStartIndex)
                );
                var afterSelection = substringManipulation(
                    value.text.Substring(selectionEndIndex)
                );
                manipulatedText = beforeSelection + inSelection + afterSelection;
                if (value.selection.baseOffset > value.selection.extentOffset) {
                    manipulatedSelection = value.selection.copyWith(
                        baseOffset: beforeSelection.Length + inSelection.Length,
                        extentOffset: beforeSelection.Length
                    );
                }
                else {
                    manipulatedSelection = value.selection.copyWith(
                        baseOffset: beforeSelection.Length,
                        extentOffset: beforeSelection.Length + inSelection.Length
                    );
                }
            }

            return new TextEditingValue(
                text: manipulatedText,
                selection: manipulatedSelection ?? TextSelection.collapsed(offset: -1),
                composing: manipulatedText == value.text ? value.composing : TextRange.empty
            );
        }
    }
}