// AUTO-GENERATED, DO NOT EDIT BY HAND

using System.Collections.Generic;
using UIWidgets.foundation;
using UnityEngine;

namespace UIWidgets.rendering {

    public abstract class RenderObjectWithChildMixinRenderObject<ChildType> : RenderObject where ChildType : RenderObject {
        public ChildType _child;

        public ChildType child {
            get { return this._child; }
            set {
                if (this._child != null) {
                    this.dropChild(this._child);
                }

                this._child = value;
                if (this._child != null) {
                    this.adoptChild(this._child);
                }
            }
        }

        public override void attach(object owner) {
            base.attach(owner);
            if (this._child != null) {
                this._child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            if (this._child != null) {
                this._child.detach();
            }
        }

        public override void redepthChildren() {
            if (this._child != null) {
                this.redepthChild(this._child);
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            if (this._child != null) {
                visitor(this._child);
            }
        }
    }


    public abstract class RenderObjectWithChildMixinRenderBox<ChildType> : RenderBox where ChildType : RenderObject {
        public ChildType _child;

        public ChildType child {
            get { return this._child; }
            set {
                if (this._child != null) {
                    this.dropChild(this._child);
                }

                this._child = value;
                if (this._child != null) {
                    this.adoptChild(this._child);
                }
            }
        }

        public override void attach(object owner) {
            base.attach(owner);
            if (this._child != null) {
                this._child.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            if (this._child != null) {
                this._child.detach();
            }
        }

        public override void redepthChildren() {
            if (this._child != null) {
                this.redepthChild(this._child);
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            if (this._child != null) {
                visitor(this._child);
            }
        }
    }




    public abstract class ContainerParentDataMixinParentData<ChildType> : ParentData, ContainerParentDataMixin<ChildType> where ChildType : RenderObject {
        public ChildType previousSibling { get; set; }

        public ChildType nextSibling { get; set; }

        public override void detach() {
            base.detach();

            if (this.previousSibling != null) {
                var previousSiblingParentData = (ContainerParentDataMixin<ChildType>) this.previousSibling.parentData;
                previousSiblingParentData.nextSibling = this.nextSibling;
            }

            if (this.nextSibling != null) {
                var nextSiblingParentData = (ContainerParentDataMixin<ChildType>) this.nextSibling.parentData;
                nextSiblingParentData.previousSibling = this.previousSibling;
            }

            this.previousSibling = null;
            this.nextSibling = null;
        }
    }



    public abstract class ContainerParentDataMixinBoxParentData<ChildType> : BoxParentData, ContainerParentDataMixin<ChildType> where ChildType : RenderObject {
        public ChildType previousSibling { get; set; }

        public ChildType nextSibling { get; set; }

        public override void detach() {
            base.detach();

            if (this.previousSibling != null) {
                var previousSiblingParentData = (ContainerParentDataMixin<ChildType>) this.previousSibling.parentData;
                previousSiblingParentData.nextSibling = this.nextSibling;
            }

            if (this.nextSibling != null) {
                var nextSiblingParentData = (ContainerParentDataMixin<ChildType>) this.nextSibling.parentData;
                nextSiblingParentData.previousSibling = this.previousSibling;
            }

            this.previousSibling = null;
            this.nextSibling = null;
        }
    }





    public abstract class ContainerRenderObjectMixinRenderBox<ChildType, ParentDataType> : RenderBox
        where ChildType : RenderObject
        where ParentDataType : ParentData, ContainerParentDataMixin<ChildType> {

        public int _childCount = 0;

        public int countCount {
            get { return this._childCount; }
        }

        public ChildType _firstChild;

        public ChildType _lastChild;

        public void _insertIntoChildList(ChildType child, ChildType after = null) {
            var childParentData = (ParentDataType) child.parentData;
            this._childCount++;
            if (after == null) {
                childParentData.nextSibling = this._firstChild;
                if (this._firstChild != null) {
                    var firstChildParentData = (ParentDataType) this._firstChild.parentData;
                    firstChildParentData.previousSibling = child;
                }

                this._firstChild = child;
                if (this._lastChild == null) {
                    this._lastChild = child;
                }
            } else {
                var afterParentData = (ParentDataType) after.parentData;
                if (afterParentData.nextSibling == null) {
                    childParentData.previousSibling = after;
                    afterParentData.nextSibling = child;
                    this._lastChild = child;
                } else {
                    childParentData.nextSibling = afterParentData.nextSibling;
                    childParentData.previousSibling = after;
                    var childPreviousSiblingParentData = (ParentDataType) childParentData.previousSibling.parentData;
                    var childNextSiblingParentData = (ParentDataType) childParentData.nextSibling.parentData;
                    childPreviousSiblingParentData.nextSibling = child;
                    childNextSiblingParentData.previousSibling = child;
                }
            }
        }

        public void insert(ChildType child, ChildType after = null) {
            this.adoptChild(child);
            this._insertIntoChildList(child, after);
        }

        public void add(ChildType child) {
            this.insert(child, this._lastChild);
        }

        public void addAll(List<ChildType> children) {
            if (children != null) {
                children.ForEach(this.add);
            }
        }

        public void _removeFromChildList(ChildType child) {
            var childParentData = (ParentDataType) child.parentData;

            if (childParentData.previousSibling == null) {
                this._firstChild = childParentData.nextSibling;
            } else {
                var childPreviousSiblingParentData = (ParentDataType) childParentData.previousSibling.parentData;
                childPreviousSiblingParentData.nextSibling = childParentData.nextSibling;
            }

            if (childParentData.nextSibling == null) {
                this._lastChild = childParentData.previousSibling;
            } else {
                var childNextSiblingParentData = (ParentDataType) childParentData.nextSibling.parentData;
                childNextSiblingParentData.previousSibling = childParentData.previousSibling;
            }

            childParentData.previousSibling = null;
            childParentData.nextSibling = null;
            this._childCount--;
        }

        public void remove(ChildType child) {
            this._removeFromChildList(child);
            this.dropChild(child);
        }

        public void removeAll() {
            ChildType child = this._firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                var next = childParentData.nextSibling;
                childParentData.previousSibling = null;
                childParentData.nextSibling = null;
                this.dropChild(child);
                child = next;
            }
            this._firstChild = null;
            this._lastChild = null;
            this._childCount = 0;
        }

        public void move(ChildType child, ChildType after = null) {
            var childParentData = (ParentDataType) child.parentData;
            if (childParentData.previousSibling == after) {
                return;
            }
            
            this._removeFromChildList(child);
            this._insertIntoChildList(child, after);
            this.markNeedsLayout();
        }

        public override void attach(object owner) {
            base.attach(owner);
            ChildType child = this._firstChild;
            while (child != null) {
                child.attach(owner);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void detach() {
            base.detach();
            ChildType child = this._firstChild;
            while (child != null) {
                child.detach();
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void redepthChildren() {
            ChildType child = this._firstChild;
            while (child != null) {
                this.redepthChild(child);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            ChildType child = this._firstChild;
            while (child != null) {
                visitor(child);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public ChildType firstChild {
            get { return this._firstChild; }
        }

        public ChildType lastChild {
            get { return this._lastChild; }
        }

        public ChildType childBefore(ChildType child) {
            var childParentData = (ParentDataType) child.parentData;
            return childParentData.previousSibling;
        }
        
        public ChildType childAfter(ChildType child) {
            var childParentData = (ParentDataType) child.parentData;
            return childParentData.nextSibling;
        }
    }


    public abstract class ContainerRenderObjectMixinRenderSliver<ChildType, ParentDataType> : RenderSliver
        where ChildType : RenderObject
        where ParentDataType : ParentData, ContainerParentDataMixin<ChildType> {

        public int _childCount = 0;

        public int countCount {
            get { return this._childCount; }
        }

        public ChildType _firstChild;

        public ChildType _lastChild;

        public void _insertIntoChildList(ChildType child, ChildType after = null) {
            var childParentData = (ParentDataType) child.parentData;
            this._childCount++;
            if (after == null) {
                childParentData.nextSibling = this._firstChild;
                if (this._firstChild != null) {
                    var firstChildParentData = (ParentDataType) this._firstChild.parentData;
                    firstChildParentData.previousSibling = child;
                }

                this._firstChild = child;
                if (this._lastChild == null) {
                    this._lastChild = child;
                }
            } else {
                var afterParentData = (ParentDataType) after.parentData;
                if (afterParentData.nextSibling == null) {
                    childParentData.previousSibling = after;
                    afterParentData.nextSibling = child;
                    this._lastChild = child;
                } else {
                    childParentData.nextSibling = afterParentData.nextSibling;
                    childParentData.previousSibling = after;
                    var childPreviousSiblingParentData = (ParentDataType) childParentData.previousSibling.parentData;
                    var childNextSiblingParentData = (ParentDataType) childParentData.nextSibling.parentData;
                    childPreviousSiblingParentData.nextSibling = child;
                    childNextSiblingParentData.previousSibling = child;
                }
            }
        }

        public void insert(ChildType child, ChildType after = null) {
            this.adoptChild(child);
            this._insertIntoChildList(child, after);
        }

        public void add(ChildType child) {
            this.insert(child, this._lastChild);
        }

        public void addAll(List<ChildType> children) {
            if (children != null) {
                children.ForEach(this.add);
            }
        }

        public void _removeFromChildList(ChildType child) {
            var childParentData = (ParentDataType) child.parentData;

            if (childParentData.previousSibling == null) {
                this._firstChild = childParentData.nextSibling;
            } else {
                var childPreviousSiblingParentData = (ParentDataType) childParentData.previousSibling.parentData;
                childPreviousSiblingParentData.nextSibling = childParentData.nextSibling;
            }

            if (childParentData.nextSibling == null) {
                this._lastChild = childParentData.previousSibling;
            } else {
                var childNextSiblingParentData = (ParentDataType) childParentData.nextSibling.parentData;
                childNextSiblingParentData.previousSibling = childParentData.previousSibling;
            }

            childParentData.previousSibling = null;
            childParentData.nextSibling = null;
            this._childCount--;
        }

        public void remove(ChildType child) {
            this._removeFromChildList(child);
            this.dropChild(child);
        }

        public void removeAll() {
            ChildType child = this._firstChild;
            while (child != null) {
                var childParentData = (ParentDataType) child.parentData;
                var next = childParentData.nextSibling;
                childParentData.previousSibling = null;
                childParentData.nextSibling = null;
                this.dropChild(child);
                child = next;
            }
            this._firstChild = null;
            this._lastChild = null;
            this._childCount = 0;
        }

        public void move(ChildType child, ChildType after = null) {
            var childParentData = (ParentDataType) child.parentData;
            if (childParentData.previousSibling == after) {
                return;
            }
            
            this._removeFromChildList(child);
            this._insertIntoChildList(child, after);
            this.markNeedsLayout();
        }

        public override void attach(object owner) {
            base.attach(owner);
            ChildType child = this._firstChild;
            while (child != null) {
                child.attach(owner);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void detach() {
            base.detach();
            ChildType child = this._firstChild;
            while (child != null) {
                child.detach();
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void redepthChildren() {
            ChildType child = this._firstChild;
            while (child != null) {
                this.redepthChild(child);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            ChildType child = this._firstChild;
            while (child != null) {
                visitor(child);
                var childParentData = (ParentDataType) child.parentData;
                child = childParentData.nextSibling;
            }
        }

        public ChildType firstChild {
            get { return this._firstChild; }
        }

        public ChildType lastChild {
            get { return this._lastChild; }
        }

        public ChildType childBefore(ChildType child) {
            var childParentData = (ParentDataType) child.parentData;
            return childParentData.previousSibling;
        }
        
        public ChildType childAfter(ChildType child) {
            var childParentData = (ParentDataType) child.parentData;
            return childParentData.nextSibling;
        }
    }



}