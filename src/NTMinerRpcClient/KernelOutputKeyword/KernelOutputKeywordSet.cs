﻿using LiteDB;
using NTMiner.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NTMiner.KernelOutputKeyword {
    public class KernelOutputKeywordSet : IKernelOutputKeywordSet {
        private readonly Dictionary<Guid, KernelOutputKeywordData> _dicById = new Dictionary<Guid, KernelOutputKeywordData>();
        private readonly string _connectionString;

        public KernelOutputKeywordSet(string dbFileFullName, bool isServer) {
            if (!string.IsNullOrEmpty(dbFileFullName)) {
                _connectionString = $"filename={dbFileFullName};journal=false";
            }
            if (!isServer) {
                VirtualRoot.BuildCmdPath<LoadKernelOutputKeywordCommand>(action: message => {
                    if (!VirtualRoot.IsKernelOutputKeywordVisible) {
                        return;
                    }
                    DateTime localTimestamp = VirtualRoot.LocalKernelOutputKeywordSetTimestamp;
                    // 如果已知服务器端最新内核输出关键字时间戳不比本地已加载的最新内核输出关键字时间戳新就不用加载了
                    if (message.KnowKernelOutputKeywordTimestamp <= Timestamp.GetTimestamp(localTimestamp)) {
                        return;
                    }
                    OfficialServer.KernelOutputKeywordService.GetKernelOutputKeywords((response, e) => {
                        if (response.IsSuccess()) {
                            Guid[] toRemoves = _dicById.Where(a => a.Value.DataLevel == DataLevel.Global).Select(a => a.Key).ToArray();
                            foreach (var id in toRemoves) {
                                _dicById.Remove(id);
                            }
                            DateTime maxTime = localTimestamp;
                            if (response.Data.Count != 0) {
                                foreach (var item in response.Data) {
                                    if (item.Timestamp > maxTime) {
                                        maxTime = item.Timestamp;
                                    }
                                    item.SetDataLevel(DataLevel.Global);
                                    _dicById.Add(item.Id, item);
                                }
                                if (maxTime != localTimestamp) {
                                    VirtualRoot.LocalKernelOutputKeywordSetTimestamp = maxTime;
                                }
                                CacheServerKernelOutputKeywords(response.Data);
                                VirtualRoot.RaiseEvent(new KernelOutputKeywordLoadedEvent(response.Data));
                            }
                        }
                    });
                });
            }
            VirtualRoot.BuildCmdPath<AddOrUpdateKernelOutputKeywordCommand>(action: (message) => {
                InitOnece();
                if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                    throw new ArgumentNullException();
                }
                if (string.IsNullOrEmpty(message.Input.MessageType)) {
                    throw new ValidationException("MessageType can't be null or empty");
                }
                if (string.IsNullOrEmpty(message.Input.Keyword)) {
                    throw new ValidationException("Keyword can't be null or empty");
                }
                if (_dicById.Values.Any(a => a.KernelOutputId == message.Input.KernelOutputId && a.Keyword == message.Input.Keyword && a.Id != message.Input.GetId())) {
                    throw new ValidationException($"关键字{message.Input.Keyword}已存在");
                }
                if (_dicById.TryGetValue(message.Input.GetId(), out KernelOutputKeywordData exist)) {
                    exist.Update(message.Input);
                    using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                        var col = db.GetCollection<KernelOutputKeywordData>();
                        col.Update(exist);
                    }
                }
                else {
                    KernelOutputKeywordData entity = new KernelOutputKeywordData().Update(message.Input);
                    _dicById.Add(entity.Id, entity);
                    using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                        var col = db.GetCollection<KernelOutputKeywordData>();
                        col.Insert(entity);
                    }
                }
            });
            VirtualRoot.BuildCmdPath<RemoveKernelOutputKeywordCommand>(action: (message) => {
                InitOnece();
                if (message == null || message.EntityId == Guid.Empty) {
                    throw new ArgumentNullException();
                }
                if (!_dicById.ContainsKey(message.EntityId)) {
                    return;
                }
                KernelOutputKeywordData entity = _dicById[message.EntityId];
                _dicById.Remove(entity.GetId());
                using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                    var col = db.GetCollection<KernelOutputKeywordData>();
                    col.Delete(message.EntityId);
                }
            });
        }

        private const string fileName = "ServerKernelOutputKeywords.json";
        private void CacheServerKernelOutputKeywords(List<KernelOutputKeywordData> data) {
            string json = VirtualRoot.JsonSerializer.Serialize(data);
            string fileId = $"$/cache/{fileName}";
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
                using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                    db.FileStorage.Upload(fileId, fileName, ms);
                }
            }
        }

        private List<KernelOutputKeywordData> GetServerKernelOutputKeywordsFromCache() {
            try {
                using (LiteDatabase db = new LiteDatabase(_connectionString))
                using (MemoryStream ms = new MemoryStream()) {
                    string fileId = $"$/cache/{fileName}";
                    db.FileStorage.Download(fileId, ms);
                    var json = Encoding.UTF8.GetString(ms.ToArray());
                    return VirtualRoot.JsonSerializer.Deserialize<List<KernelOutputKeywordData>>(json);
                }
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return new List<KernelOutputKeywordData>();
            }
        }

        private bool _isInited = false;
        private readonly object _locker = new object();

        private void InitOnece() {
            if (_isInited) {
                return;
            }
            Init();
        }

        private void Init() {
            lock (_locker) {
                if (!_isInited) {
                    foreach (var item in GetServerKernelOutputKeywordsFromCache()) {
                        if (!_dicById.ContainsKey(item.GetId())) {
                            item.SetDataLevel(DataLevel.Global);
                            _dicById.Add(item.GetId(), item);
                        }
                    }
                    using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                        var col = db.GetCollection<KernelOutputKeywordData>();
                        foreach (var item in col.FindAll()) {
                            if (!_dicById.ContainsKey(item.GetId())) {
                                item.SetDataLevel(DataLevel.Profile);
                                _dicById.Add(item.GetId(), item);
                            }
                        }
                    }
                    _isInited = true;
                }
            }
        }

        public IEnumerable<IKernelOutputKeyword> GetKeywords(Guid kernelOutputId) {
            InitOnece();
            return _dicById.Values.Where(a => a.KernelOutputId == kernelOutputId);
        }

        public bool Contains(Guid kernelOutputId, string keyword) {
            InitOnece();
            return _dicById.Values.Any(a => a.KernelOutputId == kernelOutputId && a.Keyword == keyword);
        }

        public bool TryGetKernelOutputKeyword(Guid id, out IKernelOutputKeyword keyword) {
            InitOnece();
            var result = _dicById.TryGetValue(id, out KernelOutputKeywordData data);
            keyword = data;
            return result;
        }

        public IEnumerator<IKernelOutputKeyword> GetEnumerator() {
            InitOnece();
            return _dicById.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            InitOnece();
            return _dicById.Values.GetEnumerator();
        }
    }
}
