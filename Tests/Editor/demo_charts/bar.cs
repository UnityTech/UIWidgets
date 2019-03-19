using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace UIWidgets.Tests.demo_charts {
    public class BarChart {
        public BarChart(List<Bar> bars) {
            this.bars = bars;
        }

        public static BarChart empty() {
            return new BarChart(new List<Bar>());
        }

        public static BarChart random(Size size) {
            var barWidthFraction = 0.75f;
            var ranks = selectRanks(ColorPalette.primary.length);
            var barCount = ranks.Count;
            var barDistance = size.width / (1 + barCount);
            var barWidth = barDistance * barWidthFraction;
            var startX = barDistance - barWidth / 2;
            var bars = Enumerable.Range(0, barCount).Select(i => new Bar(
                ranks[i],
                startX + i * barDistance,
                barWidth,
                Random.value * size.height,
                ColorPalette.primary[ranks[i]]
            )).ToList();
            return new BarChart(bars);
        }

        static List<int> selectRanks(int cap) {
            var ranks = new List<int>();
            var rank = 0;
            while (true) {
                if (Random.value < 0.2f) {
                    rank++;
                }
                if (cap <= rank) {
                    break;
                }
                ranks.Add(rank);
                rank++;
            }
            return ranks;
        }

        public readonly List<Bar> bars;
    }

    public class BarChartTween : Tween<BarChart> {
        readonly MergeTween<Bar> _barsTween;

        public BarChartTween(BarChart begin, BarChart end) : base(begin: begin, end: end) {
            this._barsTween = new MergeTween<Bar>(begin.bars, end.bars);
        }

        public override BarChart lerp(float t) {
            return new BarChart(this._barsTween.lerp(t));
        }
    }

    public class Bar : MergeTweenable<Bar> {
        public Bar(int rank, float x, float width, float height, Color color) {
            this.rank = rank;
            this.x = x;
            this.width = width;
            this.height = height;
            this.color = color;
        }

        public readonly int rank;
        public readonly float x;
        public readonly float width;
        public readonly float height;
        public readonly Color color;

        public Bar empty {
            get { return new Bar(this.rank, this.x, 0.0f, 0.0f, this.color); }
        }

        public bool less(Bar other) {
            return this.rank < other.rank;
        }

        public Tween<Bar> tweenTo(Bar other) {
            return new BarTween(this, other);
        }

        public static Bar lerp(Bar begin, Bar end, float t) {
            D.assert(begin.rank == end.rank);
            return new Bar(
                begin.rank,
                MathUtils.lerpFloat(begin.x, end.x, t),
                MathUtils.lerpFloat(begin.width, end.width, t),
                MathUtils.lerpFloat(begin.height, end.height, t),
                Color.lerp(begin.color, end.color, t)
            );
        }
    }

    public class BarTween : Tween<Bar> {
        public BarTween(Bar begin, Bar end) : base(begin: begin, end: end) {
            D.assert(begin.rank == end.rank);
        }

        public override Bar lerp(float t) {
            return Bar.lerp(this.begin, this.end, t);
        }
    }

    public class BarChartPainter : AbstractCustomPainter {
        public BarChartPainter(Animation<BarChart> animation)
            : base(repaint: animation) {
            this.animation = animation;
        }

        public readonly Animation<BarChart> animation;

        public override void paint(Canvas canvas, Size size) {
            var paint = new Paint();
            paint.style = PaintingStyle.fill;
            var chart = this.animation.value;
            foreach (var bar in chart.bars) {
                paint.color = bar.color;
                canvas.drawRect(
                    Rect.fromLTWH(
                        bar.x,
                        size.height - bar.height,
                        bar.width,
                        bar.height
                    ),
                    paint
                );
            }
        }

        public override bool shouldRepaint(CustomPainter old) {
            return false;
        }
    }
}
