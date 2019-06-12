using System;
using System.Collections.Generic;
using UnityEngine;

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
                    catch (Exception) {
                        _image = null;
                    }
                }

                return _image;
            }
        }

        public static readonly Dictionary<int, int> emojiLookupTable = new Dictionary<int, int> {
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

        public static Rect getMinMaxRect(float fontSize, float ascent, float descent) {
            return Rect.fromLTWH(fontSize * 0.05f, descent - fontSize, fontSize * 0.9f, fontSize * 0.9f);
        }

        public static Rect getUVRect(int code) {
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

        public static List<string> splitBySurrogatePair(string text) {
            int start = 0;
            bool? currentSurrogate = null;
            List<string> list = new List<string>();
            
            for (int i = 0; i < text.Length; i++) {
                if (i < text.Length - 1 && char.IsHighSurrogate(text[i]) && char.IsLowSurrogate(text[i + 1])) {
                    if (currentSurrogate != true) {
                        if (i > start) {
                            list.Add(text.Substring(start, i - start));
                            start = i;
                        }
                    }
                    i++;
                    currentSurrogate = true;
                }
                else {
                    if (currentSurrogate != false) {
                        if (i > start) {
                            list.Add(text.Substring(start, i - start));
                            start = i;
                        }
                    }
                    currentSurrogate = false;
                }
            }

            if (start < text.Length) {
                list.Add(text.Substring(start));
            }

            return list;
        }
    }
}