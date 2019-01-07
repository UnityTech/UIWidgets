using System;
using System.Collections;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.async;
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

        readonly Dictionary<ImageConfiguration, AssetBundleImageKey> _cache =
            new Dictionary<ImageConfiguration, AssetBundleImageKey>();

        protected override
            IPromise<AssetBundleImageKey> obtainKey(ImageConfiguration configuration) {

            AssetBundleImageKey key;
            if (this._cache.TryGetValue(configuration, out key)) {
                return Promise<AssetBundleImageKey>.Resolved(key);
            }
            
            AssetBundle chosenBundle = this.bundle ? this.bundle : configuration.bundle;
            var devicePixelRatio = configuration.devicePixelRatio ?? Window.instance.devicePixelRatio;
            var coroutine = Window.instance.startCoroutine(this._loadAssetAsync(chosenBundle, devicePixelRatio));
            return coroutine.promise.Then(result => {
                D.assert(result != null);

                key = (AssetBundleImageKey) result;
                this._cache[configuration] = key;
                return key;
            });
        }

        IEnumerator _loadAssetAsync(AssetBundle bundle, double devicePixelRatio) {
            var extension = Path.GetExtension(this.assetName);
            var name = Path.GetFileNameWithoutExtension(this.assetName);

            var upper = devicePixelRatio.ceil();
            for (var scale = upper; scale >= 1; scale--) {
                var assetName = name + "@" + scale + extension;

                Object asset;
                if (bundle == null) {
                    ResourceRequest request = Resources.LoadAsync(assetName);
                    yield return request;
                    asset = request.asset;
                } else {
                    AssetBundleRequest request = bundle.LoadAssetAsync(assetName);
                    yield return request;
                    asset = request.asset;
                }

                if (asset != null) {                    
                    if (bundle == null) {
                        Resources.UnloadAsset(asset);
                    } else {
                        bundle.Unload(asset);
                    }
                    
                    yield return new AssetBundleImageKey(
                        bundle,
                        assetName,
                        scale: scale
                    );
                    yield break;
                }
            }
            
            yield return new AssetBundleImageKey(
                bundle,
                this.assetName,
                scale: 1.0
            );
        }

        public bool Equals(AssetImage other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(this.assetName, other.assetName) && Equals(this.bundle, other.bundle);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((AssetImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.assetName != null ? this.assetName.GetHashCode() : 0) * 397) ^ (this.bundle != null ? this.bundle.GetHashCode() : 0);
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
}