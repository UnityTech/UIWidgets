using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using RSG;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Net;
using Mono.Data.Sqlite;
using UIWidgets.painting;

namespace UIWidgets.lib.cache_manager {
    public class CacheManager {
        private readonly string _keyCacheData = "lib_cached_image_data";
        private readonly string _keyCacheCleanDate = "lib_cached_image_data_last_clean";

        private string _dbUri;

        private const string DbFileName = @"ui_widgets_cache.db";

        private static TimeSpan inBetweenCleans = new TimeSpan(7, 0, 0, 0);
        private static TimeSpan maxAgeCacheObject = new TimeSpan(30, 0, 0, 0);

        private static int maxNrOfCacheObjects = 2; // configurable ?

        private static CacheManager _instance;

//      public DateTime lastCacheClean;
        private bool _isStoringData = false;
        private bool _shouldStoreDataAgain = false;

        public static CacheManager getInstance() {
            if (_instance == null) {
                _instance = new CacheManager();
                _instance._init();
            }

            return _instance;
        }

        private void _init() {
            _setupDatabase();
        }

        private void _setupDatabase() {
            var directoryPath = Application.persistentDataPath;
            var _dbFilePath = Path.Combine(directoryPath, DbFileName);
            _dbUri = "URI=file:" + _dbFilePath;

            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(_dbFilePath)) {
                SqliteConnection.CreateFile(_dbFilePath);
            }

            using (var connection = new SqliteConnection(_dbUri)) {
                connection.Open();
                const string createCacheTable = @"CREATE TABLE IF NOT EXISTS Cache (
                                                    Key TEXT NOT NULL PRIMARY KEY,
                                                    FilePath TEXT NOT NULL,
                                                    ETag TEXT,
                                                    Url TEXT NOT NULL,
                                                    Touched REAL,
                                                    ValidTill REAL
                                                )";

