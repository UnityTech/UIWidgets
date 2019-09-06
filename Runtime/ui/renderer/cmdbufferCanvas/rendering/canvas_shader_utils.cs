using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class MaterialPropertyBlockWrapper : PoolObject {
        public readonly MaterialPropertyBlock mpb;

        public MaterialPropertyBlockWrapper() {
            this.mpb = new MaterialPropertyBlock();
        }

        public override void clear() {
            this.mpb.Clear();
        }

        public void SetVector(int mid, Vector4 vec) {
            this.mpb.SetVector(mid, vec);
        }

        public void SetFloat(int mid, float value) {
            this.mpb.SetFloat(mid, value);
        }

        public void SetMatrix(int mid, Matrix4x4 mat) {
            this.mpb.SetMatrix(mid, mat);
        }

        public void SetTexture(int mid, Texture texture) {
            this.mpb.SetTexture(mid, texture);
        }

        public void SetInt(int mid, int value) {
            this.mpb.SetInt(mid, value);
        }

        public void SetFloatArray(int mid, float[] array) {
            this.mpb.SetFloatArray(mid, array);
        }

        public void SetBuffer(int mid, ComputeBuffer buffer) {
            this.mpb.SetBuffer(mid, buffer);
        }
    }
}