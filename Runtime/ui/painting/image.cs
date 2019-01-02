using System;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class Image : IEquatable<Image>, IDisposable {
        Texture _texture;
        readonly bool _isOwner;

        public Image(Texture texture, bool isOwner = true) {
            this._texture = texture;
            this._isOwner = isOwner;
        }

        public int width => this._texture != null ? this._texture.width : 0;

        public int height => this._texture != null ? this._texture.height : 0;

        public Texture texture => this._texture;

        ~Image() {
            Timer.runInMain(this._dispose);
        }

        void _dispose() {
            if (this._isOwner) {
                this._texture = ObjectUtils.SafeDestroy(this._texture);
            }
        }

        public void Dispose() {
            this._dispose();
            GC.SuppressFinalize(this);
        }

        public bool Equals(Image other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(this._texture, other._texture);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Image) obj);
        }

        public override int GetHashCode() {
            return (this._texture != null ? this._texture.GetHashCode() : 0);
        }

        public static bool operator ==(Image left, Image right) {
            return Equals(left, right);
        }

        public static bool operator !=(Image left, Image right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"[{this.width}\u00D7{this.height}]";
        }
    }
}