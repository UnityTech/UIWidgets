using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;
using UnityEngine.Windows;

namespace Unity.UIWidgets.ui {
    public class EmojiUtils {

        static Image _image;

        public static Image image {
            get {
                if (_image == null || _image.texture == null) {
                    try {
                        _image = new Image(
                            Resources.Load<Texture2D>("Emoji")
                        );
                    }
                    catch (Exception e) {
                        _image = null;
                    }
                }

                return _image;
            }
        }

        public static readonly Dictionary<uint, int> emojiLookupTable = new Dictionary<uint, int> {
            {0x1F60A, 0},
            {0x1F60B, 1},
            {0x1F60D, 2},
            {0x1F60E, 3},
            {0x1F600, 4},
            {0x1F601, 5},
            {0x1F602, 6},
            {0x1F603, 7},
            {0x1F604, 8},
            {0x1F605, 9},
            {0x1F606, 10},
            {0x1F61C, 11},
            {0x1F618, 12},
            {0x1F62D, 13},
            {0x1F60C, 14},
            {0x1F61E, 15},
        };

        public const int rowCount = 4;
        public const int colCount = 4;

        public static Rect getUVRect(uint code) {
            bool exist = emojiLookupTable.TryGetValue(code, out int index);
            if (exist) {
                return Rect.fromLTWH(
                    (index % colCount) * (1.0f / colCount), 
                    (rowCount - 1 - (index / colCount)) * (1.0f / rowCount),
                    1.0f / colCount, 1.0f / rowCount);
            }

            Debug.LogWarning($"Unrecognized unicode for emoji {code:x}");
            return Rect.fromLTWH(0, 0, 0, 0);
        }
        
        public static void encodeSurrogatePair(uint character, out char a, out char b) {
            uint code;
            D.assert(0x10000 <= character && character <= 0x10FFFF);
            code = (character - 0x10000);
            a = (char) (0xD800 | (code >> 10));
            b = (char) (0xDC00 | (code & 0x3FF));
        }

        public static uint decodeSurrogatePair(char a, char b) {
            uint code;
            D.assert(0xD800 <= a && a <= 0xDBFF);
            D.assert(0xDC00 <= b && b <= 0xDFFF);
            code = 0x10000;
            code += (uint) ((a & 0x03FF) << 10);
            code += (uint) (b & 0x03FF);
            return code;
        }

        public static bool isSurrogatePairStart(uint c) {
            return 0xD800 <= c && c <= 0xDBFF;
        }

        public static bool isSurrogatePairEnd(uint c) {
            return 0xDC00 <= c && c <= 0xDFFF;
        }

        public static List<string> splitBySurrogatePair(string text) {
            int start = 0;
            List<string> list = new List<string>();
            for (int i = 0; i < text.Length; i++) {
                if (i < text.Length - 1 && isSurrogatePairStart(text[i]) && isSurrogatePairEnd(text[i + 1])) {
                    if (i > start) {
                        list.Add(text.Substring(start, i - start));
                    }

                    start = i + 2;
                    list.Add(text.Substring(i, 2));
                    i++;
                }
            }

            if (start < text.Length) {
                list.Add(text.Substring(start));
            }

            return list;
        }
    }
}