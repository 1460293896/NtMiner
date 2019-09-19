﻿using NTMiner.Core;
using System.Diagnostics;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class ToolboxViewModel : ViewModelBase {
        public ICommand SwitchRadeonGpu { get; private set; }
        public ICommand AtikmdagPatcher { get; private set; }
        public ICommand NavigateToDriver { get; private set; }
        public ICommand RegCmdHere { get; private set; }
        public ICommand BlockWAU { get; private set; }
        public ICommand Win10Optimize { get; private set; }
        public ICommand EnableWindowsRemoteDesktop { get; private set; }
        public ICommand WindowsAutoLogon { get; private set; }

        public ToolboxViewModel() {
            if (Design.IsInDesignMode) {
                return;
            }
            this.SwitchRadeonGpu = new DelegateCommand(() => {
                if (MinerProfileViewModel.Instance.IsMining) {
                    VirtualRoot.Out.ShowInfo("请先停止挖矿");
                    return;
                }
                this.ShowDialog(message: $"过程大概需要花费5到10秒钟", title: "确认", onYes: () => {
                    VirtualRoot.Execute(new SwitchRadeonGpuCommand(on: true));
                }, onNo: () => {
                    bool isClose = false;
                    this.ShowDialog(message: "关闭计算模式挖矿算力会减半，确定关闭计算模式？", title: "二次确认", onYes: () => {
                        isClose = true;
                        VirtualRoot.Execute(new SwitchRadeonGpuCommand(on: false));
                    }, icon: IconConst.IconConfirm);
                    return isClose;
                }, icon: IconConst.IconConfirm, yesText: "开启计算模式", noText: "关闭计算模式");
            });
            this.AtikmdagPatcher = new DelegateCommand(() => {
                if (MinerProfileViewModel.Instance.IsMining) {
                    VirtualRoot.Out.ShowInfo("请先停止挖矿");
                    return;
                }
                VirtualRoot.Execute(new AtikmdagPatcherCommand());
            });
            this.NavigateToDriver = new DelegateCommand<SysDicItemViewModel>((item) => {
                if (item == null) {
                    return;
                }
                Process.Start(item.Value);
            });
            this.RegCmdHere = new DelegateCommand(() => {
                this.ShowDialog(message: $"确定在windows右键上下文菜单中添加\"命令行\"菜单吗？", title: "确认", onYes: () => {
                    VirtualRoot.Execute(new RegCmdHereCommand());
                }, icon: IconConst.IconConfirm);
            });
            this.BlockWAU = new DelegateCommand(() => {
                this.ShowDialog(message: $"确定禁用Windows系统更新吗？禁用后可在Windows服务中找到Windows Update手动启用。", title: "确认", onYes: () => {
                    VirtualRoot.Execute(new BlockWAUCommand());
                }, icon: IconConst.IconConfirm, helpUrl: "https://www.loserhub.cn/posts/details/91");
            });
            this.Win10Optimize = new DelegateCommand(() => {
                this.ShowDialog(message: $"确定面向挖矿优化windows吗？", title: "确认", onYes: () => {
                    VirtualRoot.Execute(new Win10OptimizeCommand());
                }, icon: IconConst.IconConfirm, helpUrl: "https://www.loserhub.cn/posts/details/83");
            });
            this.EnableWindowsRemoteDesktop = new DelegateCommand(() => {
                VirtualRoot.Execute(new EnableWindowsRemoteDesktopCommand());
            });
            this.WindowsAutoLogon = new DelegateCommand(() => {
                VirtualRoot.Execute(new EnableOrDisableWindowsAutoLoginCommand());
            });
        }

        public SysDicItemViewModel NvidiaDriverWin10 {
            get {
                if (NTMinerRoot.Instance.SysDicItemSet.TryGetDicItem("ThisSystem", "NvidiaDriverWin10", out ISysDicItem item)) {
                    if (AppContext.Instance.SysDicItemVms.TryGetValue(item.GetId(), out SysDicItemViewModel vm)) {
                        return vm;
                    }
                }
                return null;
            }
        }

        public SysDicItemViewModel NvidiaDriverWin7 {
            get {
                if (NTMinerRoot.Instance.SysDicItemSet.TryGetDicItem("ThisSystem", "NvidiaDriverWin7", out ISysDicItem item)) {
                    if (AppContext.Instance.SysDicItemVms.TryGetValue(item.GetId(), out SysDicItemViewModel vm)) {
                        return vm;
                    }
                }
                return null;
            }
        }

        public SysDicItemViewModel NvidiaDriverMore {
            get {
                if (NTMinerRoot.Instance.SysDicItemSet.TryGetDicItem("ThisSystem", "NvidiaDriverMore", out ISysDicItem item)) {
                    if (AppContext.Instance.SysDicItemVms.TryGetValue(item.GetId(), out SysDicItemViewModel vm)) {
                        return vm;
                    }
                }
                return null;
            }
        }

        public SysDicItemViewModel AmdDriverMore {
            get {
                if (NTMinerRoot.Instance.SysDicItemSet.TryGetDicItem("ThisSystem", "AmdDriverMore", out ISysDicItem item)) {
                    if (AppContext.Instance.SysDicItemVms.TryGetValue(item.GetId(), out SysDicItemViewModel vm)) {
                        return vm;
                    }
                }
                return null;
            }
        }

        public bool IsAutoAdminLogon {
            get { return Windows.OS.Instance.IsAutoAdminLogon; }
        }

        public string AutoAdminLogonMessage {
            get {
                if (IsAutoAdminLogon) {
                    return "开机自动登录已启用";
                }
                return "开机自动登录未启用";
            }
        }

        public bool IsRemoteDesktopEnabled {
            get {
                return NTMinerRegistry.GetIsRemoteDesktopEnabled();
            }
        }

        public string RemoteDesktopMessage {
            get {
                if (IsRemoteDesktopEnabled) {
                    return "远程桌面已启用";
                }
                return "远程桌面已禁用";
            }
        }
    }
}
