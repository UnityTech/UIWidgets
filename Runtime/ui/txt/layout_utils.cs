namespace Unity.UIWidgets.ui {
    public static class LayoutUtils {
        public const char CHAR_NBSP = '\u00A0';

        public static bool isWordSpace(char ch) {
            return ch == ' ' || ch == CHAR_NBSP;
        }

        public static bool isLineEndSpace(char c) {
            return c == '\n' || c == ' ' || c == 0x1680 || (0x2000 <= c && c <= 0x200A && c != 0x2007) ||
                   c == 0x205F || c == 0x3000;
        }
    }
}