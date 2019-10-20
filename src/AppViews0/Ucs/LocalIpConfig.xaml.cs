﻿using NTMiner.Vms;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NTMiner.Views.Ucs {
    public partial class LocalIpConfig : UserControl {
        public static void ShowWindow() {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                Title = "管理本机 IP",
                IconName = "Icon_Ip",
                Width = 450,
                IsDialogWindow = true,
                FooterVisible = Visibility.Collapsed,
                CloseVisible = Visibility.Visible
            }, ucFactory: (window) => {
                var uc = new LocalIpConfig();
                LocalIpConfigViewModel vm = (LocalIpConfigViewModel)uc.DataContext;
                vm.CloseWindow = window.Close;
                uc.ItemsControl.MouseDown += (object sender, MouseButtonEventArgs e)=> {
                    if (e.LeftButton == MouseButtonState.Pressed) {
                        window.DragMove();
                    }
                };
                window.EventPath<LocalIpSetRefreshedEvent>("本机IP集刷新后刷新IP设置页", LogEnum.DevConsole,
                    action: message => {
                        UIThread.Execute(() => {
                            vm.Refresh();
                        });
                    });
                return uc;
            }, fixedSize: true);
        }

        public LocalIpConfigViewModel Vm {
            get {
                return (LocalIpConfigViewModel)this.DataContext;
            }
        }

        private LocalIpConfig() {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            WpfUtil.ScrollViewer_PreviewMouseDown(sender, e);
        }
    }
}
