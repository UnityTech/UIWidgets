using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace UIWidgets.lib.cache_manager {
    public class CacheMeta {
        private static readonly string _directory = Application.persistentDataPath;
        public string relativePath = null;
        public string eTag = null;
        public double touched;
        public double validTill;
        public string url;
        public string key;

        public CacheMeta(string url) {
            this.url = url;
            touch();
        }
        
        private CacheMeta(Builder builder) {
            key = builder.key;
            relativePath = builder.relativePath;
            eTag = builder.eTag;
            touched = builder.touched;
            validTill = builder.validTill;
            url = builder.url;
        }

        public string getFilePath() {
            if (this.relativePath == null) {
                return null;
            }
            return _directory + this.relativePath;
        }

        public void setRelativePath(string path) {
            this.relativePath = path;
        }

        public void touch() {
            this.touched = millisecondsSinceEpoch(DateTime.Now);
        }

        public void setDataFromHeaders(Dictionary<string, string> headers) {
            var ageDuration = new TimeSpan(7, 0, 0, 0); // 7 days

            if (headers.ContainsKey("cache-control")) {
                var cacheControl = headers["cache-control"];
                string[] stringSeparators = {", "};
                var controlSettings = cacheControl.Split(stringSeparators, StringSplitOptions.None);
                foreach (var controlSetting in controlSettings) {
                    if (controlSetting.StartsWith("max-age=")) {
                        int validSeconds = 0;
                        if (int.TryParse(controlSetting.Split('=')[1], out validSeconds)) {
                            if (validSeconds > 0) {
                                ageDuration = new TimeSpan(0, 0, validSeconds);
                            }
                        }
                    }
                }
            }

            validTill = millisecondsSinceEpoch(DateTime.Now + ageDuration);

            if (headers.ContainsKey("etag")) {
                eTag = headers["etag"];
            }

            var fileExtension = "";
            if (headers.ContainsKey("content-type")) {
                var type = headers["content-type"].Split('/');
                if (type.Length == 2) {
                    fileExtension = string.Format(".{0}", type[1]);
                }
            }

            var oldPath = getFilePath();
            if (oldPath != null && !oldPath.EndsWith(fileExtension)) {
                removeOldFile(oldPath);
                relativePath = null;
            }

            if (relativePath == null) {
                var fileName = string.Format("/cache_{0}{1}", Guid.NewGuid(), fileExtension);
                relativePath = fileName;
            }
        }

        private static void removeOldFile(string filePath) {
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
        }

        public static double millisecondsSinceEpoch(DateTime time) {
            return (time - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static DateTime fromMillisecondsSinceEpoch(double ms) {
            return new DateTime(1970, 1, 1).AddMilliseconds(ms);
        }

        public sealed class Builder {
            internal string key { get; private set; }
            
            internal string relativePath { get; private set; }

            internal string eTag { get; private set; }

            internal double touched { get; private set; }

            internal double validTill { get; private set; }

            internal string url { get; private set; }

            public Builder(string key) {
                if (string.IsNullOrEmpty(key)) {
                    throw new ArgumentException("key can't be empty", "key");
                }

                this.key = key;
            }

            public Builder RelativePath(string relativePath) {
                this.relativePath = relativePath;
                return this;
            }

            public Builder ETag(string eTag) {
                this.eTag = eTag;
                return this;
            }

            public Builder Touched(double touched) {
                this.touched = touched;
                return this;
            }

            public Builder ValidTill(double validTill) {
                this.validTill = validTill;
                return this;
            }

            public Builder Url(string url) {
                this.url = url;
                return this;
            }
            
            public CacheMeta Build()
            {
                return new CacheMeta(this);
            }
        }
    }
}