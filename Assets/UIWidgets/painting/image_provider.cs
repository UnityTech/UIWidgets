using System.Collections.Generic;
using RSG;
using System.Net;
using System;
using System.IO;
using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.painting
{
    public abstract class ImageProvider<T>
    {
        public ImageStream resolve(ImageConfiguration configuration)
        {
            ImageStream stream = new ImageStream();
            T obtainedKey;
            obtainedKey = obtainKey(configuration);
            stream.setCompleter(PaintingBinding.instance.imageCache.putIfAbsent(obtainedKey, () => load(obtainedKey)));
            return stream;
        }

        public abstract ImageStreamCompleter load(T key);

        public abstract T obtainKey(ImageConfiguration configuration);
    }

    public class NetworkImage : ImageProvider<NetworkImage>
    {
        public NetworkImage(string url, Dictionary<string, string> headers, double scale = 1.0)
        {
            this.url = url;
            this.headers = headers;
            this.scale = scale;
        }

        /// The URL from which the image will be fetched.
        string url;

        /// The scale to place in the [ImageInfo] object of the image.
        double scale;

        /// The HTTP headers that will be used with [HttpClient.get] to fetch image from network.
        Dictionary<string, string> headers;

        public override NetworkImage obtainKey(ImageConfiguration configuration)
        {
//        return new SynchronousFuture<NetworkImage> (this);
            return this;
        }

        public override ImageStreamCompleter load(NetworkImage key)
        {
            return new OneFrameImageStreamCompleter(_loadAsync(key));
        }

        public static IPromise<ImageInfo> _loadAsync(NetworkImage key)
        {
            var promise = new Promise<ImageInfo>(); // Create promise.
            using (var client = new WebClient())
            {
                client.DownloadDataCompleted += // Monitor event for download completed.
                    (s, ev) =>
                    {
                        if (ev.Error != null)
                        {
                            promise.Reject(ev.Error); // Error during download, reject the promise.
                        }
                        else
                        {
                            var bytes = ev.Result;
                            var imageInfo = new ImageInfo(new ui.Image(
                                bytes
                            ));
                            promise.Resolve(imageInfo); // Downloaded completed successfully, resolve the promise.
                        }
                    };

                client.DownloadDataAsync(new Uri(key.url)); // Initiate async op.
            }

            return promise; // Return the promise so the caller can await resolution (or error).
        }

        public override string ToString()
        {
            return "NetworkImage with Url: " + this.url;
        }
        
        public bool Equals(NetworkImage other) {
            return this.url.Equals(other.url) && this.scale.Equals(other.scale);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            return obj is NetworkImage && this.Equals((NetworkImage) obj);
        }
        
        public override int GetHashCode() {
            unchecked {
                var hashCode = this.url.GetHashCode();
                hashCode = (hashCode * 397) ^ this.scale.GetHashCode();
                return hashCode;
            }
        }
    }

    public class ImageConfiguration
    {
        public ImageConfiguration(Size size = null)
        {
            this.size = size;
        }

        public static readonly ImageConfiguration empty = new ImageConfiguration();

        public ImageConfiguration copyWith(Size size = null)
        {
            return new ImageConfiguration(
                size: size ?? this.size
            );
        }

        public readonly Size size;
    }
}