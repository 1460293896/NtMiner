﻿using NTMiner.Core.Gpus;
using NTMiner.MinerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace NTMiner.Vms {
    public class GpuViewModel : ViewModelBase, IGpu {
        private int _index;
        private string _busId;
        private string _name;
        private int _temperature;
        private uint _fanSpeed;
        private uint _powerUsage;
        private int _coreClockDelta;
        private int _memoryClockDelta;
        private int _coreClockDeltaMin;
        private int _coreClockDeltaMax;
        private int _memoryClockDeltaMin;
        private int _memoryClockDeltaMax;
        private int _cool;
        private int _coolMin;
        private int _coolMax;
        private double _powerMin;
        private double _powerMax;
        private double _powerDefault;
        private int _powerCapacity;
        private int _tempLimitMin;
        private int _tempLimitDefault;
        private int _tempLimitMax;
        private int _tempLimit;
        private ulong _totalMemory;
        private int _coreVoltage;
        private int _memoryVoltage;
        private int _voltMin;
        private int _voltMax;
        private int _voltDefault;

        public GpuViewModel(IGpu data) {
            _index = data.Index;
            _busId = data.BusId;
            _name = data.Name;
            _totalMemory = data.TotalMemory;
            _temperature = data.Temperature;
            _fanSpeed = data.FanSpeed;
            _powerUsage = data.PowerUsage;
            _coreClockDelta = data.CoreClockDelta;
            _memoryClockDelta = data.MemoryClockDelta;
            _coreClockDeltaMin = data.CoreClockDeltaMin;
            _coreClockDeltaMax = data.CoreClockDeltaMax;
            _memoryClockDeltaMin = data.MemoryClockDeltaMin;
            _memoryClockDeltaMax = data.MemoryClockDeltaMax;
            _cool = data.Cool;
            _coolMin = data.CoolMin;
            _coolMax = data.CoolMax;
            _powerCapacity = data.PowerCapacity;
            _powerMin = data.PowerMin;
            _powerMax = data.PowerMax;
            _powerDefault = data.PowerDefault;
            _tempLimit = data.TempLimit;
            _tempLimitDefault = data.TempLimitDefault;
            _tempLimitMax = data.TempLimitMax;
            _tempLimitMin = data.TempLimitMin;
            _coreVoltage = data.CoreVoltage;
            _memoryVoltage = data.MemoryVoltage;
            _voltMin = data.VoltMin;
            _voltMax = data.VoltMax;
            _voltDefault = data.VoltDefault;
        }

        private readonly bool _isGpuData;
        private readonly IGpuStaticData[] _gpuDatas;
        public GpuViewModel(IGpuStaticData data, IGpuStaticData[] gpuDatas) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }
            if (gpuDatas == null) {
                throw new ArgumentNullException(nameof(gpuDatas));
            }
            _isGpuData = true;
            _gpuDatas = gpuDatas.Where(a => a.Index != NTMinerRoot.GpuAllId).ToArray();
            _index = data.Index;
            _busId = data.BusId;
            _name = data.Name;
            _totalMemory = data.TotalMemory;
            _temperature = 0;
            _fanSpeed = 0;
            _powerUsage = 0;
            _coreClockDelta = 0;
            _memoryClockDelta = 0;
            _coreClockDeltaMin = data.CoreClockDeltaMin;
            _coreClockDeltaMax = data.CoreClockDeltaMax;
            _memoryClockDeltaMin = data.MemoryClockDeltaMin;
            _memoryClockDeltaMax = data.MemoryClockDeltaMax;
            _cool = 0;
            _coolMin = data.CoolMin;
            _coolMax = data.CoolMax;
            _powerCapacity = 0;
            _powerMin = data.PowerMin;
            _powerMax = data.PowerMax;
            _powerDefault = data.PowerDefault;
            _tempLimitMin = data.TempLimitMin;
            _tempLimitMax = data.TempLimitMax;
            _tempLimitDefault = data.TempLimitDefault;
            _coreVoltage = 0;
            _memoryVoltage = 0;
            _voltMin = data.VoltMin;
            _voltMax = data.VoltMax;
            _voltDefault = data.VoltDefault;
        }

        public int Index {
            get => _index;
            set {
                if (_index != value) {
                    _index = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }

        public string IndexText {
            get {
                if (Index == NTMinerRoot.GpuAllId) {
                    return "All";
                }
                return $"GPU{Index}";
            }
        }

        public string SharpIndexText {
            get {
                if (Index == NTMinerRoot.GpuAllId) {
                    return "All";
                }
                return $"#{Index}";
            }
        }

        public string BusId {
            get {
                return _busId;
            }
            set {
                if (_busId != value) {
                    _busId = value;
                    OnPropertyChanged(nameof(BusId));
                }
            }
        }

        public string Name {
            get => _name;
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public ulong TotalMemory {
            get => _totalMemory;
            set {
                if (_totalMemory != value) {
                    _totalMemory = value;
                    OnPropertyChanged(nameof(TotalMemory));
                    OnPropertyChanged(nameof(TotalMemoryGbText));
                }
            }
        }

        public string TotalMemoryGbText {
            get {
                return Math.Round((this.TotalMemory / (double)(1024 * 1024 * 1024)), 1) + "G";
            }
        }

        public int Temperature {
            get => _temperature;
            set {
                if (_temperature != value) {
                    _temperature = value;
                    OnPropertyChanged(nameof(Temperature));
                    OnPropertyChanged(nameof(TemperatureText));
                    OnPropertyChanged(nameof(TemperatureForeground));
                }
            }
        }

        public string TemperatureText {
            get {
                if (_isGpuData) {
                    return "0℃";
                }
                if (NTMinerRoot.Instance.GpuSet == EmptyGpuSet.Instance) {
                    return "0℃";
                }
                if (this.Index == NTMinerRoot.GpuAllId && NTMinerRoot.Instance.GpuSet.Count != 0) {
                    return $"{AppContext.Instance.GpuVms.TemperatureMinText} - {AppContext.Instance.GpuVms.TemperatureMaxText}";
                }
                return this.Temperature.ToString() + "℃";
            }
        }

        public SolidColorBrush TemperatureForeground {
            get {
                if (this.Temperature >= AppContext.Instance.MinerProfileVm.MaxTemp) {
                    return Wpf.Util.RedBrush;
                }
                return Wpf.Util.BlackBrush;
            }
        }

        public uint FanSpeed {
            get => _fanSpeed;
            set {
                if (_fanSpeed != value) {
                    _fanSpeed = value;
                    OnPropertyChanged(nameof(FanSpeed));
                    OnPropertyChanged(nameof(FanSpeedText));
                }
            }
        }

        public string FanSpeedText {
            get {
                if (_isGpuData) {
                    return "0%";
                }
                if (NTMinerRoot.Instance.GpuSet == EmptyGpuSet.Instance) {
                    return "0%";
                }
                if (this.Index == NTMinerRoot.GpuAllId && NTMinerRoot.Instance.GpuSet.Count != 0) {
                    return $"{AppContext.Instance.GpuVms.FanSpeedMinText} - {AppContext.Instance.GpuVms.FanSpeedMaxText}";
                }
                return this.FanSpeed.ToString() + "%";
            }
        }

        public uint PowerUsage {
            get {
                return _powerUsage;
            }
            set {
                if (_powerUsage != value) {
                    _powerUsage = value;
                    OnPropertyChanged(nameof(PowerUsage));
                    OnPropertyChanged(nameof(PowerUsageW));
                    OnPropertyChanged(nameof(PowerUsageWText));
                    OnPropertyChanged(nameof(EChargeText));
                }
            }
        }

        public double PowerUsageW {
            get {
                return this.PowerUsage;
            }
        }

        public string PowerUsageWText {
            get {
                if (_isGpuData) {
                    return "0W";
                }
                if (NTMinerRoot.Instance.GpuSet == EmptyGpuSet.Instance) {
                    return "0W";
                }
                if (this.Index == NTMinerRoot.GpuAllId && NTMinerRoot.Instance.GpuSet.Count != 0) {
                    return $"{GetSumPower().ToString("f0")}W";
                }
                return PowerUsageW.ToString("f0") + "W";
            }
        }

        private static long GetSumPower() {
            var sum = AppContext.Instance.GpuVms.Sum(a => a.PowerUsage);
            if (AppContext.Instance.MinerProfileVm.IsPowerAppend) {
                sum = sum + AppContext.Instance.MinerProfileVm.PowerAppend * AppContext.Instance.GpuVms.Count;
                if (sum <= 0) {
                    sum = 0;
                }
            }
            return sum;
        }

        public double ECharge {
            get {
                if (_isGpuData) {
                    return 0;
                }
                if (NTMinerRoot.Instance.GpuSet == EmptyGpuSet.Instance) {
                    return 0;
                }
                if (this.Index == NTMinerRoot.GpuAllId && NTMinerRoot.Instance.GpuSet.Count != 0) {
                    return GetSumPower() * NTMinerRoot.Instance.MinerProfile.EPrice / 1000 * 24;
                }
                return (double)PowerUsageW * NTMinerRoot.Instance.MinerProfile.EPrice / 1000 * 24;
            }
        }

        public string EChargeText {
            get {
                return ECharge.ToString("f1") + "￥/天";
            }
        }

        public int CoreClockDelta {
            get { return _coreClockDelta; }
            set {
                if (_coreClockDelta != value) {
                    _coreClockDelta = value;
                    OnPropertyChanged(nameof(CoreClockDelta));
                    OnPropertyChanged(nameof(CoreClockDeltaMText));
                }
            }
        }

        public string CoreClockDeltaMText {
            get {
                if (_isGpuData) {
                    return "0M";
                }
                if (NTMinerRoot.Instance.GpuSet == EmptyGpuSet.Instance) {
                    return "0M";
                }
                if (this.Index == NTMinerRoot.GpuAllId && NTMinerRoot.Instance.GpuSet.Count != 0) {
                    int min = int.MaxValue, max = int.MinValue;
                    foreach (var item in AppContext.Instance.GpuVms) {
                        if (item.Index == NTMinerRoot.GpuAllId) {
                            continue;
                        }
                        if (item.CoreClockDelta > max) {
                            max = item.CoreClockDelta;
                        }
                        if (item.CoreClockDelta < min) {
                            min = item.CoreClockDelta;
                        }
                    }
                    return $"{min / 1000} - {max / 1000}M";
                }
                return (this.CoreClockDelta / 1000).ToString();
            }
        }

        public int MemoryClockDelta {
            get { return _memoryClockDelta; }
            set {
                if (_memoryClockDelta != value) {
                    _memoryClockDelta = value;
                    OnPropertyChanged(nameof(MemoryClockDelta));
                    OnPropertyChanged(nameof(MemoryClockDeltaMText));
                }
            }
        }

        public string MemoryClockDeltaMText {
            get {
                if (_isGpuData) {
                    return "0M";
                }
                if (NTMinerRoot.Instance.GpuSet == EmptyGpuSet.Instance) {
                    return "0M";
                }
                if (this.Index == NTMinerRoot.GpuAllId && NTMinerRoot.Instance.GpuSet.Count != 0) {
                    int min = int.MaxValue, max = int.MinValue;
                    foreach (var item in AppContext.Instance.GpuVms) {
                        if (item.Index == NTMinerRoot.GpuAllId) {
                            continue;
                        }
                        if (item.MemoryClockDelta > max) {
                            max = item.MemoryClockDelta;
                        }
                        if (item.MemoryClockDelta < min) {
                            min = item.MemoryClockDelta;
                        }
                    }
                    return $"{min / 1000} - {max / 1000}M";
                }
                return (this.MemoryClockDelta / 1000).ToString();
            }
        }

        public string CoreClockDeltaMinMaxMText {
            get {
                if (Index == NTMinerRoot.GpuAllId) {
                    if (_isGpuData) {
                        return $"{_gpuDatas.Max(a => a.CoreClockDeltaMin) / 1000}至{_gpuDatas.Min(a => a.CoreClockDeltaMax) / 1000}，默认：0";
                    }
                    else {
                        return $"{NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Max(a => a.CoreClockDeltaMin) / 1000}至{NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Min(a => a.CoreClockDeltaMax) / 1000}，默认：0";
                    }
                }
                return $"范围：{this.CoreClockDeltaMinMText} - {this.CoreClockDeltaMaxMText}，默认：0";
            }
        }

        public string MemoryClockDeltaMinMaxMText {
            get {
                if (Index == NTMinerRoot.GpuAllId) {
                    if (_isGpuData) {
                        return $"{_gpuDatas.Max(a => a.MemoryClockDeltaMin) / 1000}至{_gpuDatas.Min(a => a.MemoryClockDeltaMax) / 1000}，默认：0";
                    }
                    else {
                        return $"{NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Max(a => a.MemoryClockDeltaMin) / 1000}至{NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Min(a => a.MemoryClockDeltaMax) / 1000}，默认：0";
                    }
                }
                return $"范围：{this.MemoryClockDeltaMinMText} - {this.MemoryClockDeltaMaxMText}，默认：0";
            }
        }

        public string CoolMinMaxText {
            get {
                if (Index == NTMinerRoot.GpuAllId) {
                    if (_isGpuData) {
                        return $"{_gpuDatas.Max(a => a.CoolMin)} - {_gpuDatas.Min(a => a.CoolMax)}%，默认：0（表示驱动自控）";
                    }
                    else {
                        return $"{NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Max(a => a.CoolMin)} - {NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Min(a => a.CoolMax)}%，默认：0（表示驱动自控）";
                    }
                }
                return $"范围：{this.CoolMin} - {this.CoolMax}%，默认：0（表示驱动自控）";
            }
        }

        public string PowerMinMaxText {
            get {
                if (Index == NTMinerRoot.GpuAllId) {
                    if (_isGpuData) {
                        return $"{Math.Ceiling(_gpuDatas.Max(a => a.PowerMin))} - {(int)_gpuDatas.Min(a => a.PowerMax)}%，默认：0";
                    }
                    else {
                        return $"{Math.Ceiling(NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Max(a => a.PowerMin))} - {(int)NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Min(a => a.PowerMax)}%，默认：0";
                    }
                }
                return $"范围：{Math.Ceiling(this.PowerMin)} - {(int)this.PowerMax}%，默认：0";
            }
        }

        public string TempLimitMinMaxText {
            get {
                if (Index == NTMinerRoot.GpuAllId) {
                    if (_isGpuData) {
                        return $"{_gpuDatas.Max(a => a.TempLimitMin)} - {_gpuDatas.Min(a => a.TempLimitMax)}℃，默认：0";
                    }
                    else {
                        return $"{NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Max(a => a.TempLimitMin)} - {NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Min(a => a.TempLimitMax)}℃，默认：0";
                    }
                }
                return $"范围：{this.TempLimitMin} - {this.TempLimitMax}℃，默认：0";
            }
        }

        public string VoltageMinMaxText {
            get {
                if (Index == NTMinerRoot.GpuAllId) {
                    if (_isGpuData) {
                        return $"{_gpuDatas.Max(a => a.VoltMin)} - {_gpuDatas.Min(a => a.VoltMax)}，默认：0";
                    }
                    else {
                        return $"{NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Max(a => a.VoltMin)} - {NTMinerRoot.Instance.GpuSet.Where(a => a.Index != NTMinerRoot.GpuAllId).Min(a => a.VoltMax)}%，默认：0";
                    }
                }
                return $"范围：{this.VoltMin} - {this.VoltMax}，默认：0";
            }
        }

        public int CoreVoltage {
            get => _coreVoltage;
            set {
                if (_coreVoltage != value) {
                    _coreVoltage = value;
                    OnPropertyChanged(nameof(CoreVoltage));
                }
            }
        }

        public int MemoryVoltage {
            get => _memoryVoltage;
            set {
                if (_memoryVoltage != value) {
                    _memoryVoltage = value;
                    OnPropertyChanged(nameof(MemoryVoltage));
                }
            }
        }

        public int CoreClockDeltaMin {
            get => _coreClockDeltaMin;
            set {
                if (_coreClockDeltaMin != value) {
                    _coreClockDeltaMin = value;
                    OnPropertyChanged(nameof(CoreClockDeltaMin));
                    OnPropertyChanged(nameof(CoreClockDeltaMinMText));
                }
            }
        }
        public int CoreClockDeltaMax {
            get => _coreClockDeltaMax;
            set {
                if (_coreClockDeltaMax != value) {
                    _coreClockDeltaMax = value;
                    OnPropertyChanged(nameof(CoreClockDeltaMax));
                    OnPropertyChanged(nameof(CoreClockDeltaMaxMText));
                }
            }
        }
        public int MemoryClockDeltaMin {
            get => _memoryClockDeltaMin;
            set {
                if (_memoryClockDeltaMin != value) {
                    _memoryClockDeltaMin = value;
                    OnPropertyChanged(nameof(MemoryClockDeltaMin));
                    OnPropertyChanged(nameof(MemoryClockDeltaMinMText));
                }
            }
        }

        public int MemoryClockDeltaMax {
            get => _memoryClockDeltaMax;
            set {
                if (_memoryClockDeltaMax != value) {
                    _memoryClockDeltaMax = value;
                    OnPropertyChanged(nameof(MemoryClockDeltaMax));
                    OnPropertyChanged(nameof(MemoryClockDeltaMaxMText));
                }
            }
        }
        public string CoreClockDeltaMinMText {
            get {
                return (this.CoreClockDeltaMin / 1000).ToString();
            }
        }

        public string CoreClockDeltaMaxMText {
            get {
                return (this.CoreClockDeltaMax / 1000).ToString();
            }
        }

        public string MemoryClockDeltaMinMText {
            get {
                return (this.MemoryClockDeltaMin / 1000).ToString();
            }
        }

        public string MemoryClockDeltaMaxMText {
            get {
                return (this.MemoryClockDeltaMax / 1000).ToString();
            }
        }

        public int Cool {
            get => _cool;
            set {
                if (_cool != value) {
                    _cool = value;
                    OnPropertyChanged(nameof(Cool));
                }
            }
        }

        public int CoolMin {
            get => _coolMin;
            set {
                if (_coolMin != value) {
                    _coolMin = value;
                    OnPropertyChanged(nameof(CoolMin));
                }
            }
        }
        public int CoolMax {
            get => _coolMax;
            set {
                if (_coolMax != value) {
                    _coolMax = value;
                    OnPropertyChanged(nameof(CoolMax));
                }
            }
        }
        public double PowerMin {
            get => _powerMin;
            set {
                if (_powerMin != value) {
                    _powerMin = value;
                    OnPropertyChanged(nameof(PowerMin));
                }
            }
        }
        public double PowerMax {
            get => _powerMax;
            set {
                if (_powerMax != value) {
                    _powerMax = value;
                    OnPropertyChanged(nameof(PowerMax));
                }
            }
        }
        public double PowerDefault {
            get => _powerDefault;
            set {
                if (_powerDefault != value) {
                    _powerDefault = value;
                    OnPropertyChanged(nameof(PowerDefault));
                }
            }
        }
        public int PowerCapacity {
            get => _powerCapacity;
            set {
                if (_powerCapacity != value) {
                    _powerCapacity = value;
                    OnPropertyChanged(nameof(PowerCapacity));
                    OnPropertyChanged(nameof(PowerCapacityText));
                }
            }
        }

        public string PowerCapacityText {
            get {
                return this.PowerCapacity.ToString("f0") + "%";
            }
        }

        public int TempLimitMin {
            get => _tempLimitMin;
            set {
                if (_tempLimitMin != value) {
                    _tempLimitMin = value;
                    OnPropertyChanged(nameof(TempLimitMin));
                }
            }
        }
        public int TempLimitDefault {
            get => _tempLimitDefault;
            set {
                if (_tempLimitDefault != value) {
                    _tempLimitDefault = value;
                    OnPropertyChanged(nameof(TempLimitDefault));
                }
            }
        }
        public int TempLimitMax {
            get => _tempLimitMax;
            set {
                if (_tempLimitMax != value) {
                    _tempLimitMax = value;
                    OnPropertyChanged(nameof(TempLimitMax));
                }
            }
        }
        public int TempLimit {
            get => _tempLimit;
            set {
                if (_tempLimit != value) {
                    _tempLimit = value;
                    OnPropertyChanged(nameof(TempLimit));
                    OnPropertyChanged(nameof(TempLimitText));
                }
            }
        }

        public string TempLimitText {
            get {
                return this.TempLimit.ToString() + "℃";
            }
        }

        public int VoltMin {
            get { return _voltMin; }
            set {
                if (_voltMin != value) {
                    _voltMin = value;
                    OnPropertyChanged(nameof(VoltMin));
                }
            }
        }
        public int VoltMax {
            get { return _voltMax; }
            set {
                if (_voltMax != value) {
                    _voltMax = value;
                    OnPropertyChanged(nameof(VoltMax));
                }
            }
        }
        public int VoltDefault {
            get { return _voltDefault; }
            set {
                if (_voltDefault != value) {
                    _voltDefault = value;
                    OnPropertyChanged(nameof(VoltDefault));
                }
            }
        }

        public bool IsDeviceArgInclude {
            get => NTMinerRoot.Instance.GpuSet.GetIsUseDevice(this.Index);
            set {
                int[] old = NTMinerRoot.Instance.GpuSet.GetUseDevices();
                bool refreshAllGpu = !value && old.Length <= 1;
                NTMinerRoot.Instance.GpuSet.SetIsUseDevice(this.Index, value);
                if (refreshAllGpu) {
                    foreach (var gpuVm in AppContext.Instance.GpuVms) {
                        if (gpuVm.Index == NTMinerRoot.GpuAllId) {
                            continue;
                        }
                        gpuVm.OnPropertyChanged(nameof(IsDeviceArgInclude));
                    }
                }
                else {
                    OnPropertyChanged(nameof(IsDeviceArgInclude));
                }
                NTMinerRoot.RefreshArgsAssembly();
            }
        }
    }
}