                using (var command = new SqliteCommand(createCacheTable, connection)) {
                    command.ExecuteNonQuery();
                }
            }
        }

        public IPromise<CacheMeta> getMeta(string url) {
            var key = generateHashKey(url);

            CacheMeta meta = null;

            using (var connection = new SqliteConnection(_dbUri)) {
                connection.Open();
                const string metaQuery = @"SELECT Key, FilePath, ETag, Url, Touched, ValidTill FROM Cache
                                     WHERE Key = @Key";

                using (var command = new SqliteCommand(metaQuery, connection)) {
                    command.Parameters.AddWithValue("@Key", key);

                    using (var reader = command.ExecuteReader()) {
                        if (reader.HasRows && reader.Read()) {
                            meta = new CacheMeta.Builder(reader.GetString(0))
                                .RelativePath(reader.GetString(1))
                                .ETag(reader.IsDBNull(2) ? string.Empty : reader.GetString(2))
                                .Url(reader.GetString(3))
                                .Touched(reader.GetDouble(4))
                                .ValidTill(reader.GetDouble(5))
                                .Build();
                        }
                    }
                }
            }

            var promise = new Promise<CacheMeta>();
            if (meta == null) {
                meta = new CacheMeta(url);
            }

            meta.touch();
            promise.Resolve(meta);
            return promise;
        }

        public IPromise<CacheMeta> downloadFileIfNeeded(CacheMeta meta) {
            var promise = new Promise<CacheMeta>(); // Create promise.
            var filepath = meta.getFilePath();
            var fileExpire = meta.validTill == 0.0 ||
                             CacheMeta.fromMillisecondsSinceEpoch(meta.validTill) < DateTime.Now;
            if (filepath == null ||
                fileExpire ||
                !File.Exists(filepath)) {
                // download from url 
                WebRequest webRequest = WebRequest.Create(new Uri(meta.url));
                if (fileExpire && meta.eTag != null) {
                    webRequest.Headers.Set("If-None-Match", meta.eTag);
                }

                webRequest.BeginGetResponse(result => {
                    const int BufferSize = 1024;

                    var bytes = new byte[BufferSize];
                    var response = webRequest.EndGetResponse(result);

                    var statusCode = (int) ((HttpWebResponse) response).StatusCode;
                    var respHeaders = response.Headers;
                    var headerDict = new Dictionary<string, string>();
                    for (int i = 0; i < respHeaders.Count; i++) {
                        string header = respHeaders.GetKey(i);
                        string value = respHeaders.Get(header);
                        headerDict[header] = respHeaders[value];
                    }

                    if (statusCode == 200) {
                        meta.setDataFromHeaders(headerDict);

                        var stream = response.GetResponseStream();
                        if (stream != null) {
                            var localStream = File.Create(meta.getFilePath());
                            int bytesRead;
                            while ((bytesRead = stream.Read(bytes, 0, BufferSize)) > 0) {
                                localStream.Write(bytes, 0, bytesRead);
                            }

                            stream.Close();
                            localStream.Close();
                            promise.Resolve(meta);
                        }
                    } else if (statusCode == 304) {
                        meta.setDataFromHeaders(headerDict);
                        promise.Resolve(meta);
                    }
                }, null);
            }
            else {
                promise.Resolve(meta);
            }

            return promise;
        }

        public IPromise<string> updateMeta(CacheMeta newMeta) {
            var key = generateHashKey(newMeta.url);

            const string checkMetaQuery = @"SELECT COUNT(*) FROM Cache WHERE
                                        Key = @Key";

            bool recordFound = false;

            using (var connection = new SqliteConnection(_dbUri)) {
                connection.Open();
                using (var command = new SqliteCommand(checkMetaQuery, connection)) {
                    command.Parameters.AddWithValue("@Key", key);

                    using (var reader = command.ExecuteReader()) {
                        if (reader.Read()) {
                            recordFound = reader.GetInt32(0) > 0;
                        }
                    }
                }

                if (recordFound) {
                    const string updateCacheQuery =
                        @"UPDATE Cache SET FilePath = @FilePath, ETag = @ETag,
                                           Url = @Url, Touched = @Touched, ValidTill = @ValidTill 
                                           WHERE Key = @Key";

                    using (var command = new SqliteCommand(updateCacheQuery, connection)) {
                        command.Parameters.AddWithValue("@FilePath", newMeta.relativePath);
                        command.Parameters.AddWithValue("@ETag", newMeta.eTag);
                        command.Parameters.AddWithValue("@Url", newMeta.url);
                        command.Parameters.AddWithValue("@Touched", newMeta.touched);
                        command.Parameters.AddWithValue("@ValidTill", newMeta.validTill);
                        command.Parameters.AddWithValue("@key", key);
                        command.ExecuteNonQuery();
                    }
                }
                else {
                    const string insertQuery =
                        @"INSERT INTO Cache (Key, FilePath, ETag, Url, Touched, ValidTill)
                                     VALUES (@Key, @FilePath, @ETag, @Url, @Touched, @ValidTill)";

                    using (var command = new SqliteCommand(insertQuery, connection)) {
                        command.Parameters.AddWithValue("@Key", key);
                        command.Parameters.AddWithValue("@FilePath", newMeta.relativePath);
                        command.Parameters.AddWithValue("@ETag", newMeta.eTag);
                        command.Parameters.AddWithValue("@Url", newMeta.url);
                        command.Parameters.AddWithValue("@Touched", newMeta.touched);
                        command.Parameters.AddWithValue("@ValidTill", newMeta.validTill);
                        command.ExecuteNonQuery();
                    }
                }
            }

            _removeOldObjectsFromCache(); 
            _shrinkLargeCache();

            var promise = new Promise<string>();
            promise.Resolve(newMeta.getFilePath());
            return promise;
        }

        public IPromise<ImageInfo> loadCacheFile(string path) {
            var promise = new Promise<ImageInfo>();
            var bytes = File.ReadAllBytes(path);
            var imageInfo = new ImageInfo(new ui.Image(
                bytes
            ));
            promise.Resolve(imageInfo);
            return promise;
        }

        private void _removeOldObjectsFromCache() {
            var oldestDataAllowed = DateTime.Now - maxAgeCacheObject;
            var metas = new List<CacheMeta>();

            const string query = @"SELECT Key, FilePath, ETag, Url, Touched, ValidTill
                                     FROM Cache
                                     WHERE Touched < @OldestTouchedAllowed";

            const string deleteQuery = @"DELETE FROM Cache WHERE Key in (@KeyList)";
            using (var connection = new SqliteConnection(_dbUri)) {
                connection.Open();
                using (var command = new SqliteCommand(query, connection)) {
                    command.Parameters.AddWithValue("@OldestTouchedAllowed",
                        CacheMeta.millisecondsSinceEpoch(oldestDataAllowed));
                    using (var reader = command.ExecuteReader()) {
                        while (reader.HasRows && reader.Read()) {
                            metas.Add(
                                new CacheMeta.Builder(reader.GetString(0))
                                    .RelativePath(reader.GetString(1))
                                    .ETag(reader.IsDBNull(2) ? string.Empty : reader.GetString(2))
                                    .Url(reader.GetString(3))
                                    .Touched(reader.GetDouble(4))
                                    .ValidTill(reader.GetDouble(5))
                                    .Build()
                            );
                        }
                    }
                }

                foreach (var meta in metas) {
                    File.Delete(meta.getFilePath());
                }

                using (var command = new SqliteCommand(deleteQuery, connection)) {
                    command.Parameters.AddWithValue("@KeyList",
                        string.Join(",", metas.Select(m => m.key.ToString()).ToArray()));
                    command.ExecuteNonQuery();
                }
            }
        }

        private void _shrinkLargeCache() {
            const string countQuery = @"SELECT COUNT(*) FROM Cache";
            var totalRecord = 0;
            using (var connection = new SqliteConnection(_dbUri)) {
                connection.Open();
                using (var command = new SqliteCommand(countQuery, connection)) {
                    using (var reader = command.ExecuteReader()) {
                        if (reader.Read()) {
                            totalRecord = reader.GetInt32(0);
                        }
                    }
                }

                if (totalRecord > maxNrOfCacheObjects) {
                    var metas = new List<CacheMeta>();
                    var overflow = totalRecord - maxNrOfCacheObjects;
                    const string overflowQuery = @"SELECT Key, FilePath, ETag, Url, Touched, ValidTill
                                                 FROM Cache ORDER BY Touched LIMIT @Overflow";
                    using (var command = new SqliteCommand(overflowQuery, connection)) {
                        command.Parameters.AddWithValue("@Overflow", overflow);
                        using (var reader = command.ExecuteReader()) {
                            while (reader.HasRows && reader.Read()) {
                                metas.Add(
                                    new CacheMeta.Builder(reader.GetString(0))
                                        .RelativePath(reader.GetString(1))
                                        .ETag(reader.IsDBNull(2) ? string.Empty : reader.GetString(2))
                                        .Url(reader.GetString(3))
                                        .Touched(reader.GetDouble(4))
                                        .ValidTill(reader.GetDouble(5))
                                        .Build()
                                );
                            }
                        }
                    }

                    foreach (var meta in metas) {
                        File.Delete(meta.getFilePath());
                    }

                    const string deleteQuery = @"DELETE FROM Cache WHERE Key in (@KeyList)";
                    using (var command = new SqliteCommand(deleteQuery, connection)) {
                        command.Parameters.AddWithValue("@KeyList",
                            string.Join(",", metas.Select(m => m.key.ToString()).ToArray()));
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private static string generateHashKey(string url) {
            using (var md5 = MD5.Create()) {
                byte[] inputBytes = Encoding.ASCII.GetBytes(url);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++) {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}