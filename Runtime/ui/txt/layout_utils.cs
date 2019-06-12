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

        public static int getPrevWordBreakForCache(TextBuff buff, int offset) {
            int len = buff.size;
            if (offset == 0) {
                return 0;
            }

            if (offset > len) {
                offset = len;
            }
            if (isWordBreakBefore(buff.charAt(offset - 1))) {
                return offset - 1;
            }
            for (int i = offset - 1; i > 0; i--) {
                if (isWordBreakBefore(buff.charAt(i)) || isWordBreakAfter(buff.charAt(i - 1))) {
                    return i;
                }
            }
            return 0;
        }

        
        public static int getNextWordBreakForCache(TextBuff buff, int offset) {
            int len = buff.size;
            if (offset >= len) {
                return len;
            }

            if (isWordBreakAfter(buff.charAt(offset))) {
                return offset + 1;
            }

            for (int i = offset + 1; i < len; i++) {
                if (isWordBreakBefore(buff.charAt(i))) {
                    return i;
                }
            }

            return len;
        }

        public static bool isWordBreakAfter(ushort c) {
            if (isWordSpace(c) || (c >= 0x2000 && c <= 0x200a) || c == 0x3000) {
                // spaces
                return true;
            }
            return false;
        }
        
        public static bool isWordBreakBefore(ushort c) {
            return isWordBreakAfter(c) || (c >= 0x3400 && c <= 0x9fff);
        }
        
    }
}