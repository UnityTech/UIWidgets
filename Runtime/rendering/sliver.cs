using System;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public enum GrowthDirection {
        forward,
        reverse,
    }

    public static class GrowthDirectionUtils {
        public static AxisDirection applyGrowthDirectionToAxisDirection(
            AxisDirection axisDirection, GrowthDirection growthDirection) {
            switch (growthDirection) {
                case GrowthDirection.forward:
                    return axisDirection;
                case GrowthDirection.reverse:
                    return AxisUtils.flipAxisDirection(axisDirection);
            }

            throw new Exception("unknown growthDirection");
        }

        public static ScrollDirection applyGrowthDirectionToScrollDirection(
            ScrollDirection scrollDirection, GrowthDirection growthDirection) {
            switch (growthDirection) {
                case GrowthDirection.forward:
                    return scrollDirection;
                case GrowthDirection.reverse:
                    return ScrollDirectionUtils.flipScrollDirection(scrollDirection);
            }

            throw new Exception("unknown growthDirection");
        }
    }

    public class SliverConstraints : Constraints, IEquatable<SliverConstraints> {
        public SliverConstraints(
            AxisDirection axisDirection,
            GrowthDirection growthDirection,
            ScrollDirection userScrollDirection,
            double scrollOffset,
            double overlap,
            double remainingPaintExtent,
            double crossAxisExtent,
            AxisDirection crossAxisDirection,
            double viewportMainAxisExtent,
            double remainingCacheExtent,
            double cacheOrigin
        ) {
            this.axisDirection = axisDirection;
            this.growthDirection = growthDirection;
            this.userScrollDirection = userScrollDirection;
            this.scrollOffset = scrollOffset;
            this.overlap = overlap;
            this.remainingPaintExtent = remainingPaintExtent;
            this.crossAxisExtent = crossAxisExtent;
            this.crossAxisDirection = crossAxisDirection;
            this.viewportMainAxisExtent = viewportMainAxisExtent;
            this.remainingCacheExtent = remainingCacheExtent;
            this.cacheOrigin = cacheOrigin;
        }

        public SliverConstraints copyWith(
            AxisDirection? axisDirection = null,
            GrowthDirection? growthDirection = null,
            ScrollDirection? userScrollDirection = null,
            double? scrollOffset = null,
            double? overlap = null,
            double? remainingPaintExtent = null,
            double? crossAxisExtent = null,
            AxisDirection? crossAxisDirection = null,
            double? viewportMainAxisExtent = null,
            double? remainingCacheExtent = null,
            double? cacheOrigin = null
        ) {
            return new SliverConstraints(
                axisDirection: axisDirection ?? this.axisDirection,
                growthDirection: growthDirection ?? this.growthDirection,
                userScrollDirection: userScrollDirection ?? this.userScrollDirection,
                scrollOffset: scrollOffset ?? this.scrollOffset,
                overlap: overlap ?? this.overlap,
                remainingPaintExtent: remainingPaintExtent ?? this.remainingPaintExtent,
                crossAxisExtent: crossAxisExtent ?? this.crossAxisExtent,
                crossAxisDirection: crossAxisDirection ?? this.crossAxisDirection,
                viewportMainAxisExtent: viewportMainAxisExtent ?? this.viewportMainAxisExtent,
                remainingCacheExtent: remainingCacheExtent ?? this.remainingCacheExtent,
                cacheOrigin: cacheOrigin ?? this.cacheOrigin
            );
        }

        public readonly AxisDirection axisDirection;

        public readonly GrowthDirection growthDirection;

        public readonly ScrollDirection userScrollDirection;

        public readonly double scrollOffset;

        public readonly double overlap;

        public readonly double remainingPaintExtent;

        public readonly double crossAxisExtent;

        public readonly AxisDirection crossAxisDirection;

        public readonly double viewportMainAxisExtent;

        public readonly double cacheOrigin;

        public readonly double remainingCacheExtent;

        public Axis axis {
            get { return AxisUtils.axisDirectionToAxis(this.axisDirection); }
        }

        public GrowthDirection normalizedGrowthDirection {
            get {
                switch (this.axisDirection) {
                    case AxisDirection.down:
                    case AxisDirection.right:
                        return this.growthDirection;
                    case AxisDirection.up:
                    case AxisDirection.left:
                        switch (this.growthDirection) {
                            case GrowthDirection.forward:
                                return GrowthDirection.reverse;
                            case GrowthDirection.reverse:
                                return GrowthDirection.forward;
                        }

                        throw new Exception("unknown growthDirection");
                }

                throw new Exception("unknown axisDirection");
            }
        }

        public override bool isTight {
            get { return false; }
        }

        public override bool isNormalized {
            get {
                return this.scrollOffset >= 0.0
                       && this.crossAxisExtent >= 0.0
                       && AxisUtils.axisDirectionToAxis(this.axisDirection) !=
                       AxisUtils.axisDirectionToAxis(this.crossAxisDirection)
                       && this.viewportMainAxisExtent >= 0.0
                       && this.remainingPaintExtent >= 0.0;
            }
        }

        public BoxConstraints asBoxConstraints(
            double minExtent = 0.0,
            double maxExtent = double.PositiveInfinity,
            double? crossAxisExtent = null
        ) {
            crossAxisExtent = crossAxisExtent ?? this.crossAxisExtent;
            switch (this.axis) {
                case Axis.horizontal:
                    return new BoxConstraints(
                        minHeight: crossAxisExtent.Value,
                        maxHeight: crossAxisExtent.Value,
                        minWidth: minExtent,
                        maxWidth: maxExtent
                    );
                case Axis.vertical:
                    return new BoxConstraints(
                        minWidth: crossAxisExtent.Value,
                        maxWidth: crossAxisExtent.Value,
                        minHeight: minExtent,
                        maxHeight: maxExtent
                    );
            }

            D.assert(false);
            return null;
        }

        public override bool debugAssertIsValid(
            bool isAppliedConstraint = false,
            InformationCollector informationCollector = null
        ) {
            D.assert(() => {
                var verify = new Action<bool, string>((bool check, string message) => {
                    if (check) {
                        return;
                    }

                    var information = new StringBuilder();
                    if (informationCollector != null) {
                        informationCollector(information);
                    }

                    throw new UIWidgetsError(
                        $"{this.GetType()} is not valid: {message}\n{information}The offending constraints were: \n  {this}");
                });

                verify(this.scrollOffset >= 0.0, "The \"scrollOffset\" is negative.");
                verify(this.crossAxisExtent >= 0.0, "The \"crossAxisExtent\" is negative.");
                verify(
                    AxisUtils.axisDirectionToAxis(this.axisDirection) !=
                    AxisUtils.axisDirectionToAxis(this.crossAxisDirection),
                    "The \"axisDirection\" and the \"crossAxisDirection\" are along the same axis.");
                verify(this.viewportMainAxisExtent >= 0.0, "The \"viewportMainAxisExtent\" is negative.");
                verify(this.remainingPaintExtent >= 0.0, "The \"remainingPaintExtent\" is negative.");
                verify(this.remainingCacheExtent >= 0.0, "The \"remainingCacheExtent\" is negative.");
                verify(this.cacheOrigin <= 0.0, "The \"cacheOrigin\" is positive.");
                verify(this.isNormalized, "The constraints are not normalized.");
                return true;
            });

            return true;
        }

        public bool Equals(SliverConstraints other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return this.axisDirection == other.axisDirection
                   && this.growthDirection == other.growthDirection
                   && this.userScrollDirection == other.userScrollDirection
                   && this.scrollOffset.Equals(other.scrollOffset)
                   && this.overlap.Equals(other.overlap)
                   && this.remainingPaintExtent.Equals(other.remainingPaintExtent)
                   && this.crossAxisExtent.Equals(other.crossAxisExtent)
                   && this.crossAxisDirection == other.crossAxisDirection
                   && this.viewportMainAxisExtent.Equals(other.viewportMainAxisExtent)
                   && this.cacheOrigin.Equals(other.cacheOrigin)
                   && this.remainingCacheExtent.Equals(other.remainingCacheExtent);
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
            return this.Equals((SliverConstraints) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) this.axisDirection;
                hashCode = (hashCode * 397) ^ (int) this.growthDirection;
                hashCode = (hashCode * 397) ^ (int) this.userScrollDirection;
                hashCode = (hashCode * 397) ^ this.scrollOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ this.overlap.GetHashCode();
                hashCode = (hashCode * 397) ^ this.remainingPaintExtent.GetHashCode();
                hashCode = (hashCode * 397) ^ this.crossAxisExtent.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) this.crossAxisDirection;
                hashCode = (hashCode * 397) ^ this.viewportMainAxisExtent.GetHashCode();
                hashCode = (hashCode * 397) ^ this.cacheOrigin.GetHashCode();
                hashCode = (hashCode * 397) ^ this.remainingCacheExtent.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SliverConstraints left, SliverConstraints right) {
            return Equals(left, right);
        }

        public static bool operator !=(SliverConstraints left, SliverConstraints right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return
                $"SliverConstraints({this.axisDirection}， {this.growthDirection}， {this.userScrollDirection}， scrollOffset: {this.scrollOffset:F1}, remainingPaintExtent: {this.remainingCacheExtent:F1}, " +
                $"{(this.overlap != 0.0 ? "overlap: " + this.overlap.ToString("F1") + ", " : "")}crossAxisExtent: {this.crossAxisExtent:F1}, crossAxisDirection: {this.crossAxisDirection}, " +
                $"viewportMainAxisExtent: {this.viewportMainAxisExtent:F1}, remainingCacheExtent: {this.remainingCacheExtent:F1} " +
                $"cacheOrigin: {this.cacheOrigin:F1})";
        }
    }

    public class SliverGeometry : Diagnosticable {
        public SliverGeometry(
            double scrollExtent = 0.0,
            double paintExtent = 0.0,
            double paintOrigin = 0.0,
            double? layoutExtent = null,
            double maxPaintExtent = 0.0,
            double maxScrollObstructionExtent = 0.0,
            double? hitTestExtent = null,
            bool? visible = null,
            bool hasVisualOverflow = false,
            double? scrollOffsetCorrection = null,
            double? cacheExtent = null
        ) {
            D.assert(scrollOffsetCorrection != 0.0);

            this.scrollExtent = scrollExtent;
            this.paintExtent = paintExtent;
            this.paintOrigin = paintOrigin;
            this.layoutExtent = layoutExtent ?? paintExtent;
            this.maxPaintExtent = maxPaintExtent;
            this.maxScrollObstructionExtent = maxScrollObstructionExtent;
            this.hitTestExtent = hitTestExtent ?? paintExtent;
            this.visible = visible ?? paintExtent > 0.0;
            this.hasVisualOverflow = hasVisualOverflow;
            this.scrollOffsetCorrection = scrollOffsetCorrection;
            this.cacheExtent = cacheExtent ?? layoutExtent ?? paintExtent;
        }

        public static readonly SliverGeometry zero = new SliverGeometry();

        public readonly double scrollExtent;
        public readonly double paintOrigin;
        public readonly double paintExtent;
        public readonly double layoutExtent;
        public readonly double maxPaintExtent;
        public readonly double maxScrollObstructionExtent;
        public readonly double hitTestExtent;
        public readonly bool visible;
        public readonly bool hasVisualOverflow;
        public readonly double? scrollOffsetCorrection;
        public readonly double cacheExtent;

        internal static string _debugCompareFloats(string labelA, double valueA, string labelB, double valueB) {
            if (valueA.ToString("F1") != valueB.ToString("F1")) {
                return $"The {labelA} is {valueA:F1}, but the {labelB} is {valueB:F1}. ";
            }

            return string.Format(
                "The {0} is {1}, but the {2} is {3}. " +
                "Maybe you have fallen prey to floating point rounding errors, and should explicitly " +
                "apply the min() or max() functions, or the clamp() method, to the {2}? ",
                labelA, valueA, labelB, valueB);
        }

        public bool debugAssertIsValid(InformationCollector informationCollector = null) {
            D.assert(() => {
                var verify = new Action<bool, string>((bool check, string message) => {
                    if (check) {
                        return;
                    }
                    var information = new StringBuilder();
                    if (informationCollector != null) {
                        informationCollector(information);
                    }

                    throw new UIWidgetsError($"{this.GetType()} is not valid: {message}\n{information}");
                });

                verify(this.scrollExtent >= 0.0, "The \"scrollExtent\" is negative.");
                verify(this.paintExtent >= 0.0, "The \"paintExtent\" is negative.");
                verify(this.layoutExtent >= 0.0, "The \"layoutExtent\" is negative.");
                verify(this.cacheExtent >= 0.0, "The \"cacheExtent\" is negative.");
                if (this.layoutExtent > this.paintExtent) {
                    verify(false,
                        "The \"layoutExtent\" exceeds the \"paintExtent\".\n" +
                        _debugCompareFloats("paintExtent", this.paintExtent, "layoutExtent",
                            this.layoutExtent)
                    );
                }

                if (this.maxPaintExtent < this.paintExtent) {
                    verify(false,
                        "The \"maxPaintExtent\" is less than the \"paintExtent\".\n" +
                        _debugCompareFloats("maxPaintExtent", this.maxPaintExtent, "paintExtent",
                            this.paintExtent) +
                        "By definition, a sliver can\"t paint more than the maximum that it can paint!"
                    );
                }

                verify(this.hitTestExtent >= 0.0, "The \"hitTestExtent\" is negative.");
                verify(this.scrollOffsetCorrection != 0.0, "The \"scrollOffsetCorrection\" is zero.");
                return true;
            });
            return true;
        }

        public override string toStringShort() {
            return this.GetType().ToString();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DoubleProperty("scrollExtent", this.scrollExtent));
            if (this.paintExtent > 0.0) {
                properties.add(new DoubleProperty("paintExtent", this.paintExtent,
                    unit: this.visible ? null : " but not painting"));
            } else if (this.paintExtent == 0.0) {
                if (this.visible) {
                    properties.add(new DoubleProperty("paintExtent", this.paintExtent,
                        unit: this.visible ? null : " but visible"));
                }

                properties.add(new FlagProperty("visible", value: this.visible, ifFalse: "hidden"));
            } else {
                properties.add(new DoubleProperty("paintExtent", this.paintExtent, tooltip: "!"));
            }

            properties.add(new DoubleProperty("paintOrigin", this.paintOrigin,
                defaultValue: 0.0));
            properties.add(new DoubleProperty("layoutExtent", this.layoutExtent,
                defaultValue: this.paintExtent));
            properties.add(new DoubleProperty("maxPaintExtent", this.maxPaintExtent));
            properties.add(new DoubleProperty("hitTestExtent", this.hitTestExtent,
                defaultValue: this.paintExtent));
            properties.add(new DiagnosticsProperty<bool>("hasVisualOverflow", this.hasVisualOverflow,
                defaultValue: false));
            properties.add(new DoubleProperty("scrollOffsetCorrection", this.scrollOffsetCorrection,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DoubleProperty("cacheExtent", this.cacheExtent,
                defaultValue: 0.0));
        }
    }

    public class SliverHitTestEntry : HitTestEntry {
        public SliverHitTestEntry(RenderSliver target,
            double mainAxisPosition = 0.0,
            double crossAxisPosition = 0.0
        ) : base(target) {
            this.mainAxisPosition = mainAxisPosition;
            this.crossAxisPosition = crossAxisPosition;
        }


        public new RenderSliver target {
            get { return (RenderSliver) base.target; }
        }

        public readonly double mainAxisPosition;

        public readonly double crossAxisPosition;

        public override string ToString() {
            return $"{this.target.GetType()}@(mainAix: {this.mainAxisPosition}, crossAix: {this.crossAxisPosition})";
        }
    }

    public class SliverLogicalParentData : ParentData {
        public double layoutOffset = 0.0;

        public override string ToString() {
            return "layoutOffset=" + this.layoutOffset.ToString("F1");
        }
    }

    public class SliverLogicalContainerParentData : ContainerParentDataMixinSliverLogicalParentData<RenderSliver> {
    }


    public class SliverPhysicalParentData : ParentData {
        public Offset paintOffset = Offset.zero;

        public void applyPaintTransform(Matrix3 transform) {
            transform.preTranslate((float) this.paintOffset.dx, (float) this.paintOffset.dy);
        }

        public override string ToString() {
            return "paintOffset=" + this.paintOffset;
        }
    }

    public class SliverPhysicalContainerParentData : ContainerParentDataMixinSliverPhysicalParentData<RenderSliver> {
    }

    public abstract class RenderSliver : RenderObject {
        public new SliverConstraints constraints {
            get { return (SliverConstraints) base.constraints; }
        }

        public SliverGeometry geometry {
            get { return this._geometry; }
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
                        violation = "It appears that the geometry setter was called from performLayout().";
                        hint = "";
                    } else {
                        violation =
                            "The geometry setter was called from outside layout (neither performResize() nor performLayout() were being run for this object).";
                        if (this.owner != null && this.owner.debugDoingLayout) {
                            hint =
                                "Only the object itself can set its geometry. It is a contract violation for other objects to set it.";
                        }
                    }

                    if (this.sizedByParent) {
                        contract =
                            "Because this RenderSliver has sizedByParent set to true, it must set its geometry in performResize().";
                    } else {
                        contract =
                            "Because this RenderSliver has sizedByParent set to false, it must set its geometry in performLayout().";
                    }

                    throw new UIWidgetsError(
                        "RenderSliver geometry setter called incorrectly.\n" +
                        violation + "\n" +
                        hint + "\n" +
                        contract + "\n" +
                        "The RenderSliver in question is:\n" +
                        "  " + this
                    );
                });

                this._geometry = value;
            }
        }

        SliverGeometry _geometry;

        public override Rect paintBounds {
            get {
                switch (this.constraints.axis) {
                    case Axis.horizontal:
                        return Rect.fromLTWH(
                            0.0, 0.0,
                            this.geometry.paintExtent,
                            this.constraints.crossAxisExtent
                        );
                    case Axis.vertical:
                        return Rect.fromLTWH(
                            0.0, 0.0,
                            this.constraints.crossAxisExtent,
                            this.geometry.paintExtent
                        );
                }

                D.assert(false);
                return null;
            }
        }

        public override Rect semanticBounds {
            get { return this.paintBounds; }
        }

        protected override void debugResetSize() {
        }

        protected override void debugAssertDoesMeetConstraints() {
            D.assert(this.geometry.debugAssertIsValid(
                informationCollector: (information) => {
                    information.AppendLine("The RenderSliver that returned the offending geometry was:");
                    information.AppendLine("  " + this.toStringShallow(joiner: "\n  "));
                }));
            D.assert(() => {
                if (this.geometry.paintExtent > this.constraints.remainingPaintExtent) {
                    throw new UIWidgetsError(
                        "SliverGeometry has a paintOffset that exceeds the remainingPaintExtent from the constraints.\n" +
                        "The render object whose geometry violates the constraints is the following:\n" +
                        "  " + this.toStringShallow(joiner: "\n  ") + "\n" +
                        SliverGeometry._debugCompareFloats(
                            "remainingPaintExtent", this.constraints.remainingPaintExtent,
                            "paintExtent", this.geometry.paintExtent) +
                        "The paintExtent must cause the child sliver to paint within the viewport, and so " +
                        "cannot exceed the remainingPaintExtent."
                    );
                }

                return true;
            });
        }

        protected override void performResize() {
            D.assert(false);
        }

        public double centerOffsetAdjustment {
            get { return 0.0; }
        }

        public bool hitTest(HitTestResult result, double mainAxisPosition = 0, double crossAxisPosition = 0) {
            if (mainAxisPosition >= 0.0 && mainAxisPosition < this.geometry.hitTestExtent &&
                crossAxisPosition >= 0.0 && crossAxisPosition < this.constraints.crossAxisExtent) {
                if (this.hitTestChildren(result, mainAxisPosition: mainAxisPosition,
                        crossAxisPosition: crossAxisPosition) ||
                    this.hitTestSelf(mainAxisPosition: mainAxisPosition, crossAxisPosition: crossAxisPosition)) {
                    result.add(new SliverHitTestEntry(
                        this,
                        mainAxisPosition: mainAxisPosition,
                        crossAxisPosition: crossAxisPosition
                    ));
                    return true;
                }
            }

            return false;
        }

        protected virtual bool hitTestSelf(double mainAxisPosition = 0, double crossAxisPosition = 0) {
            return false;
        }

        protected virtual bool hitTestChildren(HitTestResult result, double mainAxisPosition = 0,
            double crossAxisPosition = 0) {
            return false;
        }

        public double calculatePaintOffset(SliverConstraints constraints, double from, double to) {
            D.assert(from <= to);
            double a = constraints.scrollOffset;
            double b = constraints.scrollOffset + constraints.remainingPaintExtent;
            return (to.clamp(a, b) - from.clamp(a, b)).clamp(0.0, constraints.remainingPaintExtent);
        }

        public double calculateCacheOffset(SliverConstraints constraints, double from, double to) {
            D.assert(from <= to);
            double a = constraints.scrollOffset + constraints.cacheOrigin;
            double b = constraints.scrollOffset + constraints.remainingCacheExtent;
            return (to.clamp(a, b) - from.clamp(a, b)).clamp(0.0, constraints.remainingCacheExtent);
        }

        public virtual double childMainAxisPosition(RenderObject child) {
            D.assert(() => { throw new UIWidgetsError(this.GetType() + " does not implement childPosition."); });

            return 0.0;
        }

        public virtual double childCrossAxisPosition(RenderObject child) {
            return 0.0;
        }

        public virtual double childScrollOffset(RenderObject child) {
            D.assert(child.parent == this);
            return 0.0;
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            D.assert(() => { throw new UIWidgetsError(this.GetType() + " does not implement applyPaintTransform."); });
        }

        internal Size getAbsoluteSizeRelativeToOrigin() {
            D.assert(this.geometry != null);
            D.assert(!this.debugNeedsLayout);

            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(
                this.constraints.axisDirection, this.constraints.growthDirection)) {
                case AxisDirection.up:
                    return new Size(this.constraints.crossAxisExtent, -this.geometry.paintExtent);
                case AxisDirection.right:
                    return new Size(this.geometry.paintExtent, this.constraints.crossAxisExtent);
                case AxisDirection.down:
                    return new Size(this.constraints.crossAxisExtent, this.geometry.paintExtent);
                case AxisDirection.left:
                    return new Size(-this.geometry.paintExtent, this.constraints.crossAxisExtent);
            }

            D.assert(false);
            return null;
        }

        void _debugDrawArrow(Canvas canvas, Paint paint, Offset p0, Offset p1, GrowthDirection direction) {
            D.assert(() => {
                if (p0 == p1) {
                    return true;
                }

                D.assert(p0.dx == p1.dx || p0.dy == p1.dy);
                double d = (p1 - p0).distance * 0.2;
                Offset temp;
                double dx1 = 0, dx2 = 0, dy1 = 0, dy2 = 0;
                switch (direction) {
                    case GrowthDirection.forward:
                        dx1 = dx2 = dy1 = dy2 = d;
                        break;
                    case GrowthDirection.reverse:
                        temp = p0;
                        p0 = p1;
                        p1 = temp;
                        dx1 = dx2 = dy1 = dy2 = -d;
                        break;
                }

                if (p0.dx == p1.dx) {
                    dx2 = -dx2;
                } else {
                    dy2 = -dy2;
                }

//      canvas.drawPath(
//        new Path()
//          ..moveTo(p0.dx, p0.dy)
//          ..lineTo(p1.dx, p1.dy)
//          ..moveTo(p1.dx - dx1, p1.dy - dy1)
//          ..lineTo(p1.dx, p1.dy)
//          ..lineTo(p1.dx - dx2, p1.dy - dy2),
//        paint
//      );
                return true;
            });
        }

        public override void debugPaint(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (D.debugPaintSizeEnabled) {
                    double strokeWidth = Math.Min(4.0, this.geometry.paintExtent / 30.0);
                    Paint paint = new Paint();
//          ..color = const Color(0xFF33CC33)
//          ..strokeWidth = strokeWidth
//          ..style = PaintingStyle.stroke
//          ..maskFilter = new MaskFilter.blur(BlurStyle.solid, strokeWidth);
                    double arrowExtent = this.geometry.paintExtent;
                    double padding = Math.Max(2.0, strokeWidth);
                    Canvas canvas = context.canvas;
//        canvas.drawCircle(
//          offset.translate(padding, padding),
//          padding * 0.5,
//          paint,
//        );
                    switch (this.constraints.axis) {
                        case Axis.vertical:
//            canvas.drawLine(
//              offset,
//              offset.translate(constraints.crossAxisExtent, 0.0),
//              paint,
//            );
                            this._debugDrawArrow(
                                canvas,
                                paint,
                                offset.translate(this.constraints.crossAxisExtent * 1.0 / 4.0, padding),
                                offset.translate(this.constraints.crossAxisExtent * 1.0 / 4.0, arrowExtent - padding),
                                this.constraints.normalizedGrowthDirection
                            );
                            this._debugDrawArrow(
                                canvas,
                                paint,
                                offset.translate(this.constraints.crossAxisExtent * 3.0 / 4.0, padding),
                                offset.translate(this.constraints.crossAxisExtent * 3.0 / 4.0, arrowExtent - padding),
                                this.constraints.normalizedGrowthDirection
                            );
                            break;
                        case Axis.horizontal:
//            canvas.drawLine(
//              offset,
//              offset.translate(0.0, constraints.crossAxisExtent),
//              paint,
//            );
                            this._debugDrawArrow(
                                canvas,
                                paint,
                                offset.translate(padding, this.constraints.crossAxisExtent * 1.0 / 4.0),
                                offset.translate(arrowExtent - padding, this.constraints.crossAxisExtent * 1.0 / 4.0),
                                this.constraints.normalizedGrowthDirection
                            );
                            this._debugDrawArrow(
                                canvas,
                                paint,
                                offset.translate(padding, this.constraints.crossAxisExtent * 3.0 / 4.0),
                                offset.translate(arrowExtent - padding, this.constraints.crossAxisExtent * 3.0 / 4.0),
                                this.constraints.normalizedGrowthDirection
                            );
                            break;
                    }
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverGeometry>("geometry", this.geometry));
        }
    }

    public static class RenderSliverHelpers {
        public static bool _getRightWayUp(SliverConstraints constraints) {
            D.assert(constraints != null);

            bool rightWayUp = true;

            switch (constraints.axisDirection) {
                case AxisDirection.up:
                case AxisDirection.left:
                    rightWayUp = false;
                    break;
                case AxisDirection.down:
                case AxisDirection.right:
                    rightWayUp = true;
                    break;
            }

            switch (constraints.growthDirection) {
                case GrowthDirection.forward:
                    break;
                case GrowthDirection.reverse:
                    rightWayUp = !rightWayUp;
                    break;
            }

            return rightWayUp;
        }

        public static bool hitTestBoxChild(this RenderSliver it, HitTestResult result, RenderBox child,
            double mainAxisPosition = 0.0, double crossAxisPosition = 0.0) {
            bool rightWayUp = _getRightWayUp(it.constraints);
            double absolutePosition = mainAxisPosition - it.childMainAxisPosition(child);
            double absoluteCrossAxisPosition = crossAxisPosition - it.childCrossAxisPosition(child);
            switch (it.constraints.axis) {
                case Axis.horizontal:
                    if (!rightWayUp) {
                        absolutePosition = child.size.width - absolutePosition;
                    }

                    return child.hitTest(result, position: new Offset(absolutePosition, absoluteCrossAxisPosition));
                case Axis.vertical:
                    if (!rightWayUp) {
                        absolutePosition = child.size.height - absolutePosition;
                    }

                    return child.hitTest(result, position: new Offset(absoluteCrossAxisPosition, absolutePosition));
            }

            return false;
        }

        public static void applyPaintTransformForBoxChild(this RenderSliver it, RenderBox child,
            Matrix3 transform) {
            bool rightWayUp = _getRightWayUp(it.constraints);
            double delta = it.childMainAxisPosition(child);
            double crossAxisDelta = it.childCrossAxisPosition(child);
            switch (it.constraints.axis) {
                case Axis.horizontal:
                    if (!rightWayUp) {
                        delta = it.geometry.paintExtent - child.size.width - delta;
                    }

                    transform.preTranslate((float) delta, (float) crossAxisDelta);
                    break;
                case Axis.vertical:
                    if (!rightWayUp) {
                        delta = it.geometry.paintExtent - child.size.height - delta;
                    }

                    transform.preTranslate((float) crossAxisDelta, (float) delta);
                    break;
            }
        }
    }

    public abstract class RenderSliverSingleBoxAdapter : RenderObjectWithChildMixinRenderSliver<RenderBox> {
        public RenderSliverSingleBoxAdapter(
            RenderBox child = null
        ) {
            this.child = child;
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverPhysicalParentData)) {
                child.parentData = new SliverPhysicalParentData();
            }
        }

        public void setChildParentData(RenderObject child, SliverConstraints constraints, SliverGeometry geometry) {
            var childParentData = (SliverPhysicalParentData) child.parentData;
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(constraints.axisDirection,
                constraints.growthDirection)) {
                case AxisDirection.up:
                    childParentData.paintOffset = new Offset(0.0,
                        -(geometry.scrollExtent - (geometry.paintExtent + constraints.scrollOffset)));
                    break;
                case AxisDirection.right:
                    childParentData.paintOffset = new Offset(-constraints.scrollOffset, 0.0);
                    break;
                case AxisDirection.down:
                    childParentData.paintOffset = new Offset(0.0, -constraints.scrollOffset);
                    break;
                case AxisDirection.left:
                    childParentData.paintOffset =
                        new Offset(-(geometry.scrollExtent - (geometry.paintExtent + constraints.scrollOffset)), 0.0);
                    break;
            }
        }

        protected override bool hitTestChildren(HitTestResult result, double mainAxisPosition = 0.0,
            double crossAxisPosition = 0.0) {
            D.assert(this.geometry.hitTestExtent > 0.0);
            if (this.child != null) {
                return this.hitTestBoxChild(result, this.child, mainAxisPosition: mainAxisPosition,
                    crossAxisPosition: crossAxisPosition);
            }

            return false;
        }

        public override double childMainAxisPosition(RenderObject child) {
            return -this.constraints.scrollOffset;
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            D.assert(child != null);
            D.assert(child == this.child);

            var childParentData = (SliverPhysicalParentData) child.parentData;
            childParentData.applyPaintTransform(transform);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null && this.geometry.visible) {
                var childParentData = (SliverPhysicalParentData) this.child.parentData;
                context.paintChild(this.child, offset + childParentData.paintOffset);
            }
        }
    }

    public class RenderSliverToBoxAdapter : RenderSliverSingleBoxAdapter {
        public RenderSliverToBoxAdapter(
            RenderBox child = null
        ) : base(child) {
        }

        protected override void performLayout() {
            if (this.child == null) {
                this.geometry = SliverGeometry.zero;
                return;
            }

            this.child.layout(this.constraints.asBoxConstraints(), parentUsesSize: true);

            double childExtent = 0.0;
            switch (this.constraints.axis) {
                case Axis.horizontal:
                    childExtent = this.child.size.width;
                    break;
                case Axis.vertical:
                    childExtent = this.child.size.height;
                    break;
            }

            double paintedChildSize = this.calculatePaintOffset(this.constraints, from: 0.0, to: childExtent);
            double cacheExtent = this.calculateCacheOffset(this.constraints, from: 0.0, to: childExtent);

            D.assert(paintedChildSize.isFinite());
            D.assert(paintedChildSize >= 0.0);

            this.geometry = new SliverGeometry(
                scrollExtent: childExtent,
                paintExtent: paintedChildSize,
                cacheExtent: cacheExtent,
                maxPaintExtent: childExtent,
                hitTestExtent: paintedChildSize,
                hasVisualOverflow: childExtent > this.constraints.remainingPaintExtent
                                   || this.constraints.scrollOffset > 0.0
            );

            this.setChildParentData(this.child, this.constraints, this.geometry);
        }
    }
}
