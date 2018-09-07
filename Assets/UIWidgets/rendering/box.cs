using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering {
    public class BoxConstraints : Constraints {
        public BoxConstraints(
            double minWidth = 0.0,
            double maxWidth = double.PositiveInfinity,
            double minHeight = 0.0,
            double maxHeight = double.PositiveInfinity) {
            this.minWidth = minWidth;
            this.maxWidth = maxWidth;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
        }

        public readonly double minWidth;
        public readonly double maxWidth;
        public readonly double minHeight;
        public readonly double maxHeight;

        public static BoxConstraints tight(Size size) {
            return new BoxConstraints(
                size.width,
                size.width,
                size.height,
                size.height
            );
        }

        public static BoxConstraints tightFor(double width, double height) {
            return new BoxConstraints(
                width,
                width,
                height,
                height
            );
        }

        public static BoxConstraints tightFor(
            double? width = null,
            double? height = null
        ) {
            return new BoxConstraints(
                width ?? 0.0,
                width ?? double.PositiveInfinity,
                height ?? 0.0,
                height ?? double.PositiveInfinity
            );
        }

        public static BoxConstraints tightForFinite(
            double width = double.PositiveInfinity,
            double height = double.PositiveInfinity
        ) {
            return new BoxConstraints(
                !double.IsPositiveInfinity(width) ? width : 0.0,
                !double.IsPositiveInfinity(width) ? width : double.PositiveInfinity,
                !double.IsPositiveInfinity(height) ? height : 0.0,
                !double.IsPositiveInfinity(height) ? height : double.PositiveInfinity
            );
        }

        public static BoxConstraints expand(
            double? width = null,
            double? height = null
        ) {
            return new BoxConstraints(
                width ?? double.PositiveInfinity,
                width ?? double.PositiveInfinity,
                height ?? double.PositiveInfinity,
                height ?? double.PositiveInfinity
            );
        }

        public BoxConstraints copyWith(
            double? minWidth = null,
            double? maxWidth = null,
            double? minHeight = null,
            double? maxHeight = null
        ) {
            return new BoxConstraints(
                minWidth ?? this.minWidth,
                maxWidth ?? this.maxWidth,
                minHeight ?? this.minHeight,
                maxHeight ?? this.maxHeight
            );
        }

        public BoxConstraints deflate(EdgeInsets edges) {
            double horizontal = edges.horizontal;
            double vertical = edges.vertical;
            double deflatedMinWidth = Math.Max(0.0, this.minWidth - horizontal);
            double deflatedMinHeight = Math.Max(0.0, this.minHeight - vertical);
            return new BoxConstraints(
                minWidth: deflatedMinWidth,
                maxWidth: Math.Max(deflatedMinWidth, this.maxWidth - horizontal),
                minHeight: deflatedMinHeight,
                maxHeight: Math.Max(deflatedMinHeight, this.maxHeight - vertical)
            );
        }

        public BoxConstraints loosen() {
            return new BoxConstraints(
                minWidth: 0.0,
                maxWidth: this.maxWidth,
                minHeight: 0.0,
                maxHeight: this.maxHeight
            );
        }

        public BoxConstraints enforce(BoxConstraints constraints) {
            return new BoxConstraints(
                minWidth: Mathf.Clamp(
                    (float) this.minWidth,
                    (float) constraints.minWidth,
                    (float) constraints.maxWidth),
                maxWidth: Mathf.Clamp(
                    (float) this.maxWidth,
                    (float) constraints.minWidth,
                    (float) constraints.maxWidth),
                minHeight: Mathf.Clamp(
                    (float) this.minHeight,
                    (float) constraints.minHeight,
                    (float) constraints.maxHeight),
                maxHeight: Mathf.Clamp(
                    (float) this.maxHeight,
                    (float) constraints.minHeight,
                    (float) constraints.maxHeight)
            );
        }

        public BoxConstraints tighten(
            double? width = null,
            double? height = null
        ) {
            return new BoxConstraints(
                minWidth: width == null
                    ? this.minWidth
                    : Mathf.Clamp((float) width.Value, (float) this.minWidth, (float) this.maxWidth),
                maxWidth: width == null
                    ? this.maxWidth
                    : Mathf.Clamp((float) width.Value, (float) this.minWidth, (float) this.maxWidth),
                minHeight: height == null
                    ? this.minHeight
                    : Mathf.Clamp((float) height.Value, (float) this.minHeight, (float) this.maxHeight),
                maxHeight: height == null
                    ? this.maxHeight
                    : Mathf.Clamp((float) height.Value, (float) this.minHeight, (float) this.maxHeight)
            );
        }

        public BoxConstraints flipped {
            get {
                return new BoxConstraints(
                    minWidth: this.minHeight,
                    maxWidth: this.maxHeight,
                    minHeight: this.minWidth,
                    maxHeight: this.maxWidth
                );
            }
        }

        public BoxConstraints widthConstraints() {
            return new BoxConstraints(minWidth: this.minWidth, maxWidth: this.maxWidth);
        }

        public BoxConstraints heightConstraints() {
            return new BoxConstraints(minHeight: this.minHeight, maxHeight: this.maxHeight);
        }

        public double constrainWidth(double width = double.PositiveInfinity) {
            return Mathf.Clamp((float) width, (float) this.minWidth, (float) this.maxWidth);
        }

        public double constrainHeight(double height = double.PositiveInfinity) {
            return Mathf.Clamp((float) height, (float) this.minHeight, (float) this.maxHeight);
        }

        public Size constrain(Size size) {
            return new Size(this.constrainWidth(size.width), this.constrainHeight(size.height));
        }

        public Size constrainDimensions(double width, double height) {
            return new Size(this.constrainWidth(width), this.constrainHeight(height));
        }

        public Size constrainSizeAndAttemptToPreserveAspectRatio(Size size) {
            if (this.isTight) {
                Size result = this.smallest;
                return result;
            }

            double width = size.width;
            double height = size.height;
            double aspectRatio = width / height;

            if (width > this.maxWidth) {
                width = this.maxWidth;
                height = width / aspectRatio;
            }

            if (height > this.maxHeight) {
                height = this.maxHeight;
                width = height * aspectRatio;
            }

            if (width < this.minWidth) {
                width = this.minWidth;
                height = width / aspectRatio;
            }

            if (height < this.minHeight) {
                height = this.minHeight;
                width = height * aspectRatio;
            }

            return new Size(this.constrainWidth(width), this.constrainHeight(height));
        }

        public Size biggest {
            get { return new Size(this.constrainWidth(), this.constrainHeight()); }
        }

        public Size smallest {
            get { return new Size(this.constrainWidth(0.0), this.constrainHeight(0.0)); }
        }

        public bool hasTightWidth {
            get { return this.minWidth >= this.maxWidth; }
        }

        public bool hasTightHeight {
            get { return this.minHeight >= this.maxHeight; }
        }

        public override bool isTight {
            get { return this.hasTightWidth && this.hasTightHeight; }
        }

        public bool hasBoundedWidth {
            get { return this.maxWidth < double.PositiveInfinity; }
        }

        public bool hasBoundedHeight {
            get { return this.maxHeight < double.PositiveInfinity; }
        }

        public bool hasInfiniteWidth {
            get { return this.minWidth >= double.PositiveInfinity; }
        }

        public bool hasInfiniteHeight {
            get { return this.minHeight >= double.PositiveInfinity; }
        }

        public bool isSatisfiedBy(Size size) {
            return this.minWidth <= size.width && size.width <= this.maxWidth &&
                   this.minHeight <= size.height && size.height <= this.maxHeight;
        }

        public override bool isNormalized {
            get {
                return this.minWidth >= 0.0 &&
                       this.minWidth <= this.maxWidth &&
                       this.minHeight >= 0.0 &&
                       this.minHeight <= this.maxHeight;
            }
        }

        public BoxConstraints normalize() {
            if (this.isNormalized) {
                return this;
            }

            var minWidth = this.minWidth >= 0.0 ? this.minWidth : 0.0;
            var minHeight = this.minHeight >= 0.0 ? this.minHeight : 0.0;

            return new BoxConstraints(
                minWidth,
                minWidth > this.maxWidth ? minWidth : this.maxWidth,
                minHeight,
                minHeight > this.maxHeight ? minHeight : this.maxHeight
            );
        }

        protected bool Equals(BoxConstraints other) {
            return this.minWidth.Equals(other.minWidth)
                   && this.maxWidth.Equals(other.maxWidth)
                   && this.minHeight.Equals(other.minHeight)
                   && this.maxHeight.Equals(other.maxHeight);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((BoxConstraints) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.minWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ this.maxWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ this.minHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ this.maxHeight.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BoxConstraints a, BoxConstraints b) {
            return object.Equals(a, b);
        }

        public static bool operator !=(BoxConstraints a, BoxConstraints b) {
            return !(a == b);
        }
    }

    public class BoxHitTestEntry : HitTestEntry {
        public BoxHitTestEntry(RenderBox target, Offset localPosition)
            : base(target) {
            D.assert(localPosition != null);
            this.localPosition = localPosition;
        }

        public new RenderBox target {
            get { return (RenderBox) base.target; }
        }

        public readonly Offset localPosition;

        public override string ToString() {
            return string.Format("{0}@{1}",
                Diagnostics.describeIdentity(this.target), this.localPosition);
        }
    }

    public class BoxParentData : ParentData {
        public Offset offset = Offset.zero;
    }

    public enum _IntrinsicDimension {
        minWidth,
        maxWidth,
        minHeight,
        maxHeight
    }

    public class _IntrinsicDimensionsCacheEntry : IEquatable<_IntrinsicDimensionsCacheEntry> {
        public _IntrinsicDimensionsCacheEntry(_IntrinsicDimension dimension, double argument) {
            this.dimension = dimension;
            this.argument = argument;
        }

        public readonly _IntrinsicDimension dimension;
        public readonly double argument;

        public bool Equals(_IntrinsicDimensionsCacheEntry other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.dimension == other.dimension && this.argument.Equals(other.argument);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((_IntrinsicDimensionsCacheEntry) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) this.dimension * 397) ^ this.argument.GetHashCode();
            }
        }

        public static bool operator ==(_IntrinsicDimensionsCacheEntry a, _IntrinsicDimensionsCacheEntry b) {
            return object.Equals(a, b);
        }

        public static bool operator !=(_IntrinsicDimensionsCacheEntry a, _IntrinsicDimensionsCacheEntry b) {
            return !(a == b);
        }
    }

    public abstract class RenderBox : RenderObject {
        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is BoxParentData)) {
                child.parentData = new BoxParentData();
            }
        }

        public Dictionary<_IntrinsicDimensionsCacheEntry, double> _cachedIntrinsicDimensions;

        public double _computeIntrinsicDimension(_IntrinsicDimension dimension, double argument,
            Func<double, double> computer) {
            if (this._cachedIntrinsicDimensions == null) {
                this._cachedIntrinsicDimensions = new Dictionary<_IntrinsicDimensionsCacheEntry, double>();
            }

            var key = new _IntrinsicDimensionsCacheEntry(dimension, argument);

            double result;
            if (this._cachedIntrinsicDimensions.TryGetValue(key, out result)) {
                return result;
            }

            return this._cachedIntrinsicDimensions[key] = computer(argument);
        }

        public double getMinIntrinsicWidth(double height) {
            return this._computeIntrinsicDimension(_IntrinsicDimension.minWidth, height, this.computeMinIntrinsicWidth);
        }

        public virtual double computeMinIntrinsicWidth(double height) {
            return 0.0;
        }

        public double getMaxIntrinsicWidth(double height) {
            return this._computeIntrinsicDimension(_IntrinsicDimension.maxWidth, height, this.computeMaxIntrinsicWidth);
        }

        public virtual double computeMaxIntrinsicWidth(double height) {
            return 0.0;
        }

        public double getMinIntrinsicHeight(double width) {
            return this._computeIntrinsicDimension(_IntrinsicDimension.minHeight, width,
                this.computeMinIntrinsicHeight);
        }

        public virtual double computeMinIntrinsicHeight(double width) {
            return 0.0;
        }

        public double getMaxIntrinsicHeight(double width) {
            return this._computeIntrinsicDimension(_IntrinsicDimension.maxHeight, width,
                this.computeMaxIntrinsicHeight);
        }

        public virtual double computeMaxIntrinsicHeight(double width) {
            return 0.0;
        }

        public bool hasSize {
            get { return this._size != null; }
        }

        public Size size {
            get { return this._size; }
            set { this._size = value; }
        }

        public Size _size;

        public Dictionary<TextBaseline, double?> _cachedBaselines;

        public double? getDistanceToBaseline(TextBaseline baseline, bool onlyReal = false) {
            double? result = this.getDistanceToActualBaseline(baseline);
            if (result == null && !onlyReal) {
                return this.size.height;
            }

            return result;
        }

        public double? getDistanceToActualBaseline(TextBaseline baseline) {
            if (this._cachedBaselines == null) {
                this._cachedBaselines = new Dictionary<TextBaseline, double?>();
            }

            double? result;
            if (this._cachedBaselines.TryGetValue(baseline, out result)) {
                return result;
            }

            return this._cachedBaselines[baseline] = this.computeDistanceToActualBaseline(baseline);
        }

        public virtual double? computeDistanceToActualBaseline(TextBaseline baseline) {
            return null;
        }

        public new BoxConstraints constraints {
            get { return (BoxConstraints) base.constraints; }
        }

        public override void markNeedsLayout() {
            if (this._cachedBaselines != null && this._cachedBaselines.Count > 0 ||
                this._cachedIntrinsicDimensions != null && this._cachedIntrinsicDimensions.Count > 0) {
                if (this._cachedBaselines != null) {
                    this._cachedBaselines.Clear();
                }

                if (this._cachedIntrinsicDimensions != null) {
                    this._cachedIntrinsicDimensions.Clear();
                }

                if (this.parent is RenderObject) {
                    this.markParentNeedsLayout();
                    return;
                }
            }

            base.markNeedsLayout();
        }


        public override void performResize() {
            this.size = this.constraints.smallest;
        }

        public override void performLayout() {
        }

        public virtual bool hitTest(HitTestResult result, Offset position) {
            D.assert(() => {
                if (!this.hasSize) {
                    throw new Exception("has no size during hitTest");
                }

                return true;
            });

            if (this._size.contains(position)) {
                if (this.hitTestChildren(result, position: position) || this.hitTestSelf(position)) {
                    result.add(new BoxHitTestEntry(this, position));
                    return true;
                }
            }

            return false;
        }

        protected virtual bool hitTestSelf(Offset position) {
            return false;
        }

        protected bool hitTestChildren(HitTestResult result, Offset position = null) {
            return false;
        }

        public override void applyPaintTransform(RenderObject child, ref Matrix4x4 transform) {
            var childParentData = (BoxParentData) child.parentData;
            var offset = childParentData.offset;
            transform = Matrix4x4.Translate(offset.toVector()) * transform;
        }

        public Offset globalToLocal(Offset point, RenderObject ancestor = null) {
            var transform = this.getTransformTo(ancestor);
            transform = transform.inverse;
            return MatrixUtils.transformPoint(transform, point);
        }

        public Offset localToGlobal(Offset point, RenderObject ancestor = null) {
            return MatrixUtils.transformPoint(this.getTransformTo(ancestor), point);
        }

        public override Rect paintBounds {
            get { return Offset.zero & this.size; }
        }
        
        int _debugActivePointers = 0;
        
        protected bool debugHandleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(()  =>{
                if (D.debugPaintPointersEnabled) {
                    if (evt is PointerDownEvent) {
                        this._debugActivePointers += 1;
                    } else if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                        this._debugActivePointers -= 1;
                    }
                    this.markNeedsPaint();
                }
                return true;
            });
            return true;
        }
        
        protected internal override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Size>("size", this._size, missingIfNull: true));
        }
    }

    public abstract class
        RenderBoxContainerDefaultsMixin<ChildType, ParentDataType>
        : ContainerRenderObjectMixinRenderBox<ChildType, ParentDataType>
        where ChildType : RenderBox
        where ParentDataType : ContainerParentDataMixinBoxParentData<ChildType> {
        public double? defaultComputeDistanceToFirstActualBaseline(TextBaseline baseline) {
            var child = this.firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                double? result = child.getDistanceToActualBaseline(baseline);
                if (result != null) {
                    return result.Value + childParentData.offset.dy;
                }

                child = childParentData.nextSibling;
            }

            return null;
        }

        public double? defaultComputeDistanceToHighestActualBaseline(TextBaseline baseline) {
            double? result = null;
            var child = this.firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                double? candidate = child.getDistanceToActualBaseline(baseline);
                if (candidate != null) {
                    candidate += childParentData.offset.dy;
                    if (result != null) {
                        result = Math.Min(result.Value, candidate.Value);
                    } else {
                        result = candidate;
                    }
                }

                child = childParentData.nextSibling;
            }

            return result;
        }

        public void defaultPaint(PaintingContext context, Offset offset) {
            var child = this.firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                context.paintChild(child, childParentData.offset + offset);
                child = childParentData.nextSibling;
            }
        }

        public List<ChildType> getChildrenAsList() {
            var result = new List<ChildType>();
            var child = this.firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                result.Add(child);
                child = childParentData.nextSibling;
            }

            return result;
        }
    }
}