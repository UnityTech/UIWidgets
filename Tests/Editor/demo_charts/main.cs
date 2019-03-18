using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgets.Tests.demo_charts {
    /***
     * from https://github.com/mravn/charts
     */
    public class ChartPage : StatefulWidget {
        public override State createState() {
            return new ChartPageState();
        }
    }

    public class ChartPageState : TickerProviderStateMixin<ChartPage> {
        public static readonly Size size = new Size(200.0f, 100.0f);

        AnimationController _animation;
        BarChartTween _tween;

        public override
            void initState() {
            base.initState();
            this._animation = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, 300),
                vsync: this
            );
            this._tween = new BarChartTween(
                BarChart.empty(),
                BarChart.random(size)
            );
            this._animation.forward();
        }

        public override void dispose() {
            this._animation.dispose();
            base.dispose();
        }

        void changeData() {
            this.setState(() => {
                this._tween = new BarChartTween(
                    this._tween.evaluate(this._animation),
                    BarChart.random(size)
                );
                this._animation.forward(from: 0.0f);
            });
        }

        public override Widget build(BuildContext context) {
            return new Scaffold(
                body: new Center(
                    child: new CustomPaint(
                        size: size,
                        painter: new BarChartPainter(this._tween.animate(this._animation))
                    )
                ),
                floatingActionButton: new FloatingActionButton(
                    child: new Icon(Unity.UIWidgets.material.Icons.refresh),
                    onPressed: this.changeData
                )
            );
        }
    }
}
