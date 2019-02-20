using System;
using UnityEngine;

namespace Unity.UIWidgets.editor {
    public class ScrollInput {
        float scrollDeltaX;
        float scrollDeltaY;

        readonly int bufferSize;
        readonly float scrollScale = 10;

        int bufferIndex = 10;
        float CurDeltaX;
        float CurDeltaY;

        float PointerX;
        float PointerY;
        int buttonId;

        public ScrollInput(int bufferSize = 10, float scrollScale = 10) {
            this.bufferIndex = bufferSize;
            this.bufferSize = bufferSize;
            this.scrollDeltaX = 0;
            this.scrollDeltaY = 0;
            this.CurDeltaX = 0;
            this.CurDeltaY = 0;
            this.scrollScale = scrollScale;
        }

        public void OnScroll(float deltaX, float deltaY, float PointerX, float PointerY, int buttonId) {
            this.scrollDeltaX += deltaX * this.scrollScale;
            this.scrollDeltaY += deltaY * this.scrollScale;
            this.bufferIndex = this.bufferSize;
            this.CurDeltaX = this.scrollDeltaX / this.bufferIndex;
            this.CurDeltaY = this.scrollDeltaY / this.bufferIndex;

            this.PointerX = PointerX;
            this.PointerY = PointerY;
            this.buttonId = buttonId;
        }

        public int GetDeviceId() {
            return this.buttonId;
        }

        public float GetPointerPosX() {
            return this.PointerX;
        }

        public float GetPointerPosY() {
            return this.PointerY;
        }

        public Vector2 GetScrollDelta() {
            if (this.scrollDeltaX == 0 && this.scrollDeltaY == 0) {
                return Vector2.zero;
            }

            var deltaScroll = new Vector2();
            if (this.bufferIndex == 0) {
                deltaScroll.x = this.scrollDeltaX;
                deltaScroll.y = this.scrollDeltaY;
                this.scrollDeltaX = 0;
                this.scrollDeltaY = 0;
            }
            else {
                deltaScroll.x = this.CurDeltaX;
                deltaScroll.y = this.CurDeltaY;
                this.scrollDeltaX = this.CurDeltaX > 0
                    ? Math.Max(0, this.scrollDeltaX - this.CurDeltaX)
                    : Math.Min(0, this.scrollDeltaX - this.CurDeltaX);
                this.scrollDeltaY = this.CurDeltaY > 0
                    ? Math.Max(0, this.scrollDeltaY - this.CurDeltaY)
                    : Math.Min(0, this.scrollDeltaY - this.CurDeltaY);
                this.bufferIndex--;
            }

            return deltaScroll;
        }
    }
}