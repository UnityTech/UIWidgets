using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    class _DebugSize : Size {
        internal _DebugSize(Size source, RenderBox _owner, bool _canBeUsedByParent) :
            base(source.width, source.height) {
            this._owner = _owner;
            this._canBeUsedByParent = _canBeUsedByParent;
        }

        internal readonly RenderBox _owner;
        internal readonly bool _canBeUsedByParent;
    }

    public class BoxConstraints : Constraints, IEquatable<BoxConstraints> {
        public BoxConstraints(
            float minWidth = 0.0f,
            float maxWidth = float.PositiveInfinity,
            float minHeight = 0.0f,
            float maxHeight = float.PositiveInfinity) {
            this.minWidth = minWidth;
            this.maxWidth = maxWidth;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
        }

        public readonly float minWidth;
        public readonly float maxWidth;
        public readonly float minHeight;
        public readonly float maxHeight;

        public static BoxConstraints tight(Size size) {
            return new BoxConstraints(
                size.width,
                size.width,
                size.height,
                size.height
            );
        }

        public static BoxConstraints tightFor(
            float? width = null,
            float? height = null
        ) {
            return new BoxConstraints(
                width ?? 0.0f,
                width ?? float.PositiveInfinity,
                height ?? 0.0f,
                height ?? float.PositiveInfinity
            );
        }

        public static BoxConstraints tightForFinite(
            float width = float.PositiveInfinity,
            float height = float.PositiveInfinity
        ) {
            return new BoxConstraints(
                !float.IsPositiveInfinity(width) ? width : 0.0f,
                !float.IsPositiveInfinity(width) ? width : float.PositiveInfinity,
                !float.IsPositiveInfinity(height) ? height : 0.0f,
                !float.IsPositiveInfinity(height) ? height : float.PositiveInfinity
            );
        }

        public static BoxConstraints loose(Size size) {
            return new BoxConstraints(
                minWidth: 0,
                maxWidth: size.width,
                minHeight: 0,
                maxHeight: size.height
            );
        }

        public static BoxConstraints expand(
            float? width = null,
            float? height = null
        ) {
            return new BoxConstraints(
                width ?? float.PositiveInfinity,
                width ?? float.PositiveInfinity,
                height ?? float.PositiveInfinity,
                height ?? float.PositiveInfinity
            );
        }

        public BoxConstraints copyWith(
            float? minWidth = null,
            float? maxWidth = null,
            float? minHeight = null,
            float? maxHeight = null
        ) {
            return new BoxConstraints(
                minWidth ?? this.minWidth,
                maxWidth ?? this.maxWidth,
                minHeight ?? this.minHeight,
                maxHeight ?? this.maxHeight
            );
        }

        public BoxConstraints deflate(EdgeInsets edges) {
            D.assert(edges != null);
            D.assert(this.debugAssertIsValid());
            float horizontal = edges.horizontal;
            float vertical = edges.vertical;
            float deflatedMinWidth = Mathf.Max(0.0f, this.minWidth - horizontal);
            float deflatedMinHeight = Mathf.Max(0.0f, this.minHeight - vertical);
            return new BoxConstraints(
                minWidth: deflatedMinWidth,
                maxWidth: Mathf.Max(deflatedMinWidth, this.maxWidth - horizontal),
                minHeight: deflatedMinHeight,
                maxHeight: Mathf.Max(deflatedMinHeight, this.maxHeight - vertical)
            );
        }

        public BoxConstraints loosen() {
            D.assert(this.debugAssertIsValid());
            return new BoxConstraints(
                minWidth: 0.0f,
                maxWidth: this.maxWidth,
                minHeight: 0.0f,
                maxHeight: this.maxHeight
            );
        }

        public BoxConstraints enforce(BoxConstraints constraints) {
            return new BoxConstraints(
                minWidth: this.minWidth.clamp(constraints.minWidth, constraints.maxWidth),
                maxWidth: this.maxWidth.clamp(constraints.minWidth, constraints.maxWidth),
                minHeight: this.minHeight.clamp(constraints.minHeight, constraints.maxHeight),
                maxHeight: this.maxHeight.clamp(constraints.minHeight, constraints.maxHeight)
            );
        }

        public BoxConstraints tighten(
            float? width = null,
            float? height = null
        ) {
            return new BoxConstraints(
                minWidth: width == null ? this.minWidth : width.Value.clamp(this.minWidth, this.maxWidth),
                maxWidth: width == null ? this.maxWidth : width.Value.clamp(this.minWidth, this.maxWidth),
                minHeight: height == null ? this.minHeight : height.Value.clamp(this.minHeight, this.maxHeight),
                maxHeight: height == null ? this.maxHeight : height.Value.clamp(this.minHeight, this.maxHeight)
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

        public float constrainWidth(float width = float.PositiveInfinity) {
            D.assert(this.debugAssertIsValid());
            return width.clamp(this.minWidth, this.maxWidth);
        }

        public float constrainHeight(float height = float.PositiveInfinity) {
            D.assert(this.debugAssertIsValid());
            return height.clamp(this.minHeight, this.maxHeight);
        }

        Size _debugPropagateDebugSize(Size size, Size result) {
            D.assert(() => {
                if (size is _DebugSize) {
                    result = new _DebugSize(result,
                        ((_DebugSize) size)._owner, ((_DebugSize) size)._canBeUsedByParent);
                }

                return true;
            });
            return result;
        }

        public Size constrain(Size size) {
            var result = new Size(this.constrainWidth(size.width), this.constrainHeight(size.height));
            D.assert(() => {
                result = this._debugPropagateDebugSize(size, result);
                return true;
            });

            return result;
        }

        public Size constrainDimensions(float width, float height) {
            return new Size(this.constrainWidth(width), this.constrainHeight(height));
        }

        public Size constrainSizeAndAttemptToPreserveAspectRatio(Size size) {
            if (this.isTight) {
                Size result1 = this.smallest;
                D.assert(() => {
                    result1 = this._debugPropagateDebugSize(size, result1);
                    return true;
                });
                return result1;
            }

            float width = size.width;
            float height = size.height;
            D.assert(width > 0.0);
            D.assert(height > 0.0);
            float aspectRatio = width / height;

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

            var result = new Size(this.constrainWidth(width), this.constrainHeight(height));
            D.assert(() => {
                result = this._debugPropagateDebugSize(size, result);
                return true;
            });
            return result;
        }

        public Size biggest {
            get { return new Size(this.constrainWidth(), this.constrainHeight()); }
        }

        public Size smallest {
            get { return new Size(this.constrainWidth(0.0f), this.constrainHeight(0.0f)); }
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
            get { return this.maxWidth < float.PositiveInfinity; }
        }

        public bool hasBoundedHeight {
            get { return this.maxHeight < float.PositiveInfinity; }
        }

        public bool hasInfiniteWidth {
            get { return this.minWidth >= float.PositiveInfinity; }
        }

        public bool hasInfiniteHeight {
            get { return this.minHeight >= float.PositiveInfinity; }
        }

        public bool isSatisfiedBy(Size size) {
            D.assert(this.debugAssertIsValid());
            return this.minWidth <= size.width && size.width <= this.maxWidth &&
                   this.minHeight <= size.height && size.height <= this.maxHeight;
        }

        public static BoxConstraints operator *(BoxConstraints it, float factor) {
            return new BoxConstraints(
                minWidth: it.minWidth * factor,
                maxWidth: it.maxWidth * factor,
                minHeight: it.minHeight * factor,
                maxHeight: it.maxHeight * factor
            );
        }

        public static BoxConstraints operator /(BoxConstraints it, float factor) {
            return new BoxConstraints(
                minWidth: it.minWidth / factor,
                maxWidth: it.maxWidth / factor,
                minHeight: it.minHeight / factor,
                maxHeight: it.maxHeight / factor
            );
        }

        public static BoxConstraints operator %(BoxConstraints it, float value) {
            return new BoxConstraints(
                minWidth: it.minWidth % value,
                maxWidth: it.maxWidth % value,
                minHeight: it.minHeight % value,
                maxHeight: it.maxHeight % value
            );
        }

        public static BoxConstraints lerp(BoxConstraints a, BoxConstraints b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b * t;
            }

            if (b == null) {
                return a * (1.0f - t);
            }

            D.assert(a.debugAssertIsValid());
            D.assert(b.debugAssertIsValid());
            D.assert(
                (a.minWidth.isFinite() && b.minWidth.isFinite()) ||
                (a.minWidth == float.PositiveInfinity && b.minWidth == float.PositiveInfinity),
                () => "Cannot interpolate between finite constraints and unbounded constraints.");
            D.assert(
                (a.maxWidth.isFinite() && b.maxWidth.isFinite()) ||
                (a.maxWidth == float.PositiveInfinity && b.maxWidth == float.PositiveInfinity),
                () => "Cannot interpolate between finite constraints and unbounded constraints.");
            D.assert(
                (a.minHeight.isFinite() && b.minHeight.isFinite()) ||
                (a.minHeight == float.PositiveInfinity && b.minHeight == float.PositiveInfinity),
                () => "Cannot interpolate between finite constraints and unbounded constraints.");
            D.assert(
                (a.maxHeight.isFinite() && b.maxHeight.isFinite()) ||
                (a.maxHeight == float.PositiveInfinity && b.maxHeight == float.PositiveInfinity),
                () => "Cannot interpolate between finite constraints and unbounded constraints.");
            return new BoxConstraints(
                minWidth: a.minWidth.isFinite()
                    ? MathUtils.lerpFloat(a.minWidth, b.minWidth, t)
                    : float.PositiveInfinity,
                maxWidth: a.maxWidth.isFinite()
                    ? MathUtils.lerpFloat(a.maxWidth, b.maxWidth, t)
                    : float.PositiveInfinity,
                minHeight: a.minHeight.isFinite()
                    ? MathUtils.lerpFloat(a.minHeight, b.minHeight, t)
                    : float.PositiveInfinity,
                maxHeight: a.maxHeight.isFinite()
                    ? MathUtils.lerpFloat(a.maxHeight, b.maxHeight, t)
                    : float.PositiveInfinity
            );
        }

        public override bool isNormalized {
            get {
                return this.minWidth >= 0.0 &&
                       this.minWidth <= this.maxWidth &&
                       this.minHeight >= 0.0 &&
                       this.minHeight <= this.maxHeight;
            }
        }

        public override bool debugAssertIsValid(
            bool isAppliedConstraint = false,
            InformationCollector informationCollector = null
        ) {
            D.assert(() => {
                var throwError = new Action<string>(message => {
                    var information = new StringBuilder();
                    if (informationCollector != null) {
                        informationCollector(information);
                    }

                    throw new UIWidgetsError($"{message}\n{information}The offending constraints were:\n  {this}");
                });

                if (this.minWidth.isNaN() ||
                    this.maxWidth.isNaN() ||
                    this.minHeight.isNaN() ||
                    this.maxHeight.isNaN()) {
                    var affectedFieldsList = new List<string>();
                    if (this.minWidth.isNaN()) {
                        affectedFieldsList.Add("minWidth");
                    }

                    if (this.maxWidth.isNaN()) {
                        affectedFieldsList.Add("maxWidth");
                    }

                    if (this.minHeight.isNaN()) {
                        affectedFieldsList.Add("minHeight");
                    }

                    if (this.maxHeight.isNaN()) {
                        affectedFieldsList.Add("maxHeight");
                    }

                    D.assert(affectedFieldsList.isNotEmpty());
                    if (affectedFieldsList.Count > 1) {
                        var last = affectedFieldsList.Last();
                        affectedFieldsList.RemoveAt(affectedFieldsList.Count - 1);
                        affectedFieldsList.Add("and " + last);
                    }

                    string whichFields;
                    if (affectedFieldsList.Count > 2) {
                        whichFields = string.Join(", ", affectedFieldsList.ToArray());
                    }
                    else if (affectedFieldsList.Count == 2) {
                        whichFields = string.Join(" ", affectedFieldsList.ToArray());
                    }
                    else {
                        whichFields = affectedFieldsList.Single();
                    }

                    throwError("BoxConstraints has NaN values in " + whichFields + ".");
                }

                if (this.minWidth < 0.0 && this.minHeight < 0.0) {
                    throwError("BoxConstraints has both a negative minimum width and a negative minimum height.");
                }

                if (this.minWidth < 0.0) {
                    throwError("BoxConstraints has a negative minimum width.");
                }

                if (this.minHeight < 0.0) {
                    throwError("BoxConstraints has a negative minimum height.");
                }

                if (this.maxWidth < this.minWidth && this.maxHeight < this.minHeight) {
                    throwError("BoxConstraints has both width and height constraints non-normalized.");
                }

                if (this.maxWidth < this.minWidth) {
                    throwError("BoxConstraints has non-normalized width constraints.");
                }

                if (this.maxHeight < this.minHeight) {
                    throwError("BoxConstraints has non-normalized height constraints.");
                }

                if (isAppliedConstraint) {
                    if (this.minWidth.isInfinite() && this.minHeight.isInfinite()) {
                        throwError("BoxConstraints forces an infinite width and infinite height.");
                    }

                    if (this.minWidth.isInfinite()) {
                        throwError("BoxConstraints forces an infinite width.");
                    }

                    if (this.minHeight.isInfinite()) {
                        throwError("BoxConstraints forces an infinite height.");
                    }
                }

                D.assert(this.isNormalized);
                return true;
            });
            return this.isNormalized;
        }

        public BoxConstraints normalize() {
            if (this.isNormalized) {
                return this;
            }

            var minWidth = this.minWidth >= 0.0 ? this.minWidth : 0.0f;
            var minHeight = this.minHeight >= 0.0 ? this.minHeight : 0.0f;

            return new BoxConstraints(
                minWidth,
                minWidth > this.maxWidth ? minWidth : this.maxWidth,
                minHeight,
                minHeight > this.maxHeight ? minHeight : this.maxHeight
            );
        }

        public bool Equals(BoxConstraints other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.minWidth.Equals(other.minWidth)
                   && this.maxWidth.Equals(other.maxWidth)
                   && this.minHeight.Equals(other.minHeight)
                   && this.maxHeight.Equals(other.maxHeight);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

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

        public static bool operator ==(BoxConstraints left, BoxConstraints right) {
            return Equals(left, right);
        }

        public static bool operator !=(BoxConstraints left, BoxConstraints right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            string annotation = this.isNormalized ? "" : "; NOT NORMALIZED";
            if (this.minWidth == float.PositiveInfinity &&
                this.minHeight == float.PositiveInfinity) {
                return "BoxConstraints(biggest" + annotation + ")";
            }

            if (this.minWidth == 0 && this.maxWidth == float.PositiveInfinity &&
                this.minHeight == 0 && this.maxHeight == float.PositiveInfinity) {
                return "BoxConstraints(unconstrained" + annotation + ")";
            }

            var describe = new Func<float, float, string, string>((min, max, dim) => {
                if (min == max) {
                    return dim + "=" + min.ToString("F1");
                }

                return min.ToString("F1") + "<=" + dim + "<=" + max.ToString("F1");
            });

            string width = describe(this.minWidth, this.maxWidth, "w");
            string height = describe(this.minHeight, this.maxHeight, "h");
            return "BoxConstraints(" + width + ", " + height + annotation + ")";
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
            return $"{Diagnostics.describeIdentity(this.target)}@{this.localPosition}";
        }
    }

    public class BoxParentData : ParentData {
        public Offset offset = Offset.zero;

        public override string ToString() {
            return "offset=" + this.offset;
        }
    }

    enum _IntrinsicDimension {
        minWidth,
        maxWidth,
        minHeight,
        maxHeight
    }

    class _IntrinsicDimensionsCacheEntry : IEquatable<_IntrinsicDimensionsCacheEntry> {
        internal _IntrinsicDimensionsCacheEntry(_IntrinsicDimension dimension, float argument) {
            this.dimension = dimension;
            this.argument = argument;
        }

        public readonly _IntrinsicDimension dimension;
        public readonly float argument;

        public bool Equals(_IntrinsicDimensionsCacheEntry other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.dimension == other.dimension && this.argument.Equals(other.argument);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((_IntrinsicDimensionsCacheEntry) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) this.dimension * 397) ^ this.argument.GetHashCode();
            }
        }

        public static bool operator ==(_IntrinsicDimensionsCacheEntry a, _IntrinsicDimensionsCacheEntry b) {
            return Equals(a, b);
        }

        public static bool operator !=(_IntrinsicDimensionsCacheEntry a, _IntrinsicDimensionsCacheEntry b) {
            return !Equals(a, b);
        }
    }

    public abstract class RenderBox : RenderObject {
        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is BoxParentData)) {
                child.parentData = new BoxParentData();
            }
        }

        Dictionary<_IntrinsicDimensionsCacheEntry, float> _cachedIntrinsicDimensions;

        float _computeIntrinsicDimension(_IntrinsicDimension dimension, float argument,
            Func<float, float> computer) {
            D.assert(debugCheckingIntrinsics || !this.debugDoingThisResize);
            bool shouldCache = true;
            D.assert(() => {
                if (debugCheckingIntrinsics) {
                    shouldCache = false;
                }

                return true;
            });

            if (shouldCache) {
                this._cachedIntrinsicDimensions =
                    this._cachedIntrinsicDimensions
                    ?? new Dictionary<_IntrinsicDimensionsCacheEntry, float>();
                return this._cachedIntrinsicDimensions.putIfAbsent(
                    new _IntrinsicDimensionsCacheEntry(dimension, argument),
                    () => computer(argument));
            }

            return computer(argument);
        }

        public float getMinIntrinsicWidth(float height) {
            D.assert(() => {
                if (height < 0.0) {
                    throw new UIWidgetsError(
                        "The height argument to getMinIntrinsicWidth was negative.\n" +
                        "The argument to getMinIntrinsicWidth must not be negative. " +
                        "If you perform computations on another height before passing it to " +
                        "getMinIntrinsicWidth, consider using Mathf.Max() or float.clamp() " +
                        "to force the value into the valid range."
                    );
                }

                return true;
            });

            return this._computeIntrinsicDimension(_IntrinsicDimension.minWidth, height, this.computeMinIntrinsicWidth);
        }

        protected virtual float computeMinIntrinsicWidth(float height) {
            return 0.0f;
        }

        public float getMaxIntrinsicWidth(float height) {
            D.assert(() => {
                if (height < 0.0) {
                    throw new UIWidgetsError(
                        "The height argument to getMaxIntrinsicWidth was negative.\n" +
                        "The argument to getMaxIntrinsicWidth must not be negative. " +
                        "If you perform computations on another height before passing it to " +
                        "getMaxIntrinsicWidth, consider using Mathf.Max() or float.clamp() " +
                        "to force the value into the valid range."
                    );
                }

                return true;
            });

            return this._computeIntrinsicDimension(_IntrinsicDimension.maxWidth, height, this.computeMaxIntrinsicWidth);
        }

        protected virtual float computeMaxIntrinsicWidth(float height) {
            return 0.0f;
        }

        public float getMinIntrinsicHeight(float width) {
            D.assert(() => {
                if (width < 0.0) {
                    throw new UIWidgetsError(
                        "The width argument to getMinIntrinsicHeight was negative.\n" +
                        "The argument to getMinIntrinsicHeight must not be negative. " +
                        "If you perform computations on another width before passing it to " +
                        "getMinIntrinsicHeight, consider using Mathf.Max() or float.clamp() " +
                        "to force the value into the valid range."
                    );
                }

                return true;
            });

            return this._computeIntrinsicDimension(_IntrinsicDimension.minHeight, width,
                this.computeMinIntrinsicHeight);
        }

        protected virtual float computeMinIntrinsicHeight(float width) {
            return 0.0f;
        }

        public float getMaxIntrinsicHeight(float width) {
            D.assert(() => {
                if (width < 0.0) {
                    throw new UIWidgetsError(
                        "The width argument to getMaxIntrinsicHeight was negative.\n" +
                        "The argument to getMaxIntrinsicHeight must not be negative. " +
                        "If you perform computations on another width before passing it to " +
                        "getMaxIntrinsicHeight, consider using Mathf.Max() or float.clamp() " +
                        "to force the value into the valid range."
                    );
                }

                return true;
            });

            return this._computeIntrinsicDimension(_IntrinsicDimension.maxHeight, width,
                this.computeMaxIntrinsicHeight);
        }

        protected internal virtual float computeMaxIntrinsicHeight(float width) {
            return 0.0f;
        }

        public bool hasSize {
            get { return this._size != null; }
        }

        public Size size {
            get {
                D.assert(this.hasSize, () => "RenderBox was not laid out: " + this);
                D.assert(() => {
                    if (this._size is _DebugSize) {
                        _DebugSize _size = (_DebugSize) this._size;
                        D.assert(_size._owner == this);
                        if (debugActiveLayout != null) {
                            D.assert(this.debugDoingThisResize || this.debugDoingThisLayout ||
                                     (debugActiveLayout == this.parent && _size._canBeUsedByParent));
                        }

                        D.assert(_size == this._size);
                    }

                    return true;
                });

                return this._size;
            }
            set {
                D.assert(!(this.debugDoingThisResize && this.debugDoingThisLayout));
                D.assert(this.sizedByParent || !this.debugDoingThisResize);
                D.assert(() => {
                    if ((this.sizedByParent && this.debugDoingThisResize) ||
                        (!this.sizedByParent && this.debugDoingThisLayout)) {
                        return true;
                    }

                    D.assert(!this.debugDoingThisResize);

                    string contract = "", violation = "", hint = "";
                    if (this.debugDoingThisLayout) {
                        D.assert(this.sizedByParent);
                        violation = "It appears that the size setter was called from performLayout().";
                        hint = "";
                    }
                    else {
                        violation =
                            "The size setter was called from outside layout (neither performResize() nor performLayout() were being run for this object).";
                        if (this.owner != null && this.owner.debugDoingLayout) {
                            hint =
                                "Only the object itself can set its size. It is a contract violation for other objects to set it.";
                        }
                    }

                    if (this.sizedByParent) {
                        contract =
                            "Because this RenderBox has sizedByParent set to true, it must set its size in performResize().";
                    }
                    else {
                        contract =
                            "Because this RenderBox has sizedByParent set to false, it must set its size in performLayout().";
                    }

                    throw new UIWidgetsError(
                        "RenderBox size setter called incorrectly.\n" +
                        violation + "\n" +
                        hint + "\n" +
                        contract + "\n" +
                        "The RenderBox in question is:\n" +
                        "  " + this
                    );
                });
                D.assert(() => {
                    value = this.debugAdoptSize(value);
                    return true;
                });

                this._size = value;

                D.assert(() => {
                    this.debugAssertDoesMeetConstraints();
                    return true;
                });
            }
        }

        Size _size;

        public Size debugAdoptSize(Size valueRaw) {
            Size result = valueRaw;
            D.assert(() => {
                if (valueRaw is _DebugSize) {
                    var value = (_DebugSize) valueRaw;
                    if (value._owner != this) {
                        if (value._owner.parent != this) {
                            throw new UIWidgetsError(
                                "The size property was assigned a size inappropriately.\n" +
                                "The following render object:\n" +
                                "  " + this + "\n" +
                                "...was assigned a size obtained from:\n" +
                                "  " + value._owner + "\n" +
                                "However, this second render object is not, or is no longer, a " +
                                "child of the first, and it is therefore a violation of the " +
                                "RenderBox layout protocol to use that size in the layout of the " +
                                "first render object.\n" +
                                "If the size was obtained at a time where it was valid to read " +
                                "the size (because the second render object above was a child " +
                                "of the first at the time), then it should be adopted using " +
                                "debugAdoptSize at that time.\n" +
                                "If the size comes from a grandchild or a render object from an " +
                                "entirely different part of the render tree, then there is no " +
                                "way to be notified when the size changes and therefore attempts " +
                                "to read that size are almost certainly a source of bugs. A different " +
                                "approach should be used."
                            );
                        }

                        if (!value._canBeUsedByParent) {
                            throw new UIWidgetsError(
                                "A child\"s size was used without setting parentUsesSize.\n" +
                                "The following render object:\n" +
                                "  " + this + "\n" +
                                "...was assigned a size obtained from its child:\n" +
                                "  " + value._owner + "\n" +
                                "However, when the child was laid out, the parentUsesSize argument " +
                                "was not set or set to false. Subsequently this transpired to be " +
                                "inaccurate: the size was nonetheless used by the parent.\n" +
                                "It is important to tell the framework if the size will be used or not " +
                                "as several important performance optimizations can be made if the " +
                                "size will not be used by the parent."
                            );
                        }
                    }
                }

                result = new _DebugSize(valueRaw, this, this.debugCanParentUseSize);
                return true;
            });
            return result;
        }

        public override Rect semanticBounds {
            get { return Offset.zero & this.size; }
        }

        protected override void debugResetSize() {
            this.size = this.size;
        }

        Dictionary<TextBaseline, float?> _cachedBaselines;
        static bool _debugDoingBaseline = false;

        static bool _debugSetDoingBaseline(bool value) {
            _debugDoingBaseline = value;
            return true;
        }

        public float? getDistanceToBaseline(TextBaseline baseline, bool onlyReal = false) {
            D.assert(!_debugDoingBaseline,
                () => "Please see the documentation for computeDistanceToActualBaseline for the required calling conventions of this method.");
            D.assert(!this.debugNeedsLayout);
            D.assert(() => {
                RenderObject parent = (RenderObject) this.parent;
                if (this.owner.debugDoingLayout) {
                    return (debugActiveLayout == parent) && parent.debugDoingThisLayout;
                }

                if (this.owner.debugDoingPaint) {
                    return ((debugActivePaint == parent) && parent.debugDoingThisPaint) ||
                           ((debugActivePaint == this) && this.debugDoingThisPaint);
                }

                D.assert(parent == this.parent);
                return false;
            });

            D.assert(_debugSetDoingBaseline(true));
            float? result = this.getDistanceToActualBaseline(baseline);
            D.assert(_debugSetDoingBaseline(false));

            if (result == null && !onlyReal) {
                return this.size.height;
            }

            return result;
        }

        public virtual float? getDistanceToActualBaseline(TextBaseline baseline) {
            D.assert(_debugDoingBaseline,
                () => "Please see the documentation for computeDistanceToActualBaseline for the required calling conventions of this method.");

            this._cachedBaselines = this._cachedBaselines ?? new Dictionary<TextBaseline, float?>();
            return this._cachedBaselines.putIfAbsent(baseline, () => this.computeDistanceToActualBaseline(baseline));
        }

        protected virtual float? computeDistanceToActualBaseline(TextBaseline baseline) {
            D.assert(_debugDoingBaseline,
                () => "Please see the documentation for computeDistanceToActualBaseline for the required calling conventions of this method.");

            return null;
        }

        public new BoxConstraints constraints {
            get { return (BoxConstraints) base.constraints; }
        }

        protected override void debugAssertDoesMeetConstraints() {
            D.assert(this.constraints != null);
            D.assert(() => {
                if (!this.hasSize) {
                    D.assert(!this.debugNeedsLayout);
                    string contract = "";
                    if (this.sizedByParent) {
                        contract =
                            "Because this RenderBox has sizedByParent set to true, it must set its size in performResize().\n";
                    }
                    else {
                        contract =
                            "Because this RenderBox has sizedByParent set to false, it must set its size in performLayout().\n";
                    }

                    throw new UIWidgetsError(
                        "RenderBox did not set its size during layout.\n" +
                        contract +
                        "It appears that this did not happen; layout completed, but the size property is still null.\n" +
                        "The RenderBox in question is:\n" +
                        "  " + this);
                }

                if (!this._size.isFinite) {
                    var information = new StringBuilder();
                    if (!this.constraints.hasBoundedWidth) {
                        RenderBox node = this;
                        while (!node.constraints.hasBoundedWidth && node.parent is RenderBox) {
                            node = (RenderBox) node.parent;
                        }

                        information.AppendLine("The nearest ancestor providing an unbounded width constraint is:");
                        information.Append("  ");
                        information.AppendLine(node.toStringShallow(joiner: "\n  "));
                    }

                    if (!this.constraints.hasBoundedHeight) {
                        RenderBox node = this;
                        while (!node.constraints.hasBoundedHeight && node.parent is RenderBox) {
                            node = (RenderBox) node.parent;
                        }

                        information.AppendLine("The nearest ancestor providing an unbounded height constraint is:");
                        information.Append("  ");
                        information.AppendLine(node.toStringShallow(joiner: "\n  "));
                    }

                    throw new UIWidgetsError(
                        this.GetType() + " object was given an infinite size during layout.\n" +
                        "This probably means that it is a render object that tries to be " +
                        "as big as possible, but it was put inside another render object " +
                        "that allows its children to pick their own size.\n" +
                        information +
                        "The constraints that applied to the " + this.GetType() + " were:\n" +
                        "  " + this.constraints + "\n" +
                        "The exact size it was given was:\n" +
                        "  " + this._size
                    );
                }

                if (!this.constraints.isSatisfiedBy(this._size)) {
                    throw new UIWidgetsError(
                        this.GetType() + " does not meet its constraints.\n" +
                        "Constraints: " + this.constraints + "\n" +
                        "Size: " + this._size + "\n" +
                        "If you are not writing your own RenderBox subclass, then this is not " +
                        "your fault."
                    );
                }

                if (D.debugCheckIntrinsicSizes) {
                    D.assert(!debugCheckingIntrinsics);
                    debugCheckingIntrinsics = true;
                    var failures = new StringBuilder();
                    int failureCount = 0;

                    var testIntrinsic = new Func<Func<float, float>, string, float, float>(
                        (function, name, constraint) => {
                            float result = function(constraint);
                            if (result < 0) {
                                failures.AppendLine(" * " + name + "(" + constraint + ") returned a negative value: " +
                                                    result);
                                failureCount += 1;
                            }

                            if (result.isInfinite()) {
                                failures.AppendLine(" * " + name + "(" + constraint +
                                                    ") returned a non-finite value: " + result);
                                failureCount += 1;
                            }

                            return result;
                        });

                    var testIntrinsicsForValues =
                        new Action<Func<float, float>, Func<float, float>, string, float>(
                            (getMin, getMax, name, constraint) => {
                                float min = testIntrinsic(getMin, "getMinIntrinsic" + name, constraint);
                                float max = testIntrinsic(getMax, "getMaxIntrinsic" + name, constraint);
                                if (min > max) {
                                    failures.AppendLine(
                                        " * getMinIntrinsic" + name + "(" + constraint + ") returned a larger value (" +
                                        min +
                                        ") than getMaxIntrinsic" + name + "(" + constraint + ") (" + max + ")");
                                    failureCount += 1;
                                }
                            });

                    testIntrinsicsForValues(this.getMinIntrinsicWidth, this.getMaxIntrinsicWidth, "Width",
                        float.PositiveInfinity);
                    testIntrinsicsForValues(this.getMinIntrinsicHeight, this.getMaxIntrinsicHeight, "Height",
                        float.PositiveInfinity);

                    if (this.constraints.hasBoundedWidth) {
                        testIntrinsicsForValues(this.getMinIntrinsicWidth, this.getMaxIntrinsicWidth, "Width",
                            this.constraints.maxHeight);
                    }

                    if (this.constraints.hasBoundedHeight) {
                        testIntrinsicsForValues(this.getMinIntrinsicHeight, this.getMaxIntrinsicHeight, "Height",
                            this.constraints.maxWidth);
                    }

                    debugCheckingIntrinsics = false;
                    if (failures.Length > 0) {
                        D.assert(failureCount > 0);
                        throw new UIWidgetsError(
                            "The intrinsic dimension methods of the " + this.GetType() +
                            " class returned values that violate the intrinsic protocol contract.\n" +
                            "The following failures was detected:\n" +
                            failures +
                            "If you are not writing your own RenderBox subclass, then this is not\n" +
                            "your fault."
                        );
                    }
                }

                return true;
            });
        }

        public override void markNeedsLayout() {
            if (this._cachedBaselines != null && this._cachedBaselines.isNotEmpty() ||
                this._cachedIntrinsicDimensions != null && this._cachedIntrinsicDimensions.isNotEmpty()) {
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


        protected override void performResize() {
            this.size = this.constraints.smallest;
            D.assert(this.size.isFinite);
        }

        protected override void performLayout() {
            D.assert(() => {
                if (!this.sizedByParent) {
                    throw new UIWidgetsError(
                        this.GetType() + " did not implement performLayout().\n" +
                        "RenderBox subclasses need to either override performLayout() to " +
                        "set a size and lay out any children, or, set sizedByParent to true " +
                        "so that performResize() sizes the render object."
                    );
                }

                return true;
            });
        }

        public virtual bool hitTest(HitTestResult result, Offset position = null) {
            D.assert(position != null);
            D.assert(() => {
                if (!this.hasSize) {
                    if (this.debugNeedsLayout) {
                        throw new UIWidgetsError(
                            "Cannot hit test a render box that has never been laid out.\n" +
                            "The hitTest() method was called on this RenderBox:\n" +
                            "  " + this + "\n" +
                            "Unfortunately, this object\"s geometry is not known at this time, " +
                            "probably because it has never been laid out. " +
                            "This means it cannot be accurately hit-tested. If you are trying " +
                            "to perform a hit test during the layout phase itself, make sure " +
                            "you only hit test nodes that have completed layout (e.g. the node\"s " +
                            "children, after their layout() method has been called)."
                        );
                    }

                    throw new UIWidgetsError(
                        "Cannot hit test a render box with no size.\n" +
                        "The hitTest() method was called on this RenderBox:\n" +
                        "  " + this + "\n" +
                        "Although this node is not marked as needing layout, " +
                        "its size is not set. A RenderBox object must have an " +
                        "explicit size before it can be hit-tested. Make sure " +
                        "that the RenderBox in question sets its size during layout."
                    );
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

        protected virtual bool hitTestChildren(HitTestResult result, Offset position = null) {
            return false;
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            D.assert(child != null);
            D.assert(child.parent == this);
            D.assert(() => {
                if (!(child.parentData is BoxParentData)) {
                    throw new UIWidgetsError(
                        this.GetType() + " does not implement applyPaintTransform.\n" +
                        "The following " + this.GetType() + " object:\n" +
                        "  " + this.toStringShallow() + "\n" +
                        "...did not use a BoxParentData class for the parentData field of the following child:\n" +
                        "  " + child.toStringShallow() + "\n" +
                        "The " + this.GetType() + " class inherits from RenderBox. " +
                        "The default applyPaintTransform implementation provided by RenderBox assumes that the " +
                        "children all use BoxParentData objects for their parentData field. " +
                        "Since " + this.GetType() +
                        " does not in fact use that ParentData class for its children, it must " +
                        "provide an implementation of applyPaintTransform that supports the specific ParentData " +
                        "subclass used by its children (which apparently is " + child.parentData.GetType() + ")."
                    );
                }

                return true;
            });

            var childParentData = (BoxParentData) child.parentData;
            var offset = childParentData.offset;
            transform.preTranslate(offset.dx, offset.dy);
        }

        public Offset globalToLocal(Offset point, RenderObject ancestor = null) {
            var transform = this.getTransformTo(ancestor);

            var inverse = Matrix3.I();
            var invertible = transform.invert(inverse);
            return invertible ? inverse.mapPoint(point) : Offset.zero;
        }

        public Offset localToGlobal(Offset point, RenderObject ancestor = null) {
            return this.getTransformTo(ancestor).mapPoint(point);
        }

        public override Rect paintBounds {
            get { return Offset.zero & this.size; }
        }

        int _debugActivePointers = 0;

        protected bool debugHandleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(() => {
                if (D.debugPaintPointersEnabled) {
                    if (evt is PointerDownEvent) {
                        this._debugActivePointers += 1;
                    }
                    else if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                        this._debugActivePointers -= 1;
                    }

                    this.markNeedsPaint();
                }

                return true;
            });
            return true;
        }

        public override void debugPaint(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (D.debugPaintSizeEnabled) {
                    this.debugPaintSize(context, offset);
                }

                if (D.debugPaintBaselinesEnabled) {
                    this.debugPaintBaselines(context, offset);
                }

                if (D.debugPaintPointersEnabled) {
                    this.debugPaintPointers(context, offset);
                }

                return true;
            });
        }

        protected virtual void debugPaintSize(PaintingContext context, Offset offset) {
            D.assert(() => {
                var paint = new Paint {
                    color = new Color(0xFF00FFFF),
                    strokeWidth = 1,
                    style = PaintingStyle.stroke,
                };
                context.canvas.drawRect((offset & this.size).deflate(0.5f), paint);
                return true;
            });
        }

        protected virtual void debugPaintBaselines(PaintingContext context, Offset offset) {
            D.assert(() => {
                Paint paint = new Paint {
                    style = PaintingStyle.stroke,
                    strokeWidth = 0.25f
                };

                Path path;
                // ideographic baseline
                float? baselineI = this.getDistanceToBaseline(TextBaseline.ideographic, onlyReal: true);
                if (baselineI != null) {
                    paint.color = new Color(0xFFFFD000);
                    path = new Path();
                    path.moveTo(offset.dx, offset.dy + baselineI.Value);
                    path.lineTo(offset.dx + this.size.width, offset.dy + baselineI.Value);
                    context.canvas.drawPath(path, paint);
                }

                // alphabetic baseline
                float? baselineA = this.getDistanceToBaseline(TextBaseline.alphabetic, onlyReal: true);
                if (baselineA != null) {
                    paint.color = new Color(0xFF00FF00);
                    path = new Path();
                    path.moveTo(offset.dx, offset.dy + baselineA.Value);
                    path.lineTo(offset.dx + this.size.width, offset.dy + baselineA.Value);
                    context.canvas.drawPath(path, paint);
                }

                return true;
            });
        }

        protected virtual void debugPaintPointers(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (this._debugActivePointers > 0) {
                    var paint = new Paint {
                        color = new Color(0x00BBBB | ((0x04000000 * this.depth) & 0xFF000000)),
                    };
                    context.canvas.drawRect(offset & this.size, paint);
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Size>("size", this._size, missingIfNull: true));
        }
    }

    public abstract class ContainerBoxParentData<ChildType> : ContainerParentDataMixinBoxParentData<ChildType>
        where ChildType : RenderBox {
    }

    public abstract class
        RenderBoxContainerDefaultsMixin<ChildType, ParentDataType>
        : ContainerRenderObjectMixinRenderBox<ChildType, ParentDataType>
        where ChildType : RenderBox
        where ParentDataType : ContainerParentDataMixinBoxParentData<ChildType> {
        public float? defaultComputeDistanceToFirstActualBaseline(TextBaseline baseline) {
            D.assert(!this.debugNeedsLayout);

            var child = this.firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                float? result = child.getDistanceToActualBaseline(baseline);
                if (result != null) {
                    return result.Value + childParentData.offset.dy;
                }

                child = childParentData.nextSibling;
            }

            return null;
        }

        public float? defaultComputeDistanceToHighestActualBaseline(TextBaseline baseline) {
            D.assert(!this.debugNeedsLayout);

            float? result = null;
            var child = this.firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                float? candidate = child.getDistanceToActualBaseline(baseline);
                if (candidate != null) {
                    candidate += childParentData.offset.dy;
                    if (result != null) {
                        result = Mathf.Min(result.Value, candidate.Value);
                    }
                    else {
                        result = candidate;
                    }
                }

                child = childParentData.nextSibling;
            }

            return result;
        }

        public bool defaultHitTestChildren(HitTestResult result, Offset position = null) {
            ChildType child = this.lastChild;
            while (child != null) {
                ParentDataType childParentData = (ParentDataType) child.parentData;
                if (child.hitTest(result, position: position - childParentData.offset)) {
                    return true;
                }

                child = childParentData.previousSibling;
            }

            return false;
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