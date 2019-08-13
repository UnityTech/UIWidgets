namespace Unity.UIWidgets.ui {
    static class LayoutUtils {
        public const char CHAR_NBSP = '\u00A0';

        public static bool isWordSpace(ushort ch) {
            return ch == ' ' || ch == CHAR_NBSP;
        }

        public static bool isLineEndSpace(char c) {
            return c == '\n' || c == ' ' || c == 0x1680 || (0x2000 <= c && c <= 0x200A && c != 0x2007) ||
                   c == 0x205F || c == 0x3000;
        }


        public static int getNextWordBreak(string text, int offset, int maxOffset) {
            int len = text.Length;
            if (len > maxOffset) {
                len = maxOffset + 1;
            }

            if (offset >= len) {
                return len;
            }

            if (isWordBreakAfter(text[offset])) {
                return offset + 1;
            }

            for (int i = offset + 1; i < len; i++) {
                if (isWordBreakBefore(text[i])) {
                    return i;
                }
            }

            return maxOffset;
        }

        public static bool isWordBreakAfter(ushort c) {
            return isWordSpace(c) || (c >= 0x2000 && c <= 0x200a) || c == 0x3000;
        }

        public static bool isWordBreakBefore(ushort c) {
            return isWordBreakAfter(c) || (c >= 0x3400 && c <= 0x9fff);
        }

        public static int minPowerOfTwo(int i) {
            // Assume that int is 32 bit
            i--;
            i = i | (i >> 1);
            i = i | (i >> 2);
            i = i | (i >> 4);
            i = i | (i >> 8);
            i = i | (i >> 16);
            return i + 1;
        }
    }
}