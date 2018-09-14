using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.rendering;
using UIWidgets.ui;
using UnityEngine.Assertions;

namespace UIWidgets.widgets {
    public class RawImage : LeafRenderObjectWidget {
        public RawImage(Key key, ui.Image image, double width, double height, double scale, Color color,
            BlendMode colorBlendMode, BoxFit fit, Rect centerSlice, Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat) : base(key) {
            this.image = image;
            this.width = width;
            this.height = height;
            this.scale = scale;
            this.color = color;
            this.blendMode = colorBlendMode;
            this.centerSlice = centerSlice;
            this.fit = fit;
            this.alignment = alignment == null ? Alignment.center : alignment;
            this.repeat = repeat;
        }

        public override Element createElement() {
            throw new NotImplementedException();
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderImage(
                this.image,
                this.width,
                this.height,
                this.color,
                this.blendMode,
                this.fit,
                this.repeat,
                this.centerSlice,
                this.alignment
            );
        }

        public ui.Image image;
        public double width;
        public double height;
        public double scale;
        public Color color;
        public BlendMode blendMode;
        public BoxFit fit;
        public Alignment alignment;
        public ImageRepeat repeat;
        public Rect centerSlice;
    }

}