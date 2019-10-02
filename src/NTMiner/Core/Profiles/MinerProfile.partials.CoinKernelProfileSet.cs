﻿using NTMiner.MinerServer;
using NTMiner.Profile;
using NTMiner.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NTMiner.Core.Profiles {
    internal partial class MinerProfile {
        private class CoinKernelProfileSet {
            private readonly Dictionary<Guid, CoinKernelProfile> _dicById = new Dictionary<Guid, CoinKernelProfile>();

            private readonly INTMinerRoot _root;
            private readonly object _locker = new object();

            public CoinKernelProfileSet(INTMinerRoot root) {
                _root = root;
            }

            public void Refresh() {
                _dicById.Clear();
            }

            public ICoinKernelProfile GetCoinKernelProfile(Guid coinKernelId) {
                if (_dicById.ContainsKey(coinKernelId)) {
                    return _dicById[coinKernelId];
                }
                lock (_locker) {
                    if (_dicById.ContainsKey(coinKernelId)) {
                        return _dicById[coinKernelId];
                    }
                    CoinKernelProfile coinKernelProfile = CoinKernelProfile.Create(_root, coinKernelId);
                    _dicById.Add(coinKernelId, coinKernelProfile);

                    return coinKernelProfile;
                }
            }

            public IEnumerable<ICoinKernelProfile> GetCoinKernelProfiles() {
                return _dicById.Values;
            }

            public void SetCoinKernelProfileProperty(Guid coinKernelId, string propertyName, object value) {
                CoinKernelProfile coinKernelProfile = (CoinKernelProfile)GetCoinKernelProfile(coinKernelId);
                coinKernelProfile.SetValue(propertyName, value);
            }

            private class CoinKernelProfile : ICoinKernelProfile {
                private static readonly CoinKernelProfile Empty = new CoinKernelProfile(new CoinKernelProfileData());
                public static CoinKernelProfile Create(INTMinerRoot root, Guid coinKernelId) {
                    if (root.CoinKernelSet.TryGetCoinKernel(coinKernelId, out ICoinKernel coinKernel)) {
                        IRepository<CoinKernelProfileData> repository = NTMinerRoot.CreateLocalRepository<CoinKernelProfileData>();
                        CoinKernelProfileData data = repository.GetByKey(coinKernelId);
                        if (data == null) {
                            double dualCoinWeight = GetDualCoinWeight(root, coinKernel.KernelId);
                            data = CoinKernelProfileData.CreateDefaultData(coinKernel.GetId(), dualCoinWeight);
                        }
                        CoinKernelProfile coinProfile = new CoinKernelProfile(data);

                        var defaultInputSegments = coinKernel.InputSegments.Where(a => a.IsDefault && a.TargetGpu.IsSupportedGpu(NTMinerRoot.Instance.GpuSet.GpuType)).ToArray();
                        string touchedArgs = coinProfile.TouchedArgs;
                        if (coinProfile.CustomArgs == null) {
                            coinProfile.CustomArgs = string.Empty;
                        }
                        if (string.IsNullOrEmpty(touchedArgs)) {
                            foreach (var defaultInputSegment in defaultInputSegments) {
                                if (!coinProfile.CustomArgs.Contains(defaultInputSegment.Segment)) {
                                    if (coinProfile.CustomArgs.Length == 0) {
                                        coinProfile.CustomArgs += defaultInputSegment.Segment;
                                    }
                                    else {
                                        coinProfile.CustomArgs += " " + defaultInputSegment.Segment;
                                    }
                                }
                            }
                        }
                        else {
                            foreach (var defaultInputSegment in defaultInputSegments) {
                                if (!touchedArgs.Contains(defaultInputSegment.Segment) && !coinProfile.CustomArgs.Contains(defaultInputSegment.Segment)) {
                                    if (coinProfile.CustomArgs.Length == 0) {
                                        coinProfile.CustomArgs += defaultInputSegment.Segment;
                                    }
                                    else {
                                        coinProfile.CustomArgs += " " + defaultInputSegment.Segment;
                                    }
                                }
                            }
                        }
                        return coinProfile;
                    }
                    else {
                        return Empty;
                    }
                }

                // 获取默认双挖权重
                private static double GetDualCoinWeight(INTMinerRoot root, Guid kernelId) {
                    double dualCoinWeight = 0;
                    if (root.KernelSet.TryGetKernel(kernelId, out IKernel kernel)) {
                        if (root.KernelInputSet.TryGetKernelInput(kernel.KernelInputId, out IKernelInput kernelInput)) {
                            dualCoinWeight = (kernelInput.DualWeightMin + kernelInput.DualWeightMax) / 2;
                        }
                    }
                    return dualCoinWeight;
                }

                private CoinKernelProfileData _data;
                private CoinKernelProfile(CoinKernelProfileData data) {
                    _data = data ?? throw new ArgumentNullException(nameof(data));
                }

                [IgnoreReflectionSet]
                public Guid CoinKernelId {
                    get => _data.CoinKernelId;
                    private set {
                        _data.CoinKernelId = value;
                    }
                }

                public bool IsDualCoinEnabled {
                    get => _data.IsDualCoinEnabled;
                    private set {
                        _data.IsDualCoinEnabled = value;
                    }
                }
                public Guid DualCoinId {
                    get => _data.DualCoinId;
                    private set {
                        _data.DualCoinId = value;
                    }
                }

                public double DualCoinWeight {
                    get => _data.DualCoinWeight;
                    private set {
                        _data.DualCoinWeight = value;
                    }
                }

                public bool IsAutoDualWeight {
                    get => _data.IsAutoDualWeight;
                    private set {
                        _data.IsAutoDualWeight = value;
                    }
                }

                public string CustomArgs {
                    get => _data.CustomArgs;
                    private set {
                        _data.CustomArgs = value;
                    }
                }

                public string TouchedArgs {
                    get { return _data.TouchedArgs; }
                    set {
                        _data.TouchedArgs = value;
                    }
                }

                private static Dictionary<string, PropertyInfo> _sProperties;
                [IgnoreReflectionSet]
                private static Dictionary<string, PropertyInfo> Properties {
                    get {
                        if (_sProperties == null) {
                            _sProperties = GetPropertiesCanSet<CoinKernelProfile>();
                        }
                        return _sProperties;
                    }
                }

                public void SetValue(string propertyName, object value) {
                    if (Properties.TryGetValue(propertyName, out PropertyInfo propertyInfo)) {
                        if (propertyInfo.CanWrite) {
                            if (propertyInfo.PropertyType == typeof(Guid)) {
                                value = VirtualRoot.ConvertToGuid(value);
                            }
                            var oldValue = propertyInfo.GetValue(this, null);
                            if (oldValue != value) {
                                propertyInfo.SetValue(this, value, null);
                                IRepository<CoinKernelProfileData> repository = NTMinerRoot.CreateLocalRepository<CoinKernelProfileData>();
                                repository.Update(_data);
                                VirtualRoot.Happened(new CoinKernelProfilePropertyChangedEvent(this.CoinKernelId, propertyName));
                            }
                        }
                    }
                }

                public override string ToString() {
                    if (_data == null) {
                        return string.Empty;
                    }
                    return _data.ToString();
                }
            }
        }
    }
}
