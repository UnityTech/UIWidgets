using System;
using UnityEngine;

namespace Unity.UIWidgets.editor {
    public class ScrollInput {
        readonly int _bufferSize = 10;
        readonly float _scrollScale = 10;

        float _scrollDeltaX;
        float _scrollDeltaY;

        int _bufferIndex;
        float _curDeltaX;
        float _curDeltaY;

        float _pointerX;
        float _pointerY;
        int _buttonId;

        public ScrollInput(int bufferSize = 10, float scrollScale = 10) {
            this._bufferIndex = bufferSize;
            this._bufferSize = bufferSize;
            this._scrollDeltaX = 0;
            this._scrollDeltaY = 0;
            this._curDeltaX = 0;
            this._curDeltaY = 0;
            this._scrollScale = scrollScale;
        }

        public void onScroll(float deltaX, float deltaY, float pointerX, float pointerY, int buttonId) {
            this._scrollDeltaX += deltaX * this._scrollScale;
            this._scrollDeltaY += deltaY * this._scrollScale;
            this._bufferIndex = this._bufferSize;
            this._curDeltaX = this._scrollDeltaX / this._bufferIndex;
            this._curDeltaY = this._scrollDeltaY / this._bufferIndex;

            this._pointerX = pointerX;
            this._pointerY = pointerY;
            this._buttonId = buttonId;
        }

        public int getDeviceId() {
            return this._buttonId;
        }

        public float getPointerPosX() {
            return this._pointerX;
        }

        public float getPointerPosY() {
            return this._pointerY;
        }

        public Vector2 getScrollDelta() {
            if (this._scrollDeltaX == 0 && this._scrollDeltaY == 0) {
                return Vector2.zero;
            }

            var deltaScroll = new Vector2();
            if (this._bufferIndex == 0) {
                deltaScroll.x = this._scrollDeltaX;
                deltaScroll.y = this._scrollDeltaY;
                this._scrollDeltaX = 0;
                this._scrollDeltaY = 0;
            }
            else {
                deltaScroll.x = this._curDeltaX;
                deltaScroll.y = this._curDeltaY;
                this._scrollDeltaX = this._curDeltaX > 0
                    ? Math.Max(0, this._scrollDeltaX - this._curDeltaX)
                    : Math.Min(0, this._scrollDeltaX - this._curDeltaX);
                this._scrollDeltaY = this._curDeltaY > 0
                    ? Math.Max(0, this._scrollDeltaY - this._curDeltaY)
                    : Math.Min(0, this._scrollDeltaY - this._curDeltaY);
                this._bufferIndex--;
            }

            return deltaScroll;
        }
    }
}