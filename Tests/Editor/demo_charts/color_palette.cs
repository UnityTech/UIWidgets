using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace UIWidgets.Tests.demo_charts {
    public class ColorPalette {
        public static readonly ColorPalette primary = new ColorPalette(new List<Color> {
            Colors.blue[400],
            Colors.red[400],
            Colors.green[400],
            Colors.yellow[400],
            Colors.purple[400],
            Colors.orange[400],
            Colors.teal[400]
        });

        public ColorPalette(List<Color> colors) {
            D.assert(colors.isNotEmpty);
            this._colors = colors;
        }

        readonly List<Color> _colors;

        public Color this[int index] {
            get { return this._colors[index % this.length]; }
        }

        public int length {
            get { return this._colors.Count; }
        }

        public Color random() {
            return this[Random.Range(0, this.length - 1)];
        }
    }
}
