﻿using NTMiner.Core;
using NTMiner.Vms;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NTMiner {
    public partial class AppContext {
        public class OverClockDataViewModels : ViewModelBase, IEnumerable<OverClockDataViewModel> {
            public static readonly OverClockDataViewModels Instance = new OverClockDataViewModels();
            private readonly Dictionary<Guid, OverClockDataViewModel> _dicById = new Dictionary<Guid, OverClockDataViewModel>();

            private OverClockDataViewModels() {
#if DEBUG
                Write.Stopwatch.Start();
#endif
                if (WpfUtil.IsInDesignMode) {
                    return;
                }
                Init(refresh: false);
                AppContextEventPath<OverClockDataSetInitedEvent>("超频建议集初始化后", LogEnum.DevConsole,
                    action: message => {
                        Init(refresh: true);
                    });
                AppContextEventPath<OverClockDataAddedEvent>("添加超频建议后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        if (!_dicById.ContainsKey(message.Source.GetId())) {
                            _dicById.Add(message.Source.GetId(), new OverClockDataViewModel(message.Source));
                            CoinViewModel coinVm;
                            if (AppContext.Instance.CoinVms.TryGetCoinVm(message.Source.CoinId, out coinVm)) {
                                coinVm.OnPropertyChanged(nameof(coinVm.OverClockDatas));
                            }
                        }
                    });
                AppContextEventPath<OverClockDataUpdatedEvent>("更新超频建议后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        _dicById[message.Source.GetId()].Update(message.Source);
                    });
                AppContextEventPath<OverClockDataRemovedEvent>("删除超频建议后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        _dicById.Remove(message.Source.GetId());
                        CoinViewModel coinVm;
                        if (AppContext.Instance.CoinVms.TryGetCoinVm(message.Source.CoinId, out coinVm)) {
                            coinVm.OnPropertyChanged(nameof(coinVm.OverClockDatas));
                        }
                    });
#if DEBUG
                var elapsedMilliseconds = Write.Stopwatch.Stop();
                Write.DevTimeSpan($"耗时{elapsedMilliseconds}毫秒 {this.GetType().Name}.ctor");
#endif
            }

            private void Init(bool refresh) {
                _dicById.Clear();
                foreach (var item in NTMinerRoot.Instance.OverClockDataSet) {
                    _dicById.Add(item.GetId(), new OverClockDataViewModel(item));
                }
                if (refresh) {
                    foreach (var coinVm in AppContext.Instance.CoinVms.AllCoins) {
                        coinVm.OnPropertyChanged(nameof(coinVm.OverClockDatas));
                    }
                }
            }

            public bool TryGetMineWorkVm(Guid id, out OverClockDataViewModel minerGroupVm) {
                return _dicById.TryGetValue(id, out minerGroupVm);
            }

            public IEnumerator<OverClockDataViewModel> GetEnumerator() {
                return _dicById.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return _dicById.Values.GetEnumerator();
            }
        }
    }
}
