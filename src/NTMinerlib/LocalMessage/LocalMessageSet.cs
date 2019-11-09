﻿using LiteDB;
using NTMiner.MinerClient;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner.LocalMessage {
    public class LocalMessageSet : ILocalMessageSet {
        private readonly string _connectionString;
        private readonly LinkedList<ILocalMessage> _records = new LinkedList<ILocalMessage>();

        public LocalMessageSet(string dbFileFullName) {
            if (!string.IsNullOrEmpty(dbFileFullName)) {
                _connectionString = $"filename={dbFileFullName};journal=false";
            }
            VirtualRoot.BuildCmdPath<AddLocalMessageCommand>(action: message => {
                if (string.IsNullOrEmpty(_connectionString)) {
                    return;
                }
                InitOnece();
                var data = LocalMessageData.Create(message.Input);
                // TODO:批量持久化，异步持久化
                List<ILocalMessage> removes = new List<ILocalMessage>();
                lock (_locker) {
                    _records.AddFirst(data);
                    while (_records.Count > NTKeyword.LocalMessageSetCapacity) {
                        var toRemove = _records.Last;
                        removes.Add(toRemove.Value);
                        _records.RemoveLast();
                        using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                            var col = db.GetCollection<LocalMessageData>();
                            col.Delete(toRemove.Value.Id);
                        }
                    }
                }
                using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                    var col = db.GetCollection<LocalMessageData>();
                    col.Insert(data);
                }
                VirtualRoot.RaiseEvent(new LocalMessageAddedEvent(data, removes));
            });
        }

        public void Clear() {
            if (string.IsNullOrEmpty(_connectionString)) {
                return;
            }
            using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                lock (_locker) {
                    _records.Clear();
                }
                db.DropCollection(nameof(LocalMessageData));
            }
            VirtualRoot.RaiseEvent(new LocalMessageClearedEvent());
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
                    if (string.IsNullOrEmpty(_connectionString)) {
                        return;
                    }
                    using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                        var col = db.GetCollection<LocalMessageData>();
                        foreach (var item in col.FindAll().OrderBy(a => a.Timestamp)) {
                            if (_records.Count < NTKeyword.LocalMessageSetCapacity) {
                                _records.AddFirst(item);
                            }
                            else {
                                col.Delete(item.Id);
                            }
                        }
                    }
                    _isInited = true;
                }
            }
        }

        public IEnumerator<ILocalMessage> GetEnumerator() {
            InitOnece();
            return _records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            InitOnece();
            return _records.GetEnumerator();
        }
    }
}
