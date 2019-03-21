using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Object = UnityEngine.Object;
using Path = System.IO.Path;

namespace Unity.UIWidgets.painting {
    public class AssetImage : AssetBundleImageProvider, IEquatable<AssetImage> {
        public AssetImage(string assetName,
            AssetBundle bundle = null) {
            D.assert(assetName != null);
            this.assetName = assetName;
            this.bundle = bundle;
        }

        public readonly string assetName;
        public readonly AssetBundle bundle;

        protected override IPromise<AssetBundleImageKey> obtainKey(ImageConfiguration configuration) {
            AssetImageConfiguration assetConfig = new AssetImageConfiguration(configuration, this.assetName);
            AssetBundleImageKey key;
            var cache = AssetBundleCache.instance.get(configuration.bundle);
            if (cache.TryGetValue(assetConfig, out key)) {
                return Promise<AssetBundleImageKey>.Resolved(key);
            }

            AssetBundle chosenBundle = this.bundle ? this.bundle : configuration.bundle;
            var devicePixelRatio = configuration.devicePixelRatio ?? Window.instance.devicePixelRatio;
            key = this._loadAsset(chosenBundle, devicePixelRatio);
            cache[assetConfig] = key;
            
            return Promise<AssetBundleImageKey>.Resolved(key);
        }

        AssetBundleImageKey _loadAsset(AssetBundle bundle, float devicePixelRatio) {
            var extension = Path.GetExtension(this.assetName);
            var name = Path.GetFileNameWithoutExtension(this.assetName);

            var upper = Mathf.Min(3, devicePixelRatio.ceil());
            for (var scale = upper; scale >= 1; scale--) {
                var assetName = name + "@" + scale + extension;

                Object asset;
                if (bundle == null) {
                    asset = Resources.Load(assetName);
                } else {
                    asset = bundle.LoadAsset(assetName);
                }

                if (asset != null) {
                    if (bundle == null) {
                        Resources.UnloadAsset(asset);
                    } else {
                        bundle.Unload(asset);
                    }

                    return new AssetBundleImageKey(
                        bundle,
                        assetName,
                        scale: scale
                    );
                }
            }

            return new AssetBundleImageKey(
                bundle,
                this.assetName,
                scale: 1.0f
            );
        }

        public bool Equals(AssetImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(this.assetName, other.assetName) && Equals(this.bundle, other.bundle);
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

            return this.Equals((AssetImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.assetName != null ? this.assetName.GetHashCode() : 0) * 397) ^
                       (this.bundle != null ? this.bundle.GetHashCode() : 0);
            }
        }

        public static bool operator ==(AssetImage left, AssetImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(AssetImage left, AssetImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}(bundle: {this.bundle}, name: \"{this.assetName}\")";
        }
    }

    public class AssetImageConfiguration : IEquatable<AssetImageConfiguration> {
        public readonly ImageConfiguration configuration;
        public readonly string assetName;

        public AssetImageConfiguration(ImageConfiguration configuration, string assetName) {
            this.configuration = configuration;
            this.assetName = assetName;
        }

        public bool Equals(AssetImageConfiguration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.configuration, other.configuration) && string.Equals(this.assetName, other.assetName);
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

            return this.Equals((AssetImageConfiguration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.configuration != null ? this.configuration.GetHashCode() : 0) * 397) ^
                       (this.assetName != null ? this.assetName.GetHashCode() : 0);
            }
        }

        public static bool operator ==(AssetImageConfiguration left, AssetImageConfiguration right) {
            return Equals(left, right);
        }

        public static bool operator !=(AssetImageConfiguration left, AssetImageConfiguration right) {
            return !Equals(left, right);
        }
    }

    public class AssetBundleCache {
        static readonly AssetBundleCache _instance = new AssetBundleCache();

        public static AssetBundleCache instance {
            get { return _instance; }
        }

        readonly Dictionary<int, Dictionary<AssetImageConfiguration, AssetBundleImageKey>> _bundleCaches =
            new Dictionary<int, Dictionary<AssetImageConfiguration, AssetBundleImageKey>>();

        public Dictionary<AssetImageConfiguration, AssetBundleImageKey> get(AssetBundle bundle) {
            Dictionary<AssetImageConfiguration, AssetBundleImageKey> result;
            int id = bundle == null ? 0 : bundle.GetInstanceID();
            if (this._bundleCaches.TryGetValue(id, out result)) {
                return result;
            }

            result = new Dictionary<AssetImageConfiguration, AssetBundleImageKey>();
            this._bundleCaches[id] = result;
            return result;
        }
    }
}