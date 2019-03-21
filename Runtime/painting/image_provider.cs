using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RSG;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.UIWidgets.painting {
    public class ImageConfiguration : IEquatable<ImageConfiguration> {
        public ImageConfiguration(
            AssetBundle bundle = null,
            float? devicePixelRatio = null,
            Locale locale = null,
            Size size = null,
            RuntimePlatform? platform = null
        ) {
            this.bundle = bundle;
            this.devicePixelRatio = devicePixelRatio;
            this.locale = locale;
            this.size = size;
            this.platform = platform;
        }

        public ImageConfiguration copyWith(
            AssetBundle bundle = null,
            float? devicePixelRatio = null,
            Locale locale = null,
            Size size = null,
            RuntimePlatform? platform = null
        ) {
            return new ImageConfiguration(
                bundle: bundle ? bundle : this.bundle,
                devicePixelRatio: devicePixelRatio ?? this.devicePixelRatio,
                locale: locale ?? this.locale,
                size: size ?? this.size,
                platform: platform ?? this.platform
            );
        }

        public readonly AssetBundle bundle;

        public readonly float? devicePixelRatio;

        public readonly Locale locale;

        public readonly Size size;

        public readonly RuntimePlatform? platform;

        public static readonly ImageConfiguration empty = new ImageConfiguration();

        public bool Equals(ImageConfiguration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.bundle, other.bundle) && this.devicePixelRatio.Equals(other.devicePixelRatio) &&
                   Equals(this.locale, other.locale) && Equals(this.size, other.size) &&
                   this.platform == other.platform;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((ImageConfiguration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.bundle != null ? this.bundle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.devicePixelRatio.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.locale != null ? this.locale.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.size != null ? this.size.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.platform.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ImageConfiguration left, ImageConfiguration right) {
            return Equals(left, right);
        }

        public static bool operator !=(ImageConfiguration left, ImageConfiguration right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            var result = new StringBuilder();
            result.Append("ImageConfiguration(");
            bool hasArguments = false;
            if (this.bundle != null) {
                if (hasArguments) {
                    result.Append(", ");
                }

                result.Append($"bundle: {this.bundle}");
                hasArguments = true;
            }

            if (this.devicePixelRatio != null) {
                if (hasArguments) {
                    result.Append(", ");
                }

                result.Append($"devicePixelRatio: {this.devicePixelRatio:F1}");
                hasArguments = true;
            }

            if (this.locale != null) {
                if (hasArguments) {
                    result.Append(", ");
                }

                result.Append($"locale: {this.locale}");
                hasArguments = true;
            }

            if (this.size != null) {
                if (hasArguments) {
                    result.Append(", ");
                }

                result.Append($"size: {this.size}");
                hasArguments = true;
            }

            if (this.platform != null) {
                if (hasArguments) {
                    result.Append(", ");
                }

                result.Append($"platform: {this.platform}");
                hasArguments = true;
            }

            result.Append(")");
            return result.ToString();
        }
    }

    public abstract class ImageProvider {
        public abstract ImageStream resolve(ImageConfiguration configuration);
    }

    public abstract class ImageProvider<T> : ImageProvider {
        public override ImageStream resolve(ImageConfiguration configuration) {
            D.assert(configuration != null);

            ImageStream stream = new ImageStream();
            T obtainedKey = default;

            this.obtainKey(configuration).Then(key => {
                obtainedKey = key;
                stream.setCompleter(PaintingBinding.instance.imageCache.putIfAbsent(key, () => this.load(key)));
            }).Catch(ex => {
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                    exception: ex,
                    library: "services library",
                    context: "while resolving an image",
                    silent: true,
                    informationCollector: information => {
                        information.AppendLine($"Image provider: {this}");
                        information.AppendLine($"Image configuration: {configuration}");
                        if (obtainedKey != null) {
                            information.AppendLine($"Image key: {obtainedKey}");
                        }
                    }
                ));
            });

            return stream;
        }

        public IPromise<bool> evict(ImageCache cache = null, ImageConfiguration configuration = null) {
            configuration = configuration ?? ImageConfiguration.empty;
            cache = cache ?? PaintingBinding.instance.imageCache;

            return this.obtainKey(configuration).Then(key => cache.evict(key));
        }

        protected abstract ImageStreamCompleter load(T key);

        protected abstract IPromise<T> obtainKey(ImageConfiguration configuration);
    }

    public class AssetBundleImageKey : IEquatable<AssetBundleImageKey> {
        public AssetBundleImageKey(
            AssetBundle bundle,
            string name,
            float scale
        ) {
            D.assert(name != null);
            D.assert(scale >= 0.0);

            this.bundle = bundle;
            this.name = name;
            this.scale = scale;
        }

        public readonly AssetBundle bundle;

        public readonly string name;

        public readonly float scale;

        public bool Equals(AssetBundleImageKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.bundle, other.bundle) && string.Equals(this.name, other.name) &&
                   this.scale.Equals(other.scale);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((AssetBundleImageKey) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.bundle != null ? this.bundle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.name != null ? this.name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.scale.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(AssetBundleImageKey left, AssetBundleImageKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(AssetBundleImageKey left, AssetBundleImageKey right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}(bundle: {this.bundle}, name: \"{this.name}\", scale: {this.scale})";
        }
    }

    public abstract class AssetBundleImageProvider : ImageProvider<AssetBundleImageKey> {
        protected AssetBundleImageProvider() {
        }

        protected override ImageStreamCompleter load(AssetBundleImageKey key) {
            return new MultiFrameImageStreamCompleter(
                codec: this._loadAsync(key),
                scale: key.scale,
                informationCollector: information => {
                    information.AppendLine($"Image provider: {this}");
                    information.Append($"Image key: {key}");
                }
            );
        }

        IPromise<Codec> _loadAsync(AssetBundleImageKey key) {
            var coroutine = Window.instance.startCoroutine(this._loadAssetAsync(key));
            return coroutine.promise.Then(result => {
                if (result == null) {
                    if (key.bundle == null) {
                        throw new Exception($"Unable to find asset \"{key.name}\" from Resources folder");
                    }

                    throw new Exception($"Unable to find asset \"{key.name}\" from asset bundle \"{key.bundle}\"");
                }

                if (result is Texture2D texture) {
                    return CodecUtils.getCodec(new Image(texture, isAsset: true, bundle: key.bundle));
                }
                else if (result is TextAsset text) {
                    var bytes = text.bytes;
                    if (key.bundle == null) {
                        Resources.UnloadAsset(text);
                    }
                    else {
                        key.bundle.Unload(text);
                    }

                    return CodecUtils.getCodec(bytes);
                }
                else {
                    throw new Exception($"Unknown type for asset \"{key.name}\": \"{result.GetType()}\"");
                }
            });
        }

        IEnumerator _loadAssetAsync(AssetBundleImageKey key) {
            if (key.bundle == null) {
                ResourceRequest request = Resources.LoadAsync(key.name);
                if (request.asset) {
                    yield return request.asset;
                } else {
                    yield return request;
                    yield return request.asset;
                }
            }
            else {
                AssetBundleRequest request = key.bundle.LoadAssetAsync(key.name);
                if (request.asset) {
                    yield return request.asset;
                } else {
                    yield return request.asset;
                }
            }
        }
    }

    public class NetworkImage : ImageProvider<NetworkImage>, IEquatable<NetworkImage> {
        public NetworkImage(string url,
            float scale = 1.0f,
            IDictionary<string, string> headers = null) {
            D.assert(url != null);
            this.url = url;
            this.scale = scale;
            this.headers = headers;
        }

        public readonly string url;

        public readonly float scale;

        public readonly IDictionary<string, string> headers;

        protected override IPromise<NetworkImage> obtainKey(ImageConfiguration configuration) {
            return Promise<NetworkImage>.Resolved(this);
        }

        protected override ImageStreamCompleter load(NetworkImage key) {
            return new MultiFrameImageStreamCompleter(
                codec: this._loadAsync(key),
                scale: key.scale,
                informationCollector: information => {
                    information.AppendLine($"Image provider: {this}");
                    information.Append($"Image key: {key}");
                }
            );
        }

        IPromise<Codec> _loadAsync(NetworkImage key) {
            var coroutine = Window.instance.startCoroutine(this._loadBytes(key));
            return coroutine.promise.Then(obj => {
                if (obj is byte[] bytes) {
                    return CodecUtils.getCodec(bytes);
                }

                return CodecUtils.getCodec(new Image((Texture2D) obj));
            });
        }

        IEnumerator _loadBytes(NetworkImage key) {
            D.assert(key == this);
            var uri = new Uri(key.url);

            if (uri.LocalPath.EndsWith(".gif")) {
                using (var www = UnityWebRequest.Get(uri)) {
                    if (this.headers != null) {
                        foreach (var header in this.headers) {
                            www.SetRequestHeader(header.Key, header.Value);
                        }
                    }

                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError) {
                        throw new Exception($"Failed to load from url \"{uri}\": {www.error}");
                    }

                    var data = www.downloadHandler.data;
                    yield return data;
                }

                yield break;
            }

            using (var www = UnityWebRequestTexture.GetTexture(uri)) {
                if (this.headers != null) {
                    foreach (var header in this.headers) {
                        www.SetRequestHeader(header.Key, header.Value);
                    }
                }

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) {
                    throw new Exception($"Failed to load from url \"{uri}\": {www.error}");
                }

                var data = ((DownloadHandlerTexture) www.downloadHandler).texture;
                yield return data;
            }
        }

        public bool Equals(NetworkImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(this.url, other.url) && this.scale.Equals(other.scale);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((NetworkImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.url != null ? this.url.GetHashCode() : 0) * 397) ^ this.scale.GetHashCode();
            }
        }

        public static bool operator ==(NetworkImage left, NetworkImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(NetworkImage left, NetworkImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"runtimeType(\"{this.url}\", scale: {this.scale})";
        }
    }

    public class FileImage : ImageProvider<FileImage>, IEquatable<FileImage> {
        public FileImage(string file, float scale = 1.0f) {
            D.assert(file != null);
            this.file = file;
            this.scale = scale;
        }

        public readonly string file;

        public readonly float scale;

        protected override IPromise<FileImage> obtainKey(ImageConfiguration configuration) {
            return Promise<FileImage>.Resolved(this);
        }

        protected override ImageStreamCompleter load(FileImage key) {
            return new MultiFrameImageStreamCompleter(this._loadAsync(key),
                scale: key.scale,
                informationCollector: information => { information.AppendLine($"Path: {this.file}"); });
        }

        IPromise<Codec> _loadAsync(FileImage key) {
            var coroutine = Window.instance.startCoroutine(this._loadBytes(key));
            return coroutine.promise.Then(obj => {
                if (obj is byte[] bytes) {
                    return CodecUtils.getCodec(bytes);
                }

                return CodecUtils.getCodec(new Image((Texture2D) obj));
            });
        }

        IEnumerator _loadBytes(FileImage key) {
            D.assert(key == this);
            var uri = "file://" + key.file;

            if (uri.EndsWith(".gif")) {
                using (var www = UnityWebRequest.Get(uri)) {
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError) {
                        throw new Exception($"Failed to get file \"{uri}\": {www.error}");
                    }

                    var data = www.downloadHandler.data;
                    yield return data;
                }

                yield break;
            }

            using (var www = UnityWebRequestTexture.GetTexture(uri)) {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) {
                    throw new Exception($"Failed to get file \"{uri}\": {www.error}");
                }

                var data = ((DownloadHandlerTexture) www.downloadHandler).texture;
                yield return data;
            }
        }

        public bool Equals(FileImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(this.file, other.file) && this.scale.Equals(other.scale);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((FileImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.file != null ? this.file.GetHashCode() : 0) * 397) ^ this.scale.GetHashCode();
            }
        }

        public static bool operator ==(FileImage left, FileImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(FileImage left, FileImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}(\"{this.file}\", scale: {this.scale})";
        }
    }

    public class MemoryImage : ImageProvider<MemoryImage>, IEquatable<MemoryImage> {
        public MemoryImage(byte[] bytes, float scale = 1.0f) {
            D.assert(bytes != null);
            this.bytes = bytes;
            this.scale = scale;
        }

        public readonly byte[] bytes;

        public readonly float scale;

        protected override IPromise<MemoryImage> obtainKey(ImageConfiguration configuration) {
            return Promise<MemoryImage>.Resolved(this);
        }

        protected override ImageStreamCompleter load(MemoryImage key) {
            return new MultiFrameImageStreamCompleter(
                this._loadAsync(key),
                scale: key.scale);
        }

        IPromise<Codec> _loadAsync(MemoryImage key) {
            D.assert(key == this);

            return CodecUtils.getCodec(this.bytes);
        }

        public bool Equals(MemoryImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.bytes, other.bytes) && this.scale.Equals(other.scale);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((MemoryImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.bytes != null ? this.bytes.GetHashCode() : 0) * 397) ^ this.scale.GetHashCode();
            }
        }

        public static bool operator ==(MemoryImage left, MemoryImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(MemoryImage left, MemoryImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({Diagnostics.describeIdentity(this.bytes)}), scale: {this.scale}";
        }
    }

    public class ExactAssetImage : AssetBundleImageProvider, IEquatable<ExactAssetImage> {
        public ExactAssetImage(
            string assetName,
            float scale = 1.0f,
            AssetBundle bundle = null
        ) {
            D.assert(assetName != null);
            this.assetName = assetName;
            this.scale = scale;
            this.bundle = bundle;
        }

        public readonly string assetName;

        public readonly float scale;

        public readonly AssetBundle bundle;

        protected override IPromise<AssetBundleImageKey> obtainKey(ImageConfiguration configuration) {
            return Promise<AssetBundleImageKey>.Resolved(new AssetBundleImageKey(
                bundle: this.bundle ? this.bundle : configuration.bundle,
                name: this.assetName,
                scale: this.scale
            ));
        }

        public bool Equals(ExactAssetImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(this.assetName, other.assetName) && this.scale.Equals(other.scale) &&
                   Equals(this.bundle, other.bundle);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((ExactAssetImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.assetName != null ? this.assetName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.scale.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.bundle != null ? this.bundle.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ExactAssetImage left, ExactAssetImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(ExactAssetImage left, ExactAssetImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}(name: \"{this.assetName}\", scale: {this.scale}, bundle: {this.bundle})";
        }
    }
}