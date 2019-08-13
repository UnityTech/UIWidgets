using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.flow {
    static class InstrumentationUtils {
        public const int kMaxSamples = 120;

        public static float now() {
#if UNITY_EDITOR
            return (float) EditorApplication.timeSinceStartup;
#else
            return Time.realtimeSinceStartup;
#endif
        }
    }

    public class Stopwatch {
        float _start;
        readonly float[] _laps;
        int _currentSample;

        public Stopwatch() {
            this._start = InstrumentationUtils.now();
            this._currentSample = 0;
            float delta = 0f;

            this._laps = new float[InstrumentationUtils.kMaxSamples];
            for (int i = 0; i < this._laps.Length; i++) {
                this._laps[i] = delta;
            }
        }

        public void start() {
            this._start = InstrumentationUtils.now();
            this._currentSample = (this._currentSample + 1) % InstrumentationUtils.kMaxSamples;
        }

        public void stop() {
            this._laps[this._currentSample] = InstrumentationUtils.now() - this._start;
        }

        public void setLapTime(float delta) {
            this._currentSample = (this._currentSample + 1) % InstrumentationUtils.kMaxSamples;
            this._laps[this._currentSample] = delta;
        }

        public float lastLap() {
            return this._laps[(this._currentSample - 1) % InstrumentationUtils.kMaxSamples];
        }

        public float maxDelta() {
            float maxDelta = 0f;
            for (int i = 0; i < this._laps.Length; i++) {
                if (maxDelta < this._laps[i]) {
                    maxDelta = this._laps[i];
                }
            }

            return maxDelta;
        }

        public void visualize(Canvas canvas, Rect rect) {
            Paint paint = new Paint {color = Colors.blue};
            Paint paint2 = new Paint {color = Colors.red};
            Paint paint3 = new Paint {color = Colors.green};
            Paint paint4 = new Paint {color = Colors.white70};

            float[] costFrames = this._laps;
            int curFrame = (this._currentSample - 1) % InstrumentationUtils.kMaxSamples;

            float barWidth = Mathf.Max(1, rect.width / costFrames.Length);
            float perHeight = rect.height / 32.0f;

            canvas.drawRect(rect, paint4);
            canvas.drawRect(Rect.fromLTWH(rect.left, rect.top + perHeight * 16.0f, rect.width, 1), paint3);

            float cur_x = rect.left;
            Path barPath = new Path();

            for (var i = 0; i < costFrames.Length; i++) {
                if (costFrames[i] != 0) {
                    float curHeight = Mathf.Min(perHeight * costFrames[i] * 1000, rect.height);
                    Rect barRect = Rect.fromLTWH(cur_x, rect.top + rect.height - curHeight, barWidth, curHeight);
                    barPath.addRect(barRect);
                }

                cur_x += barWidth;
            }

            canvas.drawPath(barPath, paint);
            if (curFrame >= 0 && curFrame < costFrames.Length) {
                if (costFrames[curFrame] != 0) {
                    float curHeight = Mathf.Min(perHeight * costFrames[curFrame] * 1000, rect.height);
                    Rect barRect = Rect.fromLTWH(rect.left + barWidth * curFrame, rect.top + rect.height - curHeight,
                        barWidth, curHeight);
                    canvas.drawRect(barRect, paint2);
                }

                var pb = new ParagraphBuilder(new ParagraphStyle { });
                pb.addText("Current Frame Cost: " + costFrames[curFrame] * 1000 + "ms" +
                           " ; Max(in last 120 frames): " + this.maxDelta() * 1000 + "ms");
                var paragraph = pb.build();
                paragraph.layout(new ParagraphConstraints(width: 800));

                canvas.drawParagraph(paragraph, new Offset(rect.left, rect.top + rect.height - 12));
                Paragraph.release(ref paragraph);
            }
        }
    }
}