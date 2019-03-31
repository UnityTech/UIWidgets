using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.animation;

namespace UIWidgets.Tests.demo_charts {
    public interface MergeTweenable<T> {
        T empty { get; }

        Tween<T> tweenTo(T other);

        bool less(T other);
    }

    public class MergeTween<T> : Tween<List<T>> where T : MergeTweenable<T> {
        public MergeTween(List<T> begin, List<T> end) : base(begin: begin, end: end) {
            int bMax = begin.Count;
            int eMax = end.Count;
            var b = 0;
            var e = 0;
            while (b + e < bMax + eMax) {
                if (b < bMax && (e == eMax || begin[b].less(end[e]))) {
                    this._tweens.Add(begin[b].tweenTo(begin[b].empty));
                    b++;
                } else if (e < eMax && (b == bMax || end[e].less(begin[b]))) {
                    this._tweens.Add(end[e].empty.tweenTo(end[e]));
                    e++;
                } else {
                    this._tweens.Add(begin[b].tweenTo(end[e]));
                    b++;
                    e++;
                }
            }
        }

        readonly List<Tween<T>> _tweens = new List<Tween<T>>();

        public override List<T> lerp(float t) {
            return Enumerable.Range(0, this._tweens.Count).Select(i => this._tweens[i].lerp(t)).ToList();
        }
    }
}
