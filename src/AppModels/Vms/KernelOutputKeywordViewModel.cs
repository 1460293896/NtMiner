﻿using NTMiner.Core;
using System;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class KernelOutputKeywordViewModel : ViewModelBase, IKernelOutputKeyword {
        private Guid _id;
        private Guid _kernelOutputId;
        private string _messageType;
        private string _keyword;

        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }
        public ICommand Save { get; private set; }

        public Action CloseWindow { get; set; }

        public KernelOutputKeywordViewModel() {
            if (!Design.IsInDesignMode) {
                throw new InvalidProgramException();
            }
        }

        public KernelOutputKeywordViewModel(IKernelOutputKeyword data) : this(data.GetId()) {
            _kernelOutputId = data.KernelOutputId;
            _messageType = data.MessageType;
            _keyword = data.Keyword;
        }

        public KernelOutputKeywordViewModel(Guid id) {
            _id = id;
            this.Save = new DelegateCommand(() => {
                if (NTMinerRoot.Instance.KernelOutputFilterSet.Contains(this.Id)) {
                    VirtualRoot.Execute(new UpdateKernelOutputKeywordCommand(this));
                }
                else {
                    VirtualRoot.Execute(new AddKernelOutputKeywordCommand(this));
                }
                CloseWindow?.Invoke();
            });
            this.Edit = new DelegateCommand<FormType?>((formType) => {
                VirtualRoot.Execute(new KernelOutputKeywordEditCommand(formType ?? FormType.Edit, this));
            });
            this.Remove = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                this.ShowDialog(new DialogWindowViewModel(message: $"您确定删除{this.Keyword}内核输出关键字吗？", title: "确认", onYes: () => {
                    VirtualRoot.Execute(new RemoveKernelOutputKeywordCommand(this.Id));
                }));
            });
        }

        public Guid GetId() {
            throw new NotImplementedException();
        }

        public Guid Id {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public Guid KernelOutputId {
            get => _kernelOutputId;
            set {
                _kernelOutputId = value;
                OnPropertyChanged(nameof(KernelOutputId));
            }
        }

        public string MessageType {
            get => _messageType;
            set {
                _messageType = value;
                OnPropertyChanged(nameof(MessageType));
            }
        }

        public string Keyword {
            get => _keyword;
            set {
                _keyword = value;
                OnPropertyChanged(nameof(Keyword));
            }
        }
    }
}
