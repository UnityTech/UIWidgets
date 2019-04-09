using UnityEngine;

namespace Unity.UIWidgets.editor {
    public class ScrollInput {
        readonly float _bufferSize = 20.0f / 60;            // a scroll action leads to 20 frames, i.e., ( 20 / 60 ) seconds smoothly-scrolling when fps = 60 (default)
        readonly float _scrollScale = 20;

        float _scrollDeltaX;
        float _scrollDeltaY;

        float _bufferIndex;
        float _curDeltaX;
        float _curDeltaY;

        float _pointerX;
        float _pointerY;
        int _buttonId;

        public ScrollInput(float? bufferSize = null, float? scrollScale = null) {
            this._bufferSize = bufferSize ?? this._bufferSize;
            this._scrollScale = scrollScale ?? this._scrollScale;

            this._bufferIndex = this._bufferSize;
            this._scrollDeltaX = 0;
            this._scrollDeltaY = 0;
            this._curDeltaX = 0;
            this._curDeltaY = 0;
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

        public Vector2 getScrollDelta(float deltaTime) {
            if (this._scrollDeltaX == 0 && this._scrollDeltaY == 0) {
                return Vector2.zero;
            }

            var deltaScroll = new Vector2();
            if (this._bufferIndex <= deltaTime) {
                deltaScroll.x = this._scrollDeltaX;
                deltaScroll.y = this._scrollDeltaY;
                this._scrollDeltaX = 0;
                this._scrollDeltaY = 0;
            }
            else {
                deltaScroll.x = this._curDeltaX * deltaTime;
                deltaScroll.y = this._curDeltaY * deltaTime;
                this._scrollDeltaX = this._curDeltaX > 0
                    ? Mathf.Max(0, this._scrollDeltaX - this._curDeltaX * deltaTime)
                    : Mathf.Min(0, this._scrollDeltaX - this._curDeltaX * deltaTime);
                this._scrollDeltaY = this._curDeltaY > 0
                    ? Mathf.Max(0, this._scrollDeltaY - this._curDeltaY * deltaTime)
                    : Mathf.Min(0, this._scrollDeltaY - this._curDeltaY * deltaTime);
                this._bufferIndex -= deltaTime;
            }
            
            return deltaScroll;
        }
    }
}