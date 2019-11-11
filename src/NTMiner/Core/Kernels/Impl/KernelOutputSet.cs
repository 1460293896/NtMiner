﻿using NTMiner.Core.Gpus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NTMiner.Core.Kernels.Impl {
    public class KernelOutputSet : IKernelOutputSet {
        private readonly Dictionary<Guid, KernelOutputData> _dicById = new Dictionary<Guid, KernelOutputData>();

        private readonly IServerContext _context;
        public KernelOutputSet(IServerContext context) {
            _context = context;
            #region 接线
            _context.BuildCmdPath<AddKernelOutputCommand>("添加内核输出组", LogEnum.DevConsole,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                        throw new ArgumentNullException();
                    }
                    if (_dicById.ContainsKey(message.Input.GetId())) {
                        return;
                    }
                    KernelOutputData entity = new KernelOutputData().Update(message.Input);
                    _dicById.Add(entity.Id, entity);
                    var repository = NTMinerRoot.CreateServerRepository<KernelOutputData>();
                    repository.Add(entity);

                    VirtualRoot.RaiseEvent(new KernelOutputAddedEvent(entity));
                });
            _context.BuildCmdPath<UpdateKernelOutputCommand>("更新内核输出组", LogEnum.DevConsole,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.Input == null || message.Input.GetId() == Guid.Empty) {
                        throw new ArgumentNullException();
                    }
                    if (string.IsNullOrEmpty(message.Input.Name)) {
                        throw new ValidationException($"{nameof(message.Input.Name)} can't be null or empty");
                    }
                    if (!_dicById.ContainsKey(message.Input.GetId())) {
                        return;
                    }
                    KernelOutputData entity = _dicById[message.Input.GetId()];
                    if (ReferenceEquals(entity, message.Input)) {
                        return;
                    }
                    entity.Update(message.Input);
                    var repository = NTMinerRoot.CreateServerRepository<KernelOutputData>();
                    repository.Update(entity);

                    VirtualRoot.RaiseEvent(new KernelOutputUpdatedEvent(entity));
                });
            _context.BuildCmdPath<RemoveKernelOutputCommand>("移除内核输出组", LogEnum.DevConsole,
                action: (message) => {
                    InitOnece();
                    if (message == null || message.EntityId == Guid.Empty) {
                        throw new ArgumentNullException();
                    }
                    if (!_dicById.ContainsKey(message.EntityId)) {
                        return;
                    }
                    IKernel[] outputUsers = context.KernelSet.Where(a => a.KernelOutputId == message.EntityId).ToArray();
                    if (outputUsers.Length != 0) {
                        throw new ValidationException($"这些内核在使用该内核输出组，删除前请先解除使用：{string.Join(",", outputUsers.Select(a => a.GetFullName()))}");
                    }
                    KernelOutputData entity = _dicById[message.EntityId];
                    List<Guid> kernelOutputTranslaterIds = context.KernelOutputTranslaterSet.Where(a => a.KernelOutputId == entity.Id).Select(a => a.GetId()).ToList();
                    foreach (var kernelOutputTranslaterId in kernelOutputTranslaterIds) {
                        VirtualRoot.Execute(new RemoveKernelOutputTranslaterCommand(kernelOutputTranslaterId));
                    }
                    _dicById.Remove(entity.GetId());
                    var repository = NTMinerRoot.CreateServerRepository<KernelOutputData>();
                    repository.Remove(message.EntityId);

                    VirtualRoot.RaiseEvent(new KernelOutputRemovedEvent(entity));
                });
            #endregion
        }

        private bool _isInited = false;
        private readonly object _locker = new object();

        private void InitOnece() {
            if (_isInited) {
                return;
            }
            Init();
        }

        // 填充空间
        private void Init() {
            lock (_locker) {
                if (!_isInited) {
                    var repository = NTMinerRoot.CreateServerRepository<KernelOutputData>();
                    foreach (var item in repository.GetAll()) {
                        if (!_dicById.ContainsKey(item.GetId())) {
                            _dicById.Add(item.GetId(), item);
                        }
                    }
                    _isInited = true;
                }
            }
        }

        public bool Contains(Guid id) {
            InitOnece();
            return _dicById.ContainsKey(id);
        }

        public bool TryGetKernelOutput(Guid id, out IKernelOutput kernelOutput) {
            InitOnece();
            KernelOutputData data;
            var result = _dicById.TryGetValue(id, out data);
            kernelOutput = data;
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            InitOnece();
            return _dicById.Values.GetEnumerator();
        }

        public IEnumerator<IKernelOutput> GetEnumerator() {
            InitOnece();
            return _dicById.Values.GetEnumerator();
        }

        private DateTime _kernelRestartKeywordOn = DateTime.MinValue;
        private string _preline;
        public void Pick(ref string line, IMineContext mineContext) {
            try {
                InitOnece();
                if (string.IsNullOrEmpty(line)) {
                    return;
                }
                if (!string.IsNullOrEmpty(mineContext.KernelOutput.KernelRestartKeyword) && line.Contains(mineContext.KernelOutput.KernelRestartKeyword)) {
                    if (_kernelRestartKeywordOn.AddSeconds(10) < DateTime.Now) {
                        mineContext.KernelSelfRestartCount = mineContext.KernelSelfRestartCount + 1;
                        _kernelRestartKeywordOn = DateTime.Now;
                        VirtualRoot.RaiseEvent(new KernelSelfRestartedEvent());
                    }
                }
                ICoin coin = mineContext.MainCoin;
                bool isDual = false;
                Guid poolId = mineContext.MainCoinPool.GetId();
                // 如果是双挖上下文且当前输入行中没有主币关键字则视为双挖币
                if ((mineContext is IDualMineContext dualMineContext) && !line.Contains(mineContext.MainCoin.Code)) {
                    isDual = true;
                    coin = dualMineContext.DualCoin;
                    poolId = dualMineContext.DualCoinPool.GetId();
                }
                INTMinerRoot root = NTMinerRoot.Instance;
                // 这些方法输出的是事件消息
                PickTotalSpeed(root, line, mineContext.KernelOutput, isDual);
                PickGpuSpeed(root, mineContext, line, mineContext.KernelOutput, isDual);
                PickTotalShare(root, line, mineContext.KernelOutput, coin, isDual);
                PickAcceptShare(root, line, mineContext.KernelOutput, coin, isDual);
                PickAcceptOneShare(root, mineContext, line, _preline, mineContext.KernelOutput, coin, isDual);
                PickRejectPattern(root, line, mineContext.KernelOutput, coin, isDual);
                PickRejectOneShare(root, mineContext, line, _preline, mineContext.KernelOutput, coin, isDual);
                PickRejectPercent(root, line, mineContext.KernelOutput, coin, isDual);
                PickPoolDelay(line, mineContext.KernelOutput, isDual, poolId);
                if (!isDual) {
                    // 决定不支持双挖的单卡份额统计
                    PicFoundOneShare(root, mineContext, line, _preline, mineContext.KernelOutput);
                    PicGotOneIncorrectShare(root, mineContext, line, _preline, mineContext.KernelOutput);
                }
                // 如果是像BMiner那样的主币和双挖币的输出在同一行那样的模式则一行输出既要视为主币又要视为双挖币
                if (isDual && mineContext.KernelOutput.IsDualInSameLine) {
                    coin = mineContext.MainCoin;
                    isDual = false;
                    PickTotalSpeed(root, line, mineContext.KernelOutput, isDual);
                    PickGpuSpeed(root, mineContext, line, mineContext.KernelOutput, isDual);
                    PickTotalShare(root, line, mineContext.KernelOutput, coin, isDual);
                    PickAcceptShare(root, line, mineContext.KernelOutput, coin, isDual);
                    PickAcceptOneShare(root, mineContext, line, _preline, mineContext.KernelOutput, coin, isDual);
                    PickRejectPattern(root, line, mineContext.KernelOutput, coin, isDual);
                    PickRejectOneShare(root, mineContext, line, _preline, mineContext.KernelOutput, coin, isDual);
                    PickRejectPercent(root, line, mineContext.KernelOutput, coin, isDual);
                    PickPoolDelay(line, mineContext.KernelOutput, isDual, poolId);
                }
                _preline = line;
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
            }
        }

        #region private methods
        #region PickTotalSpeed
        private static void PickTotalSpeed(INTMinerRoot root, string input, IKernelOutput kernelOutput, bool isDual) {
            string totalSpeedPattern = kernelOutput.TotalSpeedPattern;
            if (isDual) {
                totalSpeedPattern = kernelOutput.DualTotalSpeedPattern;
            }
            if (string.IsNullOrEmpty(totalSpeedPattern)) {
                return;
            }
            Regex regex = VirtualRoot.GetRegex(totalSpeedPattern);
            Match match = regex.Match(input);
            if (match.Success) {
                string totalSpeedText = match.Groups[NTKeyword.TotalSpeedGroupName].Value;
                string totalSpeedUnit = match.Groups[NTKeyword.TotalSpeedUnitGroupName].Value;
                if (string.IsNullOrEmpty(totalSpeedUnit)) {
                    if (isDual) {
                        totalSpeedUnit = kernelOutput.DualSpeedUnit;
                    }
                    else {
                        totalSpeedUnit = kernelOutput.SpeedUnit;
                    }
                }
                double totalSpeed;
                if (double.TryParse(totalSpeedText, out totalSpeed)) {
                    double totalSpeedL = totalSpeed.FromUnitSpeed(totalSpeedUnit);
                    var now = DateTime.Now;
                    IGpusSpeed gpuSpeeds = NTMinerRoot.Instance.GpusSpeed;
                    gpuSpeeds.SetCurrentSpeed(NTMinerRoot.GpuAllId, totalSpeedL, isDual, now);
                    string gpuSpeedPattern = kernelOutput.GpuSpeedPattern;
                    if (isDual) {
                        gpuSpeedPattern = kernelOutput.DualGpuSpeedPattern;
                    }
                    if (string.IsNullOrEmpty(gpuSpeedPattern) && root.GpuSet.Count != 0) {
                        // 平分总算力
                        double gpuSpeedL = totalSpeedL / root.GpuSet.Count;
                        foreach (var item in gpuSpeeds) {
                            if (item.Gpu.Index != NTMinerRoot.GpuAllId) {
                                gpuSpeeds.SetCurrentSpeed(item.Gpu.Index, gpuSpeedL, isDual, now);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region PickPoolDelay
        private static void PickPoolDelay(string input, IKernelOutput kernelOutput, bool isDual, Guid poolId) {
            string poolDelayPattern = kernelOutput.PoolDelayPattern;
            if (isDual) {
                poolDelayPattern = kernelOutput.DualPoolDelayPattern;
            }
            if (string.IsNullOrEmpty(poolDelayPattern)) {
                return;
            }
            Regex regex = VirtualRoot.GetRegex(poolDelayPattern);
            Match match = regex.Match(input);
            if (match.Success) {
                string poolDelayText = match.Groups[NTKeyword.PoolDelayGroupName].Value;
                VirtualRoot.RaiseEvent(new PoolDelayPickedEvent(poolId, isDual, poolDelayText));
            }
        }
        #endregion

        #region PickGpuSpeed
        private static void PickGpuSpeed(INTMinerRoot root, IMineContext mineContext, string input, IKernelOutput kernelOutput, bool isDual) {
            string gpuSpeedPattern = kernelOutput.GpuSpeedPattern;
            if (isDual) {
                gpuSpeedPattern = kernelOutput.DualGpuSpeedPattern;
            }
            if (string.IsNullOrEmpty(gpuSpeedPattern)) {
                return;
            }
            var now = DateTime.Now;
            bool hasGpuId = gpuSpeedPattern.Contains($"?<{NTKeyword.GpuIndexGroupName}>");
            Regex regex = VirtualRoot.GetRegex(gpuSpeedPattern);
            MatchCollection matches = regex.Matches(input);
            if (matches.Count > 0) {
                IGpusSpeed gpuSpeeds = NTMinerRoot.Instance.GpusSpeed;
                for (int i = 0; i < matches.Count; i++) {
                    Match match = matches[i];
                    string gpuSpeedText = match.Groups[NTKeyword.GpuSpeedGroupName].Value;
                    string gpuSpeedUnit = match.Groups[NTKeyword.GpuSpeedUnitGroupName].Value;
                    if (string.IsNullOrEmpty(gpuSpeedUnit)) {
                        if (isDual) {
                            gpuSpeedUnit = kernelOutput.DualSpeedUnit;
                        }
                        else {
                            gpuSpeedUnit = kernelOutput.SpeedUnit;
                        }
                    }
                    int gpu = i;
                    if (hasGpuId) {
                        string gpuText = match.Groups[NTKeyword.GpuIndexGroupName].Value;
                        if (!int.TryParse(gpuText, out gpu)) {
                            gpu = i;
                        }
                        else {
                            gpu = gpu - kernelOutput.GpuBaseIndex;
                            if (gpu < 0) {
                                continue;
                            }
                        }
                    }
                    if (kernelOutput.IsMapGpuIndex && !string.IsNullOrWhiteSpace(mineContext.KernelInput.DevicesArg)) {
                        if (mineContext.UseDevices.Length != 0 && mineContext.UseDevices.Length != root.GpuSet.Count && gpu < mineContext.UseDevices.Length) {
                            gpu = mineContext.UseDevices[gpu];
                        }
                    }
                    if (double.TryParse(gpuSpeedText, out double gpuSpeed)) {
                        double gpuSpeedL = gpuSpeed.FromUnitSpeed(gpuSpeedUnit);
                        gpuSpeeds.SetCurrentSpeed(gpu, gpuSpeedL, isDual, now);
                    }
                }
                string totalSpeedPattern = kernelOutput.DualTotalSpeedPattern;
                if (isDual) {
                    totalSpeedPattern = kernelOutput.DualTotalSpeedPattern;
                }
                if (string.IsNullOrEmpty(totalSpeedPattern)) {
                    // 求和分算力
                    double speed = isDual? gpuSpeeds.Where(a => a.Gpu.Index != NTMinerRoot.GpuAllId).Sum(a => a.DualCoinSpeed.Value) 
                                         : gpuSpeeds.Where(a => a.Gpu.Index != NTMinerRoot.GpuAllId).Sum(a => a.MainCoinSpeed.Value);
                    gpuSpeeds.SetCurrentSpeed(NTMinerRoot.GpuAllId, speed, isDual, now);
                }
            }
        }
        #endregion

        #region PickTotalShare
        private static void PickTotalShare(INTMinerRoot root, string input, IKernelOutput kernelOutput, ICoin coin, bool isDual) {
            string totalSharePattern = kernelOutput.TotalSharePattern;
            if (isDual) {
                totalSharePattern = kernelOutput.DualTotalSharePattern;
            }
            if (string.IsNullOrEmpty(totalSharePattern)) {
                return;
            }
            Regex regex = VirtualRoot.GetRegex(totalSharePattern);
            var match = regex.Match(input);
            if (match.Success) {
                string totalShareText = match.Groups[NTKeyword.TotalShareGroupName].Value;
                int totalShare;
                if (int.TryParse(totalShareText, out totalShare)) {
                    ICoinShare share = root.CoinShareSet.GetOrCreate(coin.GetId());
                    root.CoinShareSet.UpdateShare(coin.GetId(), acceptShareCount: totalShare - share.RejectShareCount, rejectShareCount: null, now: DateTime.Now);
                }
            }
        }
        #endregion

        #region PickAcceptShare
        private static void PickAcceptShare(INTMinerRoot root, string input, IKernelOutput kernelOutput, ICoin coin, bool isDual) {
            string acceptSharePattern = kernelOutput.AcceptSharePattern;
            if (isDual) {
                acceptSharePattern = kernelOutput.DualAcceptSharePattern;
            }
            if (string.IsNullOrEmpty(acceptSharePattern)) {
                return;
            }
            Regex regex = VirtualRoot.GetRegex(acceptSharePattern);
            var match = regex.Match(input);
            if (match.Success) {
                string acceptShareText = match.Groups[NTKeyword.AcceptShareGroupName].Value;
                int acceptShare;
                if (int.TryParse(acceptShareText, out acceptShare)) {
                    root.CoinShareSet.UpdateShare(coin.GetId(), acceptShareCount: acceptShare, rejectShareCount: null, now: DateTime.Now);
                }
            }
        }
        #endregion

        #region PicFoundOneShare
        private static void PicFoundOneShare(INTMinerRoot root, IMineContext mineContext, string input, string preline, IKernelOutput kernelOutput) {
            string foundOneShare = kernelOutput.FoundOneShare;
            if (string.IsNullOrEmpty(foundOneShare)) {
                return;
            }
            if (foundOneShare.Contains("\n")) {
                input = preline + "\n" + input;
            }
            Regex regex = VirtualRoot.GetRegex(foundOneShare);
            var match = regex.Match(input);
            if (match.Success) {
                string gpuText = match.Groups[NTKeyword.GpuIndexGroupName].Value;
                if (!string.IsNullOrEmpty(gpuText)) {
                    if (int.TryParse(gpuText, out int gpuIndex)) {
                        if (kernelOutput.IsMapGpuIndex && !string.IsNullOrWhiteSpace(mineContext.KernelInput.DevicesArg)) {
                            if (mineContext.UseDevices.Length != 0 && mineContext.UseDevices.Length != root.GpuSet.Count && gpuIndex < mineContext.UseDevices.Length) {
                                gpuIndex = mineContext.UseDevices[gpuIndex];
                            }
                        }
                        root.GpusSpeed.IncreaseFoundShare(gpuIndex);
                    }
                }
            }
        }
        #endregion

        #region PicGotOneIncorrectShare
        private static void PicGotOneIncorrectShare(INTMinerRoot root, IMineContext mineContext, string input, string preline, IKernelOutput kernelOutput) {
            string pattern = kernelOutput.GpuGotOneIncorrectShare;
            if (string.IsNullOrEmpty(pattern)) {
                return;
            }
            if (pattern.Contains("\n")) {
                input = preline + "\n" + input;
            }
            Regex regex = VirtualRoot.GetRegex(pattern);
            var match = regex.Match(input);
            if (match.Success) {
                string gpuText = match.Groups[NTKeyword.GpuIndexGroupName].Value;
                if (!string.IsNullOrEmpty(gpuText)) {
                    if (int.TryParse(gpuText, out int gpuIndex)) {
                        if (kernelOutput.IsMapGpuIndex && !string.IsNullOrWhiteSpace(mineContext.KernelInput.DevicesArg)) {
                            if (mineContext.UseDevices.Length != 0 && mineContext.UseDevices.Length != root.GpuSet.Count && gpuIndex < mineContext.UseDevices.Length) {
                                gpuIndex = mineContext.UseDevices[gpuIndex];
                            }
                        }
                        root.GpusSpeed.IncreaseIncorrectShare(gpuIndex);
                    }
                }
            }
        }
        #endregion

        #region PickAcceptOneShare
        private static void PickAcceptOneShare(INTMinerRoot root, IMineContext mineContext, string input, string preline, IKernelOutput kernelOutput, ICoin coin, bool isDual) {
            string acceptOneShare = kernelOutput.AcceptOneShare;
            if (isDual) {
                acceptOneShare = kernelOutput.DualAcceptOneShare;
            }
            if (string.IsNullOrEmpty(acceptOneShare)) {
                return;
            }
            if (acceptOneShare.Contains("\n")) {
                input = preline + "\n" + input;
            }
            Regex regex = VirtualRoot.GetRegex(acceptOneShare);
            var match = regex.Match(input);
            if (match.Success) {
                if (!isDual) {
                    // 决定不支持双挖的单卡份额统计
                    string gpuText = match.Groups[NTKeyword.GpuIndexGroupName].Value;
                    if (!string.IsNullOrEmpty(gpuText)) {
                        if (int.TryParse(gpuText, out int gpuIndex)) {
                            if (kernelOutput.IsMapGpuIndex && !string.IsNullOrWhiteSpace(mineContext.KernelInput.DevicesArg)) {
                                if (mineContext.UseDevices.Length != 0 && mineContext.UseDevices.Length != root.GpuSet.Count && gpuIndex < mineContext.UseDevices.Length) {
                                    gpuIndex = mineContext.UseDevices[gpuIndex];
                                }
                            }
                            if (string.IsNullOrEmpty(kernelOutput.FoundOneShare)) {
                                root.GpusSpeed.IncreaseFoundShare(gpuIndex);
                            }
                            root.GpusSpeed.IncreaseAcceptShare(gpuIndex);
                        }
                    }
                }
                ICoinShare share = root.CoinShareSet.GetOrCreate(coin.GetId());
                root.CoinShareSet.UpdateShare(coin.GetId(), acceptShareCount: share.AcceptShareCount + 1, rejectShareCount: null, now: DateTime.Now);
            }
        }
        #endregion

        #region PickRejectPattern
        private static void PickRejectPattern(INTMinerRoot root, string input, IKernelOutput kernelOutput, ICoin coin, bool isDual) {
            string rejectSharePattern = kernelOutput.RejectSharePattern;
            if (isDual) {
                rejectSharePattern = kernelOutput.DualRejectSharePattern;
            }
            if (string.IsNullOrEmpty(rejectSharePattern)) {
                return;
            }
            Regex regex = VirtualRoot.GetRegex(rejectSharePattern);
            var match = regex.Match(input);
            if (match.Success) {
                string rejectShareText = match.Groups[NTKeyword.RejectShareGroupName].Value;

                int rejectShare;
                if (int.TryParse(rejectShareText, out rejectShare)) {
                    root.CoinShareSet.UpdateShare(coin.GetId(), acceptShareCount: null, rejectShareCount: rejectShare, now: DateTime.Now);
                }
            }
        }
        #endregion

        #region PickRejectOneShare
        private static void PickRejectOneShare(INTMinerRoot root, IMineContext mineContext, string input, string preline, IKernelOutput kernelOutput, ICoin coin, bool isDual) {
            string rejectOneShare = kernelOutput.RejectOneShare;
            if (isDual) {
                rejectOneShare = kernelOutput.DualRejectOneShare;
            }
            if (string.IsNullOrEmpty(rejectOneShare)) {
                return;
            }
            if (rejectOneShare.Contains("\n")) {
                input = preline + "\n" + input;
            }
            Regex regex = VirtualRoot.GetRegex(rejectOneShare);
            var match = regex.Match(input);
            if (match.Success) {
                if (!isDual) {
                    // 决定不支持双挖的单卡份额统计
                    string gpuText = match.Groups[NTKeyword.GpuIndexGroupName].Value;
                    if (!string.IsNullOrEmpty(gpuText)) {
                        if (int.TryParse(gpuText, out int gpuIndex)) {
                            if (kernelOutput.IsMapGpuIndex && !string.IsNullOrWhiteSpace(mineContext.KernelInput.DevicesArg)) {
                                if (mineContext.UseDevices.Length != 0 && mineContext.UseDevices.Length != root.GpuSet.Count && gpuIndex < mineContext.UseDevices.Length) {
                                    gpuIndex = mineContext.UseDevices[gpuIndex];
                                }
                            }
                            if (string.IsNullOrEmpty(kernelOutput.FoundOneShare)) {
                                root.GpusSpeed.IncreaseFoundShare(gpuIndex);
                            }
                            root.GpusSpeed.IncreaseRejectShare(gpuIndex);
                        }
                    }
                }
                ICoinShare share = root.CoinShareSet.GetOrCreate(coin.GetId());
                root.CoinShareSet.UpdateShare(coin.GetId(), null, share.RejectShareCount + 1, DateTime.Now);
            }
        }
        #endregion

        #region PickRejectPercent
        private static void PickRejectPercent(INTMinerRoot root, string input, IKernelOutput kernelOutput, ICoin coin, bool isDual) {
            string rejectPercentPattern = kernelOutput.RejectPercentPattern;
            if (isDual) {
                rejectPercentPattern = kernelOutput.DualRejectPercentPattern;
            }
            if (string.IsNullOrEmpty(rejectPercentPattern)) {
                return;
            }
            Regex regex = VirtualRoot.GetRegex(rejectPercentPattern);
            var match = regex.Match(input);
            string rejectPercentText = match.Groups[NTKeyword.RejectPercentGroupName].Value;
            double rejectPercent;
            if (double.TryParse(rejectPercentText, out rejectPercent)) {
                ICoinShare share = root.CoinShareSet.GetOrCreate(coin.GetId());
                root.CoinShareSet.UpdateShare(coin.GetId(), acceptShareCount: null, rejectShareCount: (int)(share.TotalShareCount * rejectPercent), now: DateTime.Now);
            }
        }
        #endregion
        #endregion
    }
}
