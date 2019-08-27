using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public enum TableCellVerticalAlignment {
        top,
        middle,
        bottom,
        baseline,
        fill
    }

    public class TableCellParentData : BoxParentData {
        public TableCellVerticalAlignment? verticalAlignment;

        public int x;

        public int y;

        public override string ToString() {
            return
                base.ToString() + "; " + (this.verticalAlignment == null
                    ? "default vertical alignment"
                    : this.verticalAlignment.ToString());
        }
    }

    public abstract class TableColumnWidth {
        protected TableColumnWidth() {
        }

        public abstract float minIntrinsicWidth(List<RenderBox> cells, float containerWidth);

        public abstract float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth);

        public virtual float? flex(List<RenderBox> cells) {
            return null;
        }

        public override string ToString() {
            return this.GetType().ToString();
        }
    }


    public class IntrinsicColumnWidth : TableColumnWidth {
        public IntrinsicColumnWidth(
            float? flex = null) {
            this._flex = flex;
        }

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            float result = 0.0f;
            foreach (RenderBox cell in cells) {
                result = Mathf.Max(result, cell.getMinIntrinsicWidth(float.PositiveInfinity));
            }

            return result;
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            float result = 0.0f;
            foreach (RenderBox cell in cells) {
                result = Mathf.Max(result, cell.getMaxIntrinsicWidth(float.PositiveInfinity));
            }

            return result;
        }

        public override float? flex(List<RenderBox> cells) {
            return this._flex;
        }

        readonly float? _flex;

        public override string ToString() {
            return $"${this.GetType()}(flex: {this._flex})";
        }
    }


    public class FixedColumnWidth : TableColumnWidth {
        public FixedColumnWidth(float? value = null) {
            D.assert(value != null);
            this.value = value.Value;
        }

        public readonly float value;

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return this.value;
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return this.value;
        }

        public override string ToString() {
            return $"{this.GetType()}({this.value})";
        }
    }


    public class FractionColumnWidth : TableColumnWidth {
        public FractionColumnWidth(float? value = null) {
            D.assert(value != null);
            this.value = value.Value;
        }

        public readonly float value;

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            if (!containerWidth.isFinite()) {
                return 0.0f;
            }

            return this.value * containerWidth;
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            if (!containerWidth.isFinite()) {
                return 0.0f;
            }

            return this.value * containerWidth;
        }

        public override string ToString() {
            return $"{this.GetType()}({this.value})";
        }
    }

    public class FlexColumnWidth : TableColumnWidth {
        public FlexColumnWidth(float value = 1.0f) {
            this.value = value;
        }

        public readonly float value;

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return 0.0f;
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return 0.0f;
        }

        public override float? flex(List<RenderBox> cells) {
            return this.value;
        }

        public override string ToString() {
            return $"{this.GetType()}({this.value})";
        }
    }

    public class MaxColumnWidth : TableColumnWidth {
        public MaxColumnWidth(
            TableColumnWidth a, TableColumnWidth b) {
            this.a = a;
            this.b = b;
        }

        public readonly TableColumnWidth a;

        public readonly TableColumnWidth b;

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return Mathf.Max(
                this.a.minIntrinsicWidth(cells, containerWidth),
                this.b.minIntrinsicWidth(cells, containerWidth)
            );
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return Mathf.Max(
                this.a.maxIntrinsicWidth(cells, containerWidth),
                this.b.maxIntrinsicWidth(cells, containerWidth)
            );
        }

        public override float? flex(List<RenderBox> cells) {
            float? aFlex = this.a.flex(cells);
            if (aFlex == null) {
                return this.b.flex(cells);
            }

            float? bFlex = this.b.flex(cells);
            if (bFlex == null) {
                return null;
            }

            return Mathf.Max(aFlex.Value, bFlex.Value);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.a}, {this.b})";
        }
    }

    public class MinColumnWidth : TableColumnWidth {
        public MinColumnWidth(
            TableColumnWidth a, TableColumnWidth b) {
            this.a = a;
            this.b = b;
        }

        public readonly TableColumnWidth a;

        public readonly TableColumnWidth b;

        public override float minIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return Mathf.Min(
                this.a.minIntrinsicWidth(cells, containerWidth),
                this.b.minIntrinsicWidth(cells, containerWidth)
            );
        }

        public override float maxIntrinsicWidth(List<RenderBox> cells, float containerWidth) {
            return Mathf.Min(
                this.a.maxIntrinsicWidth(cells, containerWidth),
                this.b.maxIntrinsicWidth(cells, containerWidth)
            );
        }

        public override float? flex(List<RenderBox> cells) {
            float? aFlex = this.a.flex(cells);
            if (aFlex == null) {
                return this.b.flex(cells);
            }

            float? bFlex = this.b.flex(cells);
            if (bFlex == null) {
                return null;
            }

            return Mathf.Min(aFlex.Value, bFlex.Value);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.a}, {this.b})";
        }
    }

    public class RenderTable : RenderBox {
        public RenderTable(
            int? columns = null,
            int? rows = null,
            Dictionary<int, TableColumnWidth> columnWidths = null,
            TableColumnWidth defaultColumnWidth = null,
            TableBorder border = null,
            List<Decoration> rowDecorations = null,
            ImageConfiguration configuration = null,
            TableCellVerticalAlignment defaultVerticalAlignment = TableCellVerticalAlignment.top,
            TextBaseline? textBaseline = null,
            List<List<RenderBox>> children = null
        ) {
            defaultColumnWidth = defaultColumnWidth ?? new FlexColumnWidth(1.0f);
            configuration = configuration ?? ImageConfiguration.empty;
            D.assert(columns == null || columns >= 0);
            D.assert(rows == null || rows >= 0);
            D.assert(rows == null || children == null);

            this._columns = columns ?? (children != null && children.isNotEmpty() ? children[0].Count : 0);
            this._rows = rows ?? 0;
            this._children = new List<RenderBox>();
            for (int i = 0; i < this._columns * this._rows; i++) {
                this._children.Add(null);
            }

            this._columnWidths = columnWidths ?? new Dictionary<int, TableColumnWidth>();
            this._defaultColumnWidth = defaultColumnWidth;
            this._border = border;
            this.rowDecorations = rowDecorations;
            this._configuration = configuration;
            this._defaultVerticalAlignment = defaultVerticalAlignment;
            this._textBaseline = textBaseline;

            if (children != null) {
                foreach (List<RenderBox> row in children) {
                    this.addRow(row);
                }
            }
        }

        List<RenderBox> _children = new List<RenderBox>();

        public int columns {
            get { return this._columns; }
            set {
                D.assert(value >= 0);
                if (value == this.columns) {
                    return;
                }

                int oldColumns = this.columns;
                List<RenderBox> oldChildren = this._children;
                this._columns = value;
                this._children = new List<RenderBox>();
                for (int i = 0; i < this.columns * this.rows; i++) {
                    this._children.Add(null);
                }

                int columnsToCopy = Mathf.Min(this.columns, oldColumns);
                for (int y = 0; y < this.rows; y++) {
                    for (int x = 0; x < columnsToCopy; x++) {
                        this._children[x + y * this.columns] = oldChildren[x + y * oldColumns];
                    }
                }

                if (oldColumns > this.columns) {
                    for (int y = 0; y < this.rows; y++) {
                        for (int x = this.columns; x < oldColumns; x++) {
                            int xy = x + y * oldColumns;
                            if (oldChildren[xy] != null) {
                                this.dropChild(oldChildren[xy]);
                            }
                        }
                    }
                }

                this.markNeedsLayout();
            }
        }

        int _columns;

        public int rows {
            get { return this._rows; }
            set {
                D.assert(value >= 0);
                if (value == this.rows) {
                    return;
                }

                if (this._rows > value) {
                    for (int xy = this.columns * value; xy < this._children.Count; xy++) {
                        if (this._children[xy] != null) {
                            this.dropChild(this._children[xy]);
                        }
                    }
                }

                this._rows = value;
                if (this._children.Count > this.columns * this.rows) {
                    this._children.RemoveRange(this.columns * this.rows,
                        this._children.Count - this.columns * this.rows);
                }
                else if (this._children.Count < this.columns * this.rows) {
                    while (this._children.Count < this.columns * this.rows) {
                        this._children.Add(null);
                    }
                }

                D.assert(this._children.Count == this.columns * this.rows);

                this.markNeedsLayout();
            }
        }

        int _rows;

        public Dictionary<int, TableColumnWidth> columnWidths {
            get { return this._columnWidths; }
            set {
                value = value ?? new Dictionary<int, TableColumnWidth>();
                if (this._columnWidths == value) {
                    return;
                }

                this._columnWidths = value;
                this.markNeedsLayout();
            }
        }

        Dictionary<int, TableColumnWidth> _columnWidths;

        public void setColumnWidth(int column, TableColumnWidth value) {
            if (this._columnWidths.getOrDefault(column) == value) {
                return;
            }

            this._columnWidths[column] = value;
            this.markNeedsLayout();
            ;
        }

        public TableColumnWidth defaultColumnWidth {
            get { return this._defaultColumnWidth; }
            set {
                D.assert(value != null);
                if (this.defaultColumnWidth == value) {
                    return;
                }

                this._defaultColumnWidth = value;
                this.markNeedsLayout();
            }
        }

        TableColumnWidth _defaultColumnWidth;


        public TableBorder border {
            get { return this._border; }
            set {
                if (this.border == value) {
                    return;
                }

                this._border = value;
                this.markNeedsPaint();
            }
        }

        TableBorder _border;


        public List<Decoration> rowDecorations {
            get { return this._rowDecorations ?? new List<Decoration>(); }
            set {
                if (this._rowDecorations == value) {
                    return;
                }

                this._rowDecorations = value;
                if (this._rowDecorationPainters != null) {
                    foreach (BoxPainter painter in this._rowDecorationPainters) {
                        painter?.Dispose();
                    }
                }

                if (this._rowDecorations != null) {
                    this._rowDecorationPainters = new List<BoxPainter>();
                    for (int i = 0; i < this._rowDecorations.Count; i++) {
                        this._rowDecorationPainters.Add(null);
                    }
                }
                else {
                    this._rowDecorationPainters = null;
                }
            }
        }

        List<Decoration> _rowDecorations;
        List<BoxPainter> _rowDecorationPainters;

        public ImageConfiguration configuration {
            get { return this._configuration; }
            set {
                D.assert(value != null);
                if (value == this._configuration) {
                    return;
                }

                this._configuration = value;
                this.markNeedsPaint();
            }
        }

        ImageConfiguration _configuration;

        public TableCellVerticalAlignment defaultVerticalAlignment {
            get { return this._defaultVerticalAlignment; }
            set {
                if (this._defaultVerticalAlignment == value) {
                    return;
                }

                this._defaultVerticalAlignment = value;
                this.markNeedsLayout();
            }
        }

        TableCellVerticalAlignment _defaultVerticalAlignment;

        public TextBaseline? textBaseline {
            get { return this._textBaseline; }
            set {
                if (this._textBaseline == value) {
                    return;
                }

                this._textBaseline = value;
                this.markNeedsLayout();
            }
        }

        TextBaseline? _textBaseline;

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is TableCellParentData)) {
                child.parentData = new TableCellParentData();
            }
        }

        public void setFlatChildren(int columns, List<RenderBox> cells) {
            if (cells == this._children && columns == this._columns) {
                return;
            }

            D.assert(columns >= 0);

            if (columns == 0 || cells.isEmpty()) {
                D.assert(cells == null || cells.isEmpty());
                this._columns = columns;
                if (this._children.isEmpty()) {
                    D.assert(this._rows == 0);
                    return;
                }

                foreach (RenderBox oldChild in this._children) {
                    if (oldChild != null) {
                        this.dropChild(oldChild);
                    }
                }

                this._rows = 0;
                this._children.Clear();
                this.markNeedsLayout();
                return;
            }

            D.assert(cells != null);
            D.assert(cells.Count % columns == 0);

            HashSet<RenderBox> lostChildren = new HashSet<RenderBox>();
            int y, x;
            for (y = 0; y < this._rows; y++) {
                for (x = 0; x < this._columns; x++) {
                    int xyOld = x + y * this._columns;
                    int xyNew = x + y * columns;
                    if (this._children[xyOld] != null &&
                        (x >= columns || xyNew >= cells.Count || this._children[xyOld] != cells[xyNew])) {
                        lostChildren.Add(this._children[xyOld]);
                    }
                }
            }

            y = 0;
            while (y * columns < cells.Count) {
                for (x = 0; x < columns; x++) {
                    int xyNew = x + y * columns;
                    int xyOld = x + y * this._columns;
                    if (cells[xyNew] != null &&
                        (x >= this._columns || y >= this._rows || this._children[xyOld] != cells[xyNew])) {
                        if (lostChildren.Contains(cells[xyNew])) {
                            lostChildren.Remove(cells[xyNew]);
                        }
                        else {
                            this.adoptChild(cells[xyNew]);
                        }
                    }
                }

                y += 1;
            }

            foreach (RenderBox child in lostChildren) {
                this.dropChild(child);
            }

            this._columns = columns;
            this._rows = cells.Count / columns;
            this._children = cells;
            D.assert(this._children.Count == this.rows * this.columns);
            this.markNeedsLayout();
        }


        void setChildren(List<List<RenderBox>> cells) {
            if (cells == null) {
                this.setFlatChildren(0, null);
                return;
            }

            foreach (RenderBox oldChild in this._children) {
                if (oldChild != null) {
                    this.dropChild(oldChild);
                }
            }

            this._children.Clear();
            this._columns = cells.isNotEmpty() ? cells[0].Count : 0;
            this._rows = 0;
            foreach (List<RenderBox> row in cells) {
                this.addRow(row);
            }

            D.assert(this._children.Count == this.rows * this.columns);
        }


        void addRow(List<RenderBox> cells) {
            D.assert(cells.Count == this.columns);
            D.assert(this._children.Count == this.rows * this.columns);

            this._rows += 1;
            this._children.AddRange(cells);
            foreach (RenderBox cell in cells) {
                if (cell != null) {
                    this.adoptChild(cell);
                }
            }

            this.markNeedsLayout();
        }

        public void setChild(int x, int y, RenderBox value) {
            D.assert(x >= 0 && x < this.columns && y >= 0 && y < this.rows);
            D.assert(this._children.Count == this.rows * this.columns);

            int xy = x + y * this.columns;
            RenderBox oldChild = this._children[xy];
            if (oldChild == value) {
                return;
            }

            if (oldChild != null) {
                this.dropChild(oldChild);
            }

            this._children[xy] = value;
            if (value != null) {
                this.adoptChild(value);
            }
        }


        public override void attach(object owner) {
            base.attach(owner);
            foreach (RenderBox child in this._children) {
                child?.attach(owner);
            }
        }

        public override void detach() {
            base.detach();
            if (this._rowDecorationPainters != null) {
                foreach (BoxPainter painter in this._rowDecorationPainters) {
                    painter?.Dispose();
                }

                this._rowDecorationPainters = null;
            }

            foreach (RenderBox child in this._children) {
                child?.detach();
            }
        }

        public override void visitChildren(RenderObjectVisitor visitor) {
            D.assert(this._children.Count == this.rows * this.columns);
            foreach (RenderBox child in this._children) {
                if (child != null) {
                    visitor(child);
                }
            }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            D.assert(this._children.Count == this.rows * this.columns);
            float totalMinWidth = 0.0f;
            for (int x = 0; x < this.columns; x++) {
                TableColumnWidth columnWidth = this._columnWidths.getOrDefault(x) ?? this.defaultColumnWidth;
                List<RenderBox> columnCells = this.column(x);
                totalMinWidth += columnWidth.minIntrinsicWidth(columnCells, float.PositiveInfinity);
            }

            return totalMinWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            D.assert(this._children.Count == this.rows * this.columns);
            float totalMaxWidth = 0.0f;
            for (int x = 0; x < this.columns; x++) {
                TableColumnWidth columnWidth = this._columnWidths.getOrDefault(x) ?? this.defaultColumnWidth;
                List<RenderBox> columnCells = this.column(x);
                totalMaxWidth += columnWidth.maxIntrinsicWidth(columnCells, float.PositiveInfinity);
            }

            return totalMaxWidth;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            D.assert(this._children.Count == this.rows * this.columns);
            List<float> widths = this._computeColumnWidths(BoxConstraints.tightForFinite(width: width));
            float rowTop = 0.0f;
            for (int y = 0; y < this.rows; y++) {
                float rowHeight = 0.0f;
                for (int x = 0; x < this.columns; x++) {
                    int xy = x + y * this.columns;
                    RenderBox child = this._children[xy];
                    if (child != null) {
                        rowHeight = Mathf.Max(rowHeight, child.getMaxIntrinsicHeight(widths[x]));
                    }
                }

                rowTop += rowHeight;
            }

            return rowTop;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return this.computeMinIntrinsicHeight(width);
        }

        float? _baselineDistance;

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return this._baselineDistance;
        }

        List<RenderBox> column(int x) {
            List<RenderBox> ret = new List<RenderBox>();
            for (int y = 0; y < this.rows; y++) {
                int xy = x + y * this.columns;
                RenderBox child = this._children[xy];
                if (child != null) {
                    ret.Add(child);
                }
            }

            return ret;
        }

        List<RenderBox> row(int y) {
            List<RenderBox> ret = new List<RenderBox>();
            int start = y * this.columns;
            int end = (y + 1) * this.columns;
            for (int xy = start; xy < end; xy++) {
                RenderBox child = this._children[xy];
                if (child != null) {
                    ret.Add(child);
                }
            }

            return ret;
        }

        List<float> _computeColumnWidths(BoxConstraints constraints) {
            D.assert(constraints != null);
            D.assert(this._children.Count == this.rows * this.columns);

            List<float> widths = new List<float>();
            List<float> minWidths = new List<float>();
            List<float?> flexes = new List<float?>();

            for (int i = 0; i < this.columns; i++) {
                widths.Add(0.0f);
                minWidths.Add(0.0f);
                flexes.Add(null);
            }

            float tableWidth = 0.0f;
            float? unflexedTableWidth = 0.0f;
            float totalFlex = 0.0f;

            for (int x = 0; x < this.columns; x++) {
                TableColumnWidth columnWidth = this._columnWidths.getOrDefault(x) ?? this.defaultColumnWidth;
                List<RenderBox> columnCells = this.column(x);

                float maxIntrinsicWidth = columnWidth.maxIntrinsicWidth(columnCells, constraints.maxWidth);
                D.assert(maxIntrinsicWidth.isFinite());
                D.assert(maxIntrinsicWidth >= 0.0f);
                widths[x] = maxIntrinsicWidth;
                tableWidth += maxIntrinsicWidth;

                float minIntrinsicWidth = columnWidth.minIntrinsicWidth(columnCells, constraints.maxWidth);
                D.assert(minIntrinsicWidth.isFinite());
                D.assert(minIntrinsicWidth >= 0.0f);
                minWidths[x] = minIntrinsicWidth;
                D.assert(maxIntrinsicWidth >= minIntrinsicWidth);

                float? flex = columnWidth.flex(columnCells);
                if (flex != null) {
                    D.assert(flex.Value.isFinite());
                    D.assert(flex.Value > 0.0f);
                    flexes[x] = flex;
                    totalFlex += flex.Value;
                }
                else {
                    unflexedTableWidth += maxIntrinsicWidth;
                }
            }

            float maxWidthConstraint = constraints.maxWidth;
            float minWidthConstraint = constraints.minWidth;

            if (totalFlex > 0.0f) {
                float targetWidth = 0.0f;
                if (maxWidthConstraint.isFinite()) {
                    targetWidth = maxWidthConstraint;
                }
                else {
                    targetWidth = minWidthConstraint;
                }

                if (tableWidth < targetWidth) {
                    float remainingWidth = targetWidth - unflexedTableWidth.Value;
                    D.assert(remainingWidth.isFinite());
                    D.assert(remainingWidth >= 0.0f);
                    for (int x = 0; x < this.columns; x++) {
                        if (flexes[x] != null) {
                            float flexedWidth = remainingWidth * flexes[x].Value / totalFlex;
                            D.assert(flexedWidth.isFinite());
                            D.assert(flexedWidth >= 0.0f);
                            if (widths[x] < flexedWidth) {
                                float delta = flexedWidth - widths[x];
                                tableWidth += delta;
                                widths[x] = flexedWidth;
                            }
                        }
                    }

                    D.assert(tableWidth >= targetWidth);
                }
            }
            else if (tableWidth < minWidthConstraint) {
                float delta = (minWidthConstraint - tableWidth) / this.columns;
                for (int x = 0; x < this.columns; x++) {
                    widths[x] += delta;
                }

                tableWidth = minWidthConstraint;
            }

            D.assert(() => {
                unflexedTableWidth = null;
                return true;
            });

            if (tableWidth > maxWidthConstraint) {
                float deficit = tableWidth - maxWidthConstraint;

                int availableColumns = this.columns;
                float minimumDeficit = 0.00000001f;
                while (deficit > 0.0f && totalFlex > minimumDeficit) {
                    float newTotalFlex = 0.0f;
                    for (int x = 0; x < this.columns; x++) {
                        if (flexes[x] != null) {
                            float newWidth = widths[x] - deficit * flexes[x].Value / totalFlex;
                            D.assert(newWidth.isFinite());
                            if (newWidth <= minWidths[x]) {
                                deficit -= widths[x] - minWidths[x];
                                widths[x] = minWidths[x];
                                flexes[x] = null;
                                availableColumns -= 1;
                            }
                            else {
                                deficit -= widths[x] - newWidth;
                                widths[x] = newWidth;
                                newTotalFlex += flexes[x].Value;
                            }

                            D.assert(widths[x] >= 0.0f);
                        }
                    }

                    totalFlex = newTotalFlex;
                }

                if (deficit > 0.0f) {
                    do {
                        float delta = deficit / availableColumns;
                        int newAvailableColumns = 0;
                        for (int x = 0; x < this.columns; x++) {
                            float availableDelta = widths[x] - minWidths[x];
                            if (availableDelta > 0.0f) {
                                if (availableDelta <= delta) {
                                    deficit -= widths[x] - minWidths[x];
                                    widths[x] = minWidths[x];
                                }
                                else {
                                    deficit -= availableDelta;
                                    widths[x] -= availableDelta;
                                    newAvailableColumns += 1;
                                }
                            }
                        }

                        availableColumns = newAvailableColumns;
                    } while (deficit > 0.0f && availableColumns > 0);
                }
            }

            return widths;
        }

        readonly List<float> _rowTops = new List<float>();
        List<float> _columnLefts;

        Rect getRowBox(int row) {
            D.assert(row >= 0);
            D.assert(row < this.rows);

            return Rect.fromLTRB(0.0f, this._rowTops[row], this.size.width, this._rowTops[row + 1]);
        }

        protected override void performLayout() {
            int rows = this.rows;
            int columns = this.columns;
            D.assert(this._children.Count == rows * columns);
            if (rows * columns == 0) {
                this.size = this.constraints.constrain(new Size(0.0f, 0.0f));
                return;
            }

            List<float> widths = this._computeColumnWidths(this.constraints);
            List<float> positions = new List<float>();
            float tableWidth = 0.0f;
            positions.Add(0.0f);
            for (int x = 1; x < columns; x++) {
                positions.Add(positions[x - 1] + widths[x - 1]);
            }

            this._columnLefts = positions;
            tableWidth = positions[positions.Count - 1] + widths[widths.Count - 1];

            this._rowTops.Clear();
            this._baselineDistance = null;

            float rowTop = 0.0f;
            for (int y = 0; y < rows; y++) {
                this._rowTops.Add(rowTop);
                float rowHeight = 0.0f;
                bool haveBaseline = false;
                float beforeBaselineDistance = 0.0f;
                float afterBaselineDistance = 0.0f;
                List<float?> baselines = new List<float?>();
                for (int i = 0; i < columns; i++) {
                    baselines.Add(null);
                }

                for (int x = 0; x < columns; x++) {
                    int xy = x + y * columns;
                    RenderBox child = this._children[xy];
                    if (child != null) {
                        TableCellParentData childParentData = (TableCellParentData) child.parentData;
                        D.assert(childParentData != null);
                        childParentData.x = x;
                        childParentData.y = y;
                        switch (childParentData.verticalAlignment ?? this.defaultVerticalAlignment) {
                            case TableCellVerticalAlignment.baseline: {
                                D.assert(this.textBaseline != null);
                                child.layout(BoxConstraints.tightFor(width: widths[x]), parentUsesSize: true);
                                float? childBaseline =
                                    child.getDistanceToBaseline(this.textBaseline.Value, onlyReal: true);
                                if (childBaseline != null) {
                                    beforeBaselineDistance = Mathf.Max(beforeBaselineDistance, childBaseline.Value);
                                    afterBaselineDistance = Mathf.Max(afterBaselineDistance,
                                        child.size.height - childBaseline.Value);
                                    baselines[x] = childBaseline.Value;
                                    haveBaseline = true;
                                }
                                else {
                                    rowHeight = Mathf.Max(rowHeight, child.size.height);
                                    childParentData.offset = new Offset(positions[x], rowTop);
                                }

                                break;
                            }
                            case TableCellVerticalAlignment.top:
                            case TableCellVerticalAlignment.middle:
                            case TableCellVerticalAlignment.bottom: {
                                child.layout(BoxConstraints.tightFor(width: widths[x]), parentUsesSize: true);
                                rowHeight = Mathf.Max(rowHeight, child.size.height);
                                break;
                            }
                            case TableCellVerticalAlignment.fill: {
                                break;
                            }
                        }
                    }
                }

                if (haveBaseline) {
                    if (y == 0) {
                        this._baselineDistance = beforeBaselineDistance;
                    }

                    rowHeight = Mathf.Max(rowHeight, beforeBaselineDistance + afterBaselineDistance);
                }

                for (int x = 0; x < columns; x++) {
                    int xy = x + y * columns;
                    RenderBox child = this._children[xy];
                    if (child != null) {
                        TableCellParentData childParentData = (TableCellParentData) child.parentData;
                        switch (childParentData.verticalAlignment ?? this.defaultVerticalAlignment) {
                            case TableCellVerticalAlignment.baseline: {
                                if (baselines[x] != null) {
                                    childParentData.offset = new Offset(positions[x],
                                        rowTop + beforeBaselineDistance - baselines[x].Value);
                                }

                                break;
                            }
                            case TableCellVerticalAlignment.top: {
                                childParentData.offset = new Offset(positions[x], rowTop);
                                break;
                            }
                            case TableCellVerticalAlignment.middle: {
                                childParentData.offset = new Offset(positions[x],
                                    rowTop + (rowHeight - child.size.height) / 2.0f);
                                break;
                            }
                            case TableCellVerticalAlignment.bottom: {
                                childParentData.offset =
                                    new Offset(positions[x], rowTop + rowHeight - child.size.height);
                                break;
                            }
                            case TableCellVerticalAlignment.fill: {
                                child.layout(BoxConstraints.tightFor(width: widths[x], height: rowHeight));
                                childParentData.offset = new Offset(positions[x], rowTop);
                                break;
                            }
                        }
                    }
                }

                rowTop += rowHeight;
            }

            this._rowTops.Add(rowTop);
            this.size = this.constraints.constrain(new Size(tableWidth, rowTop));
            D.assert(this._rowTops.Count == rows + 1);
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            D.assert(this._children.Count == this.rows * this.columns);
            for (int index = this._children.Count - 1; index >= 0; index--) {
                RenderBox child = this._children[index];
                if (child != null) {
                    BoxParentData childParentData = (BoxParentData) child.parentData;
                    if (child.hitTest(result, position: position - childParentData.offset)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void paint(PaintingContext context, Offset offset) {
            D.assert(this._children.Count == this.rows * this.columns);
            if (this.rows * this.columns == 0) {
                if (this.border != null) {
                    Rect borderRect = Rect.fromLTWH(offset.dx, offset.dy, this.size.width, 0.0f);
                    this.border.paint(context.canvas, borderRect, rows: new List<float>(), columns: new List<float>());
                }

                return;
            }

            D.assert(this._rowTops.Count == this.rows + 1);
            if (this._rowDecorations != null) {
                Canvas canvas = context.canvas;
                for (int y = 0; y < this.rows; y++) {
                    if (this._rowDecorations.Count <= y) {
                        break;
                    }

                    if (this._rowDecorations[y] != null) {
                        this._rowDecorationPainters[y] = this._rowDecorationPainters[y] ??
                                                         this._rowDecorations[y].createBoxPainter(this.markNeedsPaint);
                        this._rowDecorationPainters[y].paint(
                            canvas,
                            new Offset(offset.dx, offset.dy + this._rowTops[y]),
                            this.configuration.copyWith(
                                size: new Size(this.size.width, this._rowTops[y + 1] - this._rowTops[y])
                            )
                        );
                    }
                }
            }

            for (int index = 0; index < this._children.Count; index++) {
                RenderBox child = this._children[index];
                if (child != null) {
                    BoxParentData childParentData = (BoxParentData) child.parentData;
                    context.paintChild(child, childParentData.offset + offset);
                }
            }

            D.assert(this._rows == this._rowTops.Count - 1);
            D.assert(this._columns == this._columnLefts.Count);
            if (this.border != null) {
                Rect borderRect = Rect.fromLTWH(offset.dx, offset.dy, this.size.width, this._rowTops[this._rowTops.Count - 1]);
                List<float> rows = this._rowTops.GetRange(1, this._rowTops.Count - 2);
                List<float> columns = this._columnLefts.GetRange(1, this._columnLefts.Count - 1);
                this.border.paint(context.canvas, borderRect, rows: rows, columns: columns);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<TableBorder>("border", this.border, defaultValue: null));
            properties.add(new DiagnosticsProperty<Dictionary<int, TableColumnWidth>>("specified column widths",
                this._columnWidths,
                level: this._columnWidths.isEmpty() ? DiagnosticLevel.hidden : DiagnosticLevel.info));
            properties.add(new DiagnosticsProperty<TableColumnWidth>("default column width", this.defaultColumnWidth));
            properties.add(new MessageProperty("table size", $"{this.columns}*{this.rows}"));
            properties.add(new EnumerableProperty<float>("column offsets", this._columnLefts, ifNull: "unknown"));
            properties.add(new EnumerableProperty<float>("row offsets", this._rowTops, ifNull: "unknown"));
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            if (this._children.isEmpty()) {
                return new List<DiagnosticsNode> {DiagnosticsNode.message("table is empty")};
            }

            List<DiagnosticsNode> children = new List<DiagnosticsNode>();
            for (int y = 0; y < this.rows; y++) {
                for (int x = 0; x < this.columns; x++) {
                    int xy = x + y * this.columns;
                    RenderBox child = this._children[xy];
                    string name = $"child ({x}, {y})";
                    if (child != null) {
                        children.Add(child.toDiagnosticsNode(name: name));
                    }
                    else {
                        children.Add(new DiagnosticsProperty<object>(name, null, ifNull: "is null",
                            showSeparator: false));
                    }
                }
            }

            return children;
        }
    }
}