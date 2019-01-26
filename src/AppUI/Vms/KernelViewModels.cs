﻿using NTMiner.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner.Vms {
    public class KernelViewModels : ViewModelBase {
        public static readonly KernelViewModels Current = new KernelViewModels();

        private readonly Dictionary<Guid, KernelViewModel> _dicById = new Dictionary<Guid, KernelViewModel>();

        private KernelViewModels() {
            Global.Access<KernelSetRefreshedEvent>(
                Guid.Parse("CD2F6575-7032-4D9F-AFFA-8B6A4EAACAD2"),
                "内核数据集刷新后刷新Vm内存",
                LogEnum.Console,
                action: message => {
                    Init(isRefresh: true);
                });
            Global.Access<KernelAddedEvent>(
                Guid.Parse("35917be2-7373-440d-b083-8edc1050f2cc"),
                "添加了内核后调整VM内存",
                LogEnum.Log,
                action: (message) => {
                    _dicById.Add(message.Source.GetId(), new KernelViewModel(message.Source));
                    OnPropertyChanged(nameof(AllKernels));
                    foreach (var coinKernelVm in CoinKernelViewModels.Current.AllCoinKernels.Where(a => a.KernelId == message.Source.GetId())) {
                        coinKernelVm.OnPropertyChanged(nameof(coinKernelVm.IsSupportDualMine));
                    }
                });
            Global.Access<KernelRemovedEvent>(
                Guid.Parse("c8a5fdca-85c3-40ae-8c79-41af2aa1d4da"),
                "删除了内核后调整VM内存",
                LogEnum.Log,
                action: message => {
                    _dicById.Remove(message.Source.GetId());
                    OnPropertyChanged(nameof(AllKernels));
                    foreach (var coinKernelVm in CoinKernelViewModels.Current.AllCoinKernels.Where(a => a.KernelId == message.Source.GetId())) {
                        coinKernelVm.OnPropertyChanged(nameof(coinKernelVm.IsSupportDualMine));
                    }
                });
            Global.Access<KernelUpdatedEvent>(
                Guid.Parse("98b29a2f-fbcf-466d-81a4-ddbbc4594225"),
                "更新了内核后调整VM内存",
                LogEnum.Log,
                action: message => {
                    var entity = _dicById[message.Source.GetId()];
                    int sortNumber = entity.SortNumber;
                    Guid kernelInputId = entity.KernelInputId;
                    entity.Update(message.Source);
                    if (sortNumber != entity.SortNumber) {
                        KernelPageViewModel.Current.OnPropertyChanged(nameof(KernelPageViewModel.QueryResults));
                    }
                    if (kernelInputId != entity.KernelInputId) {
                        Global.Execute(new RefreshArgsAssemblyCommand());
                    }
                });

            Init();
        }

        private void Init(bool isRefresh = false) {
            if (isRefresh) {
                foreach (var item in NTMinerRoot.Current.KernelSet) {
                    KernelViewModel vm;
                    if (_dicById.TryGetValue(item.GetId(), out vm)) {
                        Global.Execute(new UpdateKernelCommand(item));
                    }
                    else {
                        Global.Execute(new AddKernelCommand(item));
                    }
                }
            }
            else {
                foreach (var item in NTMinerRoot.Current.KernelSet) {
                    _dicById.Add(item.GetId(), new KernelViewModel(item));
                }
            }
        }

        public bool TryGetKernelVm(Guid kernelId, out KernelViewModel kernelVm) {
            return _dicById.TryGetValue(kernelId, out kernelVm);
        }

        public List<KernelViewModel> AllKernels {
            get {
                return _dicById.Values.ToList();
            }
        }
    }
}
