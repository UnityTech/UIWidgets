using System;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.painting {
    public enum BoxFit {
        fill,
        contain,
        cover,
        fitWidth,
        fitHeight,
        none,
        scaleDown,
    }

    public class FittedSizes {
        public FittedSizes(Size source, Size destination) {
            this.source = source;
            this.destination = destination;
        }

        public readonly Size source;
        public readonly Size destination;

        public static FittedSizes applyBoxFit(BoxFit fit, Size inputSize, Size outputSize) {
            if (inputSize.height <= 0.0
                || inputSize.width <= 0.0
                || outputSize.height <= 0.0
                || outputSize.width <= 0.0) {
                return new FittedSizes(Size.zero, Size.zero);
            }

            Size sourceSize = null;
            Size destinationSize = null;
            switch (fit) {
                case BoxFit.fill:
                    sourceSize = inputSize;
                    destinationSize = outputSize;
                    break;
                case BoxFit.contain:
                    sourceSize = inputSize;
                    if (outputSize.width / outputSize.height > sourceSize.width / sourceSize.height) {
                        destinationSize = new Size(sourceSize.width * outputSize.height / sourceSize.height,
                            outputSize.height);
                    }
                    else {
                        destinationSize = new Size(outputSize.width,
                            sourceSize.height * outputSize.width / sourceSize.width);
                    }

                    break;
                case BoxFit.cover:
                    if (outputSize.width / outputSize.height > inputSize.width / inputSize.height) {
                        sourceSize = new Size(inputSize.width, inputSize.width * outputSize.height / outputSize.width);
                    }
                    else {
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
                    sourceSize = new Size(Mathf.Min(inputSize.width, outputSize.width),
                        Mathf.Min(inputSize.height, outputSize.height));
                    destinationSize = sourceSize;
                    break;
                case BoxFit.scaleDown:
                    sourceSize = inputSize;
                    destinationSize = inputSize;
                    float aspectRatio = inputSize.width / inputSize.height;
                    if (destinationSize.height > outputSize.height) {
                        destinationSize = new Size(outputSize.height * aspectRatio,
                            outputSize.height);
                    }

                    if (destinationSize.width > outputSize.width) {
                        destinationSize = new Size(outputSize.width, outputSize.width / aspectRatio);
                    }

                    break;
            }

            return new FittedSizes(sourceSize, destinationSize);
        }
    }
}