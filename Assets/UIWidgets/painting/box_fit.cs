using UIWidgets.ui;
using System;

namespace UIWidgets.painting
{
    public enum BoxFit
    {
        /// Fill the target box by distorting the source's aspect ratio.
        ///
        /// ![](https://flutter.github.io/assets-for-api-docs/assets/painting/box_fit_fill.png)
        fill,

        /// As large as possible while still containing the source entirely within the
        /// target box.
        ///
        /// ![](https://flutter.github.io/assets-for-api-docs/assets/painting/box_fit_contain.png)
        contain,

        /// As small as possible while still covering the entire target box.
        ///
        /// ![](https://flutter.github.io/assets-for-api-docs/assets/painting/box_fit_cover.png)
        cover,

        /// Make sure the full width of the source is shown, regardless of
        /// whether this means the source overflows the target box vertically.
        ///
        /// ![](https://flutter.github.io/assets-for-api-docs/assets/painting/box_fit_fitWidth.png)
        fitWidth,

        /// Make sure the full height of the source is shown, regardless of
        /// whether this means the source overflows the target box horizontally.
        ///
        /// ![](https://flutter.github.io/assets-for-api-docs/assets/painting/box_fit_fitHeight.png)
        fitHeight,

        /// Align the source within the target box (by default, centering) and discard
        /// any portions of the source that lie outside the box.
        ///
        /// The source image is not resized.
        ///
        /// ![](https://flutter.github.io/assets-for-api-docs/assets/painting/box_fit_none.png)
        none,

        /// Align the source within the target box (by default, centering) and, if
        /// necessary, scale the source down to ensure that the source fits within the
        /// box.
        ///
        /// This is the same as `contain` if that would shrink the image, otherwise it
        /// is the same as `none`.
        ///
        /// ![](https://flutter.github.io/assets-for-api-docs/assets/painting/box_fit_scaleDown.png)
        scaleDown,
    }

    public class FittedSizes
    {
        public FittedSizes(Size source, Size destination)
        {
            //todo wrong
            this.source = source;
            this.destination = destination;
        }

        public Size source;
        public Size destination;

        public static FittedSizes applyBoxFit(BoxFit fit, Size inputSize, Size outputSize)
        {
            if (inputSize.height <= 0.0 || inputSize.width <= 0.0 || outputSize.height <= 0.0 ||
                outputSize.width <= 0.0)
                return new FittedSizes(Size.zero, Size.zero);
            Size sourceSize = null;
            Size destinationSize = null;
            switch (fit)
            {
                case BoxFit.fill:
                    sourceSize = inputSize;
                    destinationSize = outputSize;
                    break;
                case BoxFit.contain:
                    sourceSize = inputSize;
                    if (outputSize.width / outputSize.height > sourceSize.width / sourceSize.height)
                        destinationSize = new Size(sourceSize.width * outputSize.height / sourceSize.height,
                            outputSize.height);
                    else
                        destinationSize = new Size(outputSize.width,
                            sourceSize.height * outputSize.width / sourceSize.width);
                    break;
                case BoxFit.cover:
                    if (outputSize.width / outputSize.height > inputSize.width / inputSize.height)
                    {
                        sourceSize = new Size(inputSize.width, inputSize.width * outputSize.height / outputSize.width);
                    }
                    else
                    {
                        sourceSize = new Size(inputSize.height * outputSize.width / outputSize.height,
                            inputSize.height);
                    }

                    destinationSize = outputSize;
                    break;
                case BoxFit.fitWidth:
                    sourceSize = new Size(inputSize.width, inputSize.width * outputSize.height / outputSize.width);
                    destinationSize = new Size(outputSize.width,
                        sourceSize.height * outputSize.width / sourceSize.width);
                    break;
                case BoxFit.fitHeight:
                    sourceSize = new Size(inputSize.height * outputSize.width / outputSize.height, inputSize.height);
                    destinationSize = new Size(sourceSize.width * outputSize.height / sourceSize.height,
                        outputSize.height);
                    break;
                case BoxFit.none:
                    sourceSize = new Size(Math.Min(inputSize.width, outputSize.width),
                        Math.Min(inputSize.height, outputSize.height));
                    destinationSize = sourceSize;
                    break;
                case BoxFit.scaleDown:
                    sourceSize = inputSize;
                    destinationSize = inputSize;
                    double aspectRatio = inputSize.width / inputSize.height;
                    if (destinationSize.height > outputSize.height)
                        destinationSize = new Size(outputSize.height * aspectRatio, outputSize.height);
                    if (destinationSize.width > outputSize.width)
                        destinationSize = new Size(outputSize.width, outputSize.width / aspectRatio);
                    break;
            }
            return new FittedSizes(sourceSize, destinationSize);
        }
    }
}