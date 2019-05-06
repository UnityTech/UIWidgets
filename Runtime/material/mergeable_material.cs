using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public abstract class MergeableMaterialItem {
        public MergeableMaterialItem(
            LocalKey key) {
            D.assert(key != null);
            this.key = key;
        }

        public readonly LocalKey key;
    }

    public class MaterialSlice : MergeableMaterialItem {
        public MaterialSlice(
            LocalKey key = null,
            Widget child = null) : base(key: key) {
            D.assert(key != null);
            D.assert(child != null);
            this.child = child;
        }

        public readonly Widget child;

        public override string ToString() {
            return "MergeableSlice(key: " + this.key + ", child: " + this.child + ")";
        }
    }

    public class MaterialGap : MergeableMaterialItem {
        public MaterialGap(
            LocalKey key = null,
            float size = 16.0f) : base(key: key) {
            D.assert(key != null);
            this.size = size;
        }

        public readonly float size;

        public override string ToString() {
            return "MaterialGap(key: " + this.key + ", child: " + this.size + ")";
        }
    }


    public class MergeableMaterial : StatefulWidget {
        public MergeableMaterial(
            Key key = null,
            Axis mainAxis = Axis.vertical,
            int elevation = 2,
            bool hasDividers = false,
            List<MergeableMaterialItem> children = null) : base(key: key) {
            this.mainAxis = mainAxis;
            this.elevation = elevation;
            this.hasDividers = hasDividers;
            this.children = children ?? new List<MergeableMaterialItem>();
        }

        public readonly List<MergeableMaterialItem> children;

        public readonly Axis mainAxis;

        public readonly int elevation;

        public readonly bool hasDividers;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<Axis>("mainAxis", this.mainAxis));
            properties.add(new FloatProperty("elevation", this.elevation));
        }

        public override State createState() {
            return new _MergeableMaterialState();
        }
    }


    public class _AnimationTuple {
        public _AnimationTuple(
            AnimationController controller = null,
            CurvedAnimation startAnimation = null,
            CurvedAnimation endAnimation = null,
            CurvedAnimation gapAnimation = null,
            float gapStart = 0.0f) {
            this.controller = controller;
            this.startAnimation = startAnimation;
            this.endAnimation = endAnimation;
            this.gapAnimation = gapAnimation;
            this.gapStart = gapStart;
        }

        public readonly AnimationController controller;

        public readonly CurvedAnimation startAnimation;

        public readonly CurvedAnimation endAnimation;

        public readonly CurvedAnimation gapAnimation;

        public float gapStart;
    }


    public class _MergeableMaterialState : TickerProviderStateMixin<MergeableMaterial> {
        List<MergeableMaterialItem> _children;

        public readonly Dictionary<LocalKey, _AnimationTuple> _animationTuples =
            new Dictionary<LocalKey, _AnimationTuple>();

        public override void initState() {
            base.initState();
            this._children = new List<MergeableMaterialItem>();
            this._children.AddRange(this.widget.children);

            for (int i = 0; i < this._children.Count; i++) {
                if (this._children[i] is MaterialGap) {
                    this._initGap((MaterialGap) this._children[i]);
                    this._animationTuples[this._children[i].key].controller.setValue(1.0f);
                }
            }

            D.assert(this._debugGapsAreValid(this._children));
        }

        void _initGap(MaterialGap gap) {
            AnimationController controller = new AnimationController(
                duration: ThemeUtils.kThemeAnimationDuration,
                vsync: this);

            CurvedAnimation startAnimation = new CurvedAnimation(
                parent: controller,
                curve: Curves.fastOutSlowIn);

            CurvedAnimation endAnimation = new CurvedAnimation(
                parent: controller,
                curve: Curves.fastOutSlowIn);

            CurvedAnimation gapAnimation = new CurvedAnimation(
                parent: controller,
                curve: Curves.fastOutSlowIn);

            controller.addListener(this._handleTick);

            this._animationTuples[gap.key] = new _AnimationTuple(
                controller: controller,
                startAnimation: startAnimation,
                endAnimation: endAnimation,
                gapAnimation: gapAnimation);
        }

        public override void dispose() {
            foreach (MergeableMaterialItem child in this._children) {
                if (child is MaterialGap) {
                    this._animationTuples[child.key].controller.dispose();
                }
            }

            base.dispose();
        }


        void _handleTick() {
            this.setState(() => { });
        }

        bool _debugHasConsecutiveGaps(List<MergeableMaterialItem> children) {
            for (int i = 0; i < this.widget.children.Count - 1; i++) {
                if (this.widget.children[i] is MaterialGap &&
                    this.widget.children[i + 1] is MaterialGap) {
                    return true;
                }
            }

            return false;
        }

        bool _debugGapsAreValid(List<MergeableMaterialItem> children) {
            if (this._debugHasConsecutiveGaps(children)) {
                return false;
            }

            if (children.isNotEmpty()) {
                if (children.first() is MaterialGap || children.last() is MaterialGap) {
                    return false;
                }
            }

            return true;
        }

        void _insertChild(int index, MergeableMaterialItem child) {
            this._children.Insert(index, child);

            if (child is MaterialGap) {
                this._initGap((MaterialGap) child);
            }
        }

        void _removeChild(int index) {
            MergeableMaterialItem child = this._children[index];
            this._children.RemoveAt(index);

            if (child is MaterialGap) {
                this._animationTuples[child.key] = null;
            }
        }

        bool _isClosingGap(int index) {
            if (index < this._children.Count - 1 && this._children[index] is MaterialGap) {
                return this._animationTuples[this._children[index].key].controller.status == AnimationStatus.reverse;
            }

            return false;
        }

        void _removeEmptyGaps() {
            int j = 0;

            while (j < this._children.Count) {
                if (this._children[j] is MaterialGap &&
                    this._animationTuples[this._children[j].key].controller.status == AnimationStatus.dismissed) {
                    this._removeChild(j);
                }
                else {
                    j++;
                }
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);

            MergeableMaterial _oldWidget = (MergeableMaterial) oldWidget;
            HashSet<LocalKey> oldKeys = new HashSet<LocalKey>();
            foreach (MergeableMaterialItem child in _oldWidget.children) {
                oldKeys.Add(child.key);
            }

            HashSet<LocalKey> newKeys = new HashSet<LocalKey>();
            foreach (MergeableMaterialItem child in this.widget.children) {
                newKeys.Add(child.key);
            }

            HashSet<LocalKey> newOnly = new HashSet<LocalKey>();
            foreach (var key in newKeys) {
                if (!oldKeys.Contains(key)) {
                    newOnly.Add(key);
                }
            }

            HashSet<LocalKey> oldOnly = new HashSet<LocalKey>();
            foreach (var key in oldKeys) {
                if (!newKeys.Contains(key)) {
                    oldOnly.Add(key);
                }
            }

            List<MergeableMaterialItem> newChildren = this.widget.children;
            int i = 0;
            int j = 0;

            D.assert(this._debugGapsAreValid(newChildren));
            this._removeEmptyGaps();

            while (i < newChildren.Count && j < this._children.Count) {
                if (newOnly.Contains(newChildren[i].key) ||
                    oldOnly.Contains(this._children[j].key)) {
                    int startNew = i;
                    int startOld = j;

                    while (newOnly.Contains(newChildren[i].key)) {
                        i++;
                    }

                    while (oldOnly.Contains(this._children[j].key) || this._isClosingGap(j)) {
                        j++;
                    }

                    int newLength = i - startNew;
                    int oldLength = j - startOld;

                    if (newLength > 0) {
                        if (oldLength > 1 || oldLength == 1 && this._children[startOld] is MaterialSlice) {
                            if (newLength == 1 && newChildren[startNew] is MaterialGap) {
                                float gapSizeSum = 0.0f;

                                while (startOld < j) {
                                    if (this._children[startOld] is MaterialGap) {
                                        MaterialGap gap = (MaterialGap) this._children[startOld];
                                        gapSizeSum += gap.size;
                                    }

                                    this._removeChild(startOld);
                                    j--;
                                }

                                this._insertChild(startOld, newChildren[startNew]);
                                this._animationTuples[newChildren[startNew].key].gapStart = gapSizeSum;
                                this._animationTuples[newChildren[startNew].key].controller.forward();
                                j++;
                            }
                            else {
                                for (int k = 0; k < oldLength; k++) {
                                    this._removeChild(startOld);
                                }

                                for (int k = 0; k < newLength; k++) {
                                    this._insertChild(startOld + k, newChildren[startNew + k]);
                                }

                                j += (newLength - oldLength);
                            }
                        }
                        else if (oldLength == 1) {
                            if (newLength == 1 && newChildren[startNew] is MaterialGap &&
                                this._children[startOld].key == newChildren[startNew].key) {
                                this._animationTuples[newChildren[startNew].key].controller.forward();
                            }
                            else {
                                float gapSize = this._getGapSize(startOld);

                                this._removeChild(startOld);

                                for (int k = 0; k < newLength; k++) {
                                    this._insertChild(startOld + k, newChildren[startNew + k]);
                                }

                                j += (newLength - 1);
                                float gapSizeSum = 0.0f;

                                for (int k = startNew; k < i; k++) {
                                    if (newChildren[k] is MaterialGap) {
                                        MaterialGap gap = (MaterialGap) newChildren[k];
                                        gapSizeSum += gap.size;
                                    }
                                }

                                for (int k = startNew; k < i; k++) {
                                    if (newChildren[k] is MaterialGap) {
                                        MaterialGap gap = (MaterialGap) newChildren[k];

                                        this._animationTuples[gap.key].gapStart = gapSize * gap.size / gapSizeSum;
                                        this._animationTuples[gap.key].controller.setValue(0.0f);
                                        this._animationTuples[gap.key].controller.forward();
                                    }
                                }
                            }
                        }
                        else {
                            for (int k = 0; k < newLength; k++) {
                                this._insertChild(startOld + k, newChildren[startNew + k]);

                                if (newChildren[startNew + k] is MaterialGap) {
                                    MaterialGap gap = (MaterialGap) newChildren[startNew + k];
                                    this._animationTuples[gap.key].controller.forward();
                                }
                            }

                            j += newLength;
                        }
                    }
                    else {
                        if (oldLength > 1 || oldLength == 1 && this._children[startOld] is MaterialSlice) {
                            float gapSizeSum = 0.0f;

                            while (startOld < j) {
                                if (this._children[startOld] is MaterialGap) {
                                    MaterialGap gap = (MaterialGap) this._children[startOld];
                                    gapSizeSum += gap.size;
                                }

                                this._removeChild(startOld);
                                j--;
                            }

                            if (gapSizeSum != 0.0) {
                                MaterialGap gap = new MaterialGap(key: new UniqueKey(), size: gapSizeSum);
                                this._insertChild(startOld, gap);
                                this._animationTuples[gap.key].gapStart = 0.0f;
                                this._animationTuples[gap.key].controller.setValue(1.0f);
                                this._animationTuples[gap.key].controller.reverse();
                                j++;
                            }
                        }
                        else if (oldLength == 1) {
                            MaterialGap gap = (MaterialGap) this._children[startOld];
                            this._animationTuples[gap.key].gapStart = 0.0f;
                            this._animationTuples[gap.key].controller.reverse();
                        }
                    }
                }
                else {
                    if ((this._children[j] is MaterialGap) == (newChildren[i] is MaterialGap)) {
                        this._children[j] = newChildren[i];

                        i++;
                        j++;
                    }
                    else {
                        D.assert(this._children[j] is MaterialGap);
                        j++;
                    }
                }
            }

            while (j < this._children.Count) {
                this._removeChild(j);
            }

            while (i < newChildren.Count) {
                this._insertChild(j, newChildren[i]);

                i++;
                j++;
            }
        }


        BorderRadius _borderRadius(int index, bool start, bool end) {
            D.assert(MaterialConstantsUtils.kMaterialEdges[MaterialType.card].topLeft ==
                     MaterialConstantsUtils.kMaterialEdges[MaterialType.card].topRight);
            D.assert(MaterialConstantsUtils.kMaterialEdges[MaterialType.card].topLeft ==
                     MaterialConstantsUtils.kMaterialEdges[MaterialType.card].bottomLeft);
            D.assert(MaterialConstantsUtils.kMaterialEdges[MaterialType.card].topLeft ==
                     MaterialConstantsUtils.kMaterialEdges[MaterialType.card].bottomRight);

            Radius cardRadius = MaterialConstantsUtils.kMaterialEdges[MaterialType.card].topLeft;
            Radius startRadius = Radius.zero;
            Radius endRadius = Radius.zero;

            if (index > 0 && this._children[index - 1] is MaterialGap) {
                startRadius = Radius.lerp(
                    Radius.zero,
                    cardRadius,
                    this._animationTuples[this._children[index - 1].key].startAnimation.value);
            }

            if (index < this._children.Count - 2 && this._children[index + 1] is MaterialGap) {
                endRadius = Radius.lerp(
                    Radius.zero,
                    cardRadius,
                    this._animationTuples[this._children[index + 1].key].endAnimation.value);
            }

            if (this.widget.mainAxis == Axis.vertical) {
                return BorderRadius.vertical(
                    top: start ? cardRadius : startRadius,
                    bottom: end ? cardRadius : endRadius);
            }
            else {
                return BorderRadius.horizontal(
                    left: start ? cardRadius : startRadius,
                    right: end ? cardRadius : endRadius);
            }
        }


        float _getGapSize(int index) {
            MaterialGap gap = (MaterialGap) this._children[index];

            return MathUtils.lerpFloat(this._animationTuples[gap.key].gapStart,
                gap.size,
                this._animationTuples[gap.key].gapAnimation.value);
        }


        bool _willNeedDivider(int index) {
            if (index < 0) {
                return false;
            }

            if (index >= this._children.Count) {
                return false;
            }

            return this._children[index] is MaterialSlice || this._isClosingGap(index);
        }


        public override Widget build(BuildContext context) {
            this._removeEmptyGaps();

            List<Widget> widgets = new List<Widget>();
            List<Widget> slices = new List<Widget>();
            int i;

            for (i = 0; i < this._children.Count; i++) {
                if (this._children[i] is MaterialGap) {
                    D.assert(slices.isNotEmpty());
                    widgets.Add(
                        new Container(
                            decoration: new BoxDecoration(
                                color: Theme.of(context).cardColor,
                                borderRadius: this._borderRadius(i - 1, widgets.isEmpty(), false),
                                shape: BoxShape.rectangle),
                            child: new ListBody(
                                mainAxis: this.widget.mainAxis,
                                children: slices)
                        )
                    );

                    slices = new List<Widget>();
                    widgets.Add(
                        new SizedBox(
                            width: this.widget.mainAxis == Axis.horizontal ? this._getGapSize(i) : (float?) null,
                            height: this.widget.mainAxis == Axis.vertical ? this._getGapSize(i) : (float?) null)
                    );
                }
                else {
                    MaterialSlice slice = (MaterialSlice) this._children[i];
                    Widget child = slice.child;

                    if (this.widget.hasDividers) {
                        bool hasTopDivider = this._willNeedDivider(i - 1);
                        bool hasBottomDivider = this._willNeedDivider(i + 1);

                        Border border;
                        BorderSide divider = Divider.createBorderSide(
                            context,
                            width: 0.5f
                        );

                        if (i == 0) {
                            border = new Border(
                                bottom: hasBottomDivider ? divider : BorderSide.none);
                        }
                        else if (i == this._children.Count - 1) {
                            border = new Border(
                                top: hasTopDivider ? divider : BorderSide.none);
                        }
                        else {
                            border = new Border(
                                top: hasTopDivider ? divider : BorderSide.none,
                                bottom: hasBottomDivider ? divider : BorderSide.none
                            );
                        }

                        D.assert(border != null);

                        child = new AnimatedContainer(
                            key: new _MergeableMaterialSliceKey(this._children[i].key),
                            decoration: new BoxDecoration(border: border),
                            duration: ThemeUtils.kThemeAnimationDuration,
                            curve: Curves.fastOutSlowIn,
                            child: child
                        );
                    }

                    slices.Add(
                        new Material(
                            type: MaterialType.transparency,
                            child: child
                        )
                    );
                }
            }

            if (slices.isNotEmpty()) {
                widgets.Add(
                    new Container(
                        decoration: new BoxDecoration(
                            color: Theme.of(context).cardColor,
                            borderRadius: this._borderRadius(i - 1, widgets.isEmpty(), true),
                            shape: BoxShape.rectangle
                        ),
                        child: new ListBody(
                            mainAxis: this.widget.mainAxis,
                            children: slices
                        )
                    )
                );
                slices = new List<Widget>();
            }

            return new _MergeableMaterialListBody(
                mainAxis: this.widget.mainAxis,
                boxShadows: ShadowConstants.kElevationToShadow[this.widget.elevation],
                items: this._children,
                children: widgets
            );
        }
    }


    class _MergeableMaterialSliceKey : GlobalKey {
        public _MergeableMaterialSliceKey(LocalKey value) : base() {
            this.value = value;
        }

        public readonly LocalKey value;

        public bool Equals(_MergeableMaterialSliceKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.value == this.value;
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

            return this.Equals((_MergeableMaterialSliceKey) obj);
        }

        public static bool operator ==(_MergeableMaterialSliceKey left, _MergeableMaterialSliceKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(_MergeableMaterialSliceKey left, _MergeableMaterialSliceKey right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.value.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            return "_MergeableMaterialSliceKey(" + this.value + ")";
        }
    }


    class _MergeableMaterialListBody : ListBody {
        public _MergeableMaterialListBody(
            List<Widget> children = null,
            Axis mainAxis = Axis.vertical,
            List<MergeableMaterialItem> items = null,
            List<BoxShadow> boxShadows = null
        ) : base(children: children, mainAxis: mainAxis) {
            this.items = items;
            this.boxShadows = boxShadows;
        }

        public readonly List<MergeableMaterialItem> items;

        public readonly List<BoxShadow> boxShadows;

        AxisDirection _getDirection(BuildContext context) {
            return AxisDirectionUtils.getAxisDirectionFromAxisReverseAndDirectionality(context, this.mainAxis, false) ??
                   AxisDirection.right;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderMergeableMaterialListBody(
                axisDirection: this._getDirection(context),
                boxShadows: this.boxShadows
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            _RenderMergeableMaterialListBody materialRenderListBody = (_RenderMergeableMaterialListBody) renderObject;
            materialRenderListBody.axisDirection = this._getDirection(context);
            materialRenderListBody.boxShadows = this.boxShadows;
        }
    }


    class _RenderMergeableMaterialListBody : RenderListBody {
        public _RenderMergeableMaterialListBody(
            List<RenderBox> children = null,
            AxisDirection axisDirection = AxisDirection.down,
            List<BoxShadow> boxShadows = null
        ) : base(children: children, axisDirection: axisDirection) {
            this.boxShadows = boxShadows;
        }

        public List<BoxShadow> boxShadows;

        void _paintShadows(Canvas canvas, Rect rect) {
            foreach (BoxShadow boxShadow in this.boxShadows) {
                Paint paint = boxShadow.toPaint();
                canvas.drawRRect(
                    MaterialConstantsUtils.kMaterialEdges[MaterialType.card].toRRect(rect), paint);
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            RenderBox child = this.firstChild;
            int i = 0;

            while (child != null) {
                ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                Rect rect = (childParentData.offset + offset) & child.size;
                if (i % 2 == 0) {
                    this._paintShadows(context.canvas, rect);
                }

                child = childParentData.nextSibling;
                i++;
            }

            this.defaultPaint(context, offset);
        }
    }
}