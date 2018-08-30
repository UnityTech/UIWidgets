using UIWidgets.painting;
using UnityEngine;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering {
    public interface RenderAbstractViewport {
        RevealedOffset getOffsetToReveal(RenderObject target, double alignment, Rect rect = null);
    }

    public static class RenderAbstractViewportUtils {
        public static RenderAbstractViewport of(RenderObject obj) {
            while (obj != null) {
                if (obj is RenderAbstractViewport)
                    return (RenderAbstractViewport) obj;
                obj = (RenderObject) obj.parent;
            }

            return null;
        }

        public const double defaultCacheExtent = 250.0;
    }

    public class RevealedOffset {
        public RevealedOffset(double offset, Rect rect) {
            this.offset = offset;
            this.rect = rect;
        }

        public readonly double offset;
        public readonly Rect rect;
    }

    public abstract class RenderViewportBase<ParentDataClass> :
        ContainerRenderObjectMixinRenderBox<RenderSliver, ParentDataClass>,
        RenderAbstractViewport
        where ParentDataClass : ParentData, ContainerParentDataMixin<RenderSliver> {
        protected RenderViewportBase(
            AxisDirection crossAxisDirection,
            ViewportOffset offset,
            double cacheExtent = RenderAbstractViewportUtils.defaultCacheExtent,
            AxisDirection axisDirection = AxisDirection.down
        ) {
            Debug.Assert(AxisUtils.axisDirectionToAxis(axisDirection) !=
                         AxisUtils.axisDirectionToAxis(crossAxisDirection));
            this._axisDirection = axisDirection;
            this._crossAxisDirection = crossAxisDirection;
            this._offset = offset;
            this._cacheExtent = cacheExtent;
        }

        public AxisDirection axisDirection {
            get { return this._axisDirection; }
            set {
                if (value == this._axisDirection) {
                    return;
                }

                this._axisDirection = value;
                this.markNeedsLayout();
            }
        }

        public AxisDirection _axisDirection;

        public AxisDirection crossAxisDirection {
            get { return this._crossAxisDirection; }
            set {
                if (value == this._crossAxisDirection) {
                    return;
                }

                this._crossAxisDirection = value;
                this.markNeedsLayout();
            }
        }

        public AxisDirection _crossAxisDirection;

        public Axis axis {
            get { return AxisUtils.axisDirectionToAxis(this.axisDirection); }
        }


        public ViewportOffset offset {
            get { return this._offset; }
            set {
                if (object.Equals(value, this._offset)) {
                    return;
                }

                if (this.attached) {
                    this._offset.removeListener(this.markNeedsLayout);
                }

                this._offset = value;
                if (this.attached) {
                    this._offset.addListener(this.markNeedsLayout);
                }

                this.markNeedsLayout();
            }
        }

        public ViewportOffset _offset;

        public double cacheExtent {
            get { return this._cacheExtent; }
            set {
                if (value == this._cacheExtent) {
                    return;
                }

                this._cacheExtent = value;
                this.markNeedsLayout();
            }
        }

        public double _cacheExtent;

        public override void attach(object owner) {
            base.attach(owner);
            this._offset.addListener(this.markNeedsLayout);
        }

        public override void detach() {
            this._offset.removeListener(this.markNeedsLayout);
            base.detach();
        }


        public RevealedOffset getOffsetToReveal(RenderObject target, double alignment, Rect rect = null) {
            throw new System.NotImplementedException();
        }
    }


    public class RenderViewport : RenderViewportBase<SliverPhysicalContainerParentData> {
        public RenderViewport(AxisDirection crossAxisDirection, ViewportOffset offset,
            double cacheExtent = RenderAbstractViewportUtils.defaultCacheExtent,
            AxisDirection axisDirection = AxisDirection.down) : base(crossAxisDirection, offset, cacheExtent,
            axisDirection) {
            Font x = new Font();
            // x.characterInfo
        }
    }
}