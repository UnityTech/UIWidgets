using System.Collections.Generic;
using UnityEngine;

namespace Unity.UIWidgets.service {
    static partial class TextInputUtils {
        static Dictionary<Event, TextInputAction> _keyToOperations;

        public static TextInputAction? getInputAction(Event evt) {
            if (_keyToOperations == null) {
                initKeyToOperations();
            }

            EventModifiers m = evt.modifiers;
            evt.modifiers &= ~EventModifiers.CapsLock;
            TextInputAction result;
            var exists = _keyToOperations.TryGetValue(evt, out result);
            evt.modifiers = m;
            if (exists) {
                return result;
            }

            return null;
        }

        public static void initKeyToOperations() {
            if (_keyToOperations != null) {
                return;
            }

            _keyToOperations = new Dictionary<Event, TextInputAction>();

            // key mappings shared by the platforms
            mapKey("return", TextInputAction.newline);
            mapKey("left", TextInputAction.moveLeft);
            mapKey("right", TextInputAction.moveRight);
            mapKey("up", TextInputAction.moveUp);
            mapKey("down", TextInputAction.moveDown);

            mapKey("#left", TextInputAction.selectLeft);
            mapKey("#right", TextInputAction.selectRight);
            mapKey("#up", TextInputAction.selectUp);
            mapKey("#down", TextInputAction.selectDown);

            mapKey("delete", TextInputAction.delete);
            mapKey("backspace", TextInputAction.backspace);
            mapKey("#backspace", TextInputAction.backspace);

            // OSX is the special case for input shortcuts
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX) {
                // Keyboard mappings for mac
                // TODO     mapKey ("home", TextInputAction.scrollStart);
                // TODO     mapKey ("end", TextInputAction.scrollEnd);
                // TODO     mapKey ("page up", TextInputAction.scrollPageUp);
                // TODO     mapKey ("page down", TextInputAction.scrollPageDown);

                mapKey("^left", TextInputAction.moveGraphicalLineStart);
                mapKey("^right", TextInputAction.moveGraphicalLineEnd);
                // TODO     mapKey ("^up", TextInputAction.scrollPageUp);
                // TODO     mapKey ("^down", TextInputAction.scrollPageDown);

                mapKey("&left", TextInputAction.moveWordLeft);
                mapKey("&right", TextInputAction.moveWordRight);
                mapKey("&up", TextInputAction.moveParagraphBackward);
                mapKey("&down", TextInputAction.moveParagraphForward);

                mapKey("%left", TextInputAction.moveGraphicalLineStart);
                mapKey("%right", TextInputAction.moveGraphicalLineEnd);
                mapKey("%up", TextInputAction.moveTextStart);
                mapKey("%down", TextInputAction.moveTextEnd);

                mapKey("#home", TextInputAction.selectTextStart);
                mapKey("#end", TextInputAction.selectTextEnd);
                // TODO         mapKey ("#page up", TextInputAction.selectPageUp);
                // TODO         mapKey ("#page down", TextInputAction.selectPageDown);

                mapKey("#^left", TextInputAction.expandSelectGraphicalLineStart);
                mapKey("#^right", TextInputAction.expandSelectGraphicalLineEnd);
                mapKey("#^up", TextInputAction.selectParagraphBackward);
                mapKey("#^down", TextInputAction.selectParagraphForward);

                mapKey("#&left", TextInputAction.selectWordLeft);
                mapKey("#&right", TextInputAction.selectWordRight);
                mapKey("#&up", TextInputAction.selectParagraphBackward);
                mapKey("#&down", TextInputAction.selectParagraphForward);

                mapKey("#%left", TextInputAction.expandSelectGraphicalLineStart);
                mapKey("#%right", TextInputAction.expandSelectGraphicalLineEnd);
                mapKey("#%up", TextInputAction.selectTextStart);
                mapKey("#%down", TextInputAction.selectTextEnd);

                mapKey("%a", TextInputAction.selectAll);
                mapKey("%x", TextInputAction.cut);
                mapKey("%c", TextInputAction.copy);
                mapKey("%v", TextInputAction.paste);

                // emacs-like keybindings
                mapKey("^d", TextInputAction.delete);
                mapKey("^h", TextInputAction.backspace);
                mapKey("^b", TextInputAction.moveLeft);
                mapKey("^f", TextInputAction.moveRight);
                mapKey("^a", TextInputAction.moveLineStart);
                mapKey("^e", TextInputAction.moveLineEnd);

                mapKey("&delete", TextInputAction.deleteWordForward);
                mapKey("&backspace", TextInputAction.deleteWordBack);
                mapKey("%backspace", TextInputAction.deleteLineBack);
            }
            else {
                // Windows/Linux keymappings
                mapKey("home", TextInputAction.moveGraphicalLineStart);
                mapKey("end", TextInputAction.moveGraphicalLineEnd);
                // TODO     mapKey ("page up", TextInputAction.movePageUp);
                // TODO     mapKey ("page down", TextInputAction.movePageDown);

                mapKey("%left", TextInputAction.moveWordLeft);
                mapKey("%right", TextInputAction.moveWordRight);
                mapKey("%up", TextInputAction.moveParagraphBackward);
                mapKey("%down", TextInputAction.moveParagraphForward);

                mapKey("^left", TextInputAction.moveToEndOfPreviousWord);
                mapKey("^right", TextInputAction.moveToStartOfNextWord);
                mapKey("^up", TextInputAction.moveParagraphBackward);
                mapKey("^down", TextInputAction.moveParagraphForward);

                mapKey("#^left", TextInputAction.selectToEndOfPreviousWord);
                mapKey("#^right", TextInputAction.selectToStartOfNextWord);
                mapKey("#^up", TextInputAction.selectParagraphBackward);
                mapKey("#^down", TextInputAction.selectParagraphForward);

                mapKey("#home", TextInputAction.selectGraphicalLineStart);
                mapKey("#end", TextInputAction.selectGraphicalLineEnd);
                // TODO         mapKey ("#page up", TextInputAction.selectPageUp);
                // TODO         mapKey ("#page down", TextInputAction.selectPageDown);

                mapKey("^delete", TextInputAction.deleteWordForward);
                mapKey("^backspace", TextInputAction.deleteWordBack);
                mapKey("%backspace", TextInputAction.deleteLineBack);

                mapKey("^a", TextInputAction.selectAll);
                mapKey("^x", TextInputAction.cut);
                mapKey("^c", TextInputAction.copy);
                mapKey("^v", TextInputAction.paste);
                mapKey("#delete", TextInputAction.cut);
                mapKey("^insert", TextInputAction.copy);
                mapKey("#insert", TextInputAction.paste);
            }
        }

        static void mapKey(string key, TextInputAction action) {
            _keyToOperations[Event.KeyboardEvent(key)] = action;
        }
    }
}