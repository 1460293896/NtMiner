﻿using NTMiner.Core;
using System;
using System.Windows.Media;

namespace NTMiner.Vms {
    public class SpeedViewModel : ViewModelBase, ISpeed {
        private double _value;
        private DateTime _speedOn;
        private string _speedValueText = "0.0";
        private string _speedUnit = "H/s";
        private string _speedText = "0.0 H/s";
        private int _foundShare;
        private int _acceptShare;
        private int _rejectShare;
        private int _incorrectShare;

        public SpeedViewModel(ISpeed speed) {
            Value = speed.Value;
            SpeedOn = speed.SpeedOn;
            _foundShare = speed.FoundShare;
            _acceptShare = speed.AcceptShare;
            _rejectShare = speed.RejectShare;
            _incorrectShare = speed.IncorrectShare;
        }

        public void UpdateSpeed(double value, DateTime speedOn) {
            this.Value = value;
            this.SpeedOn = speedOn;
        }

        public void Reset() {
            this.Value = 0;
            this.SpeedOn = DateTime.Now;
            this.FoundShare = 0;
            this.AcceptShare = 0;
            this.RejectShare = 0;
            this.IncorrectShare = 0;
        }

        public double Value {
            get => _value;
            set {
                if (_value != value) {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                    value.ToUnitSpeedText(out string speedValueText, out string speedUnit);
                    this.SpeedValueText = speedValueText;
                    this.SpeedUnit = speedUnit;
                    this.SpeedText = $"{speedValueText} {speedUnit}";
                }
            }
        }

        public string SpeedText {
            get => _speedText;
            private set {
                _speedText = value;
                OnPropertyChanged(nameof(SpeedText));
            }
        }

        public string SpeedValueText {
            get => _speedValueText;
            private set {
                _speedValueText = value;
                OnPropertyChanged(nameof(SpeedValueText));
            }
        }

        public string SpeedUnit {
            get => _speedUnit;
            private set {
                _speedUnit = value;
                OnPropertyChanged(nameof(SpeedUnit));
            }
        }

        public DateTime SpeedOn {
            get {
                return _speedOn;
            }
            set {
                if (_speedOn != value) {
                    _speedOn = value;
                    OnPropertyChanged(nameof(SpeedOn));
                    OnPropertyChanged(nameof(LastSpeedOnText));
                    OnPropertyChanged(nameof(LastSpeedOnForeground));
                }
            }
        }

        public int FoundShare {
            get => _foundShare;
            set {
                _foundShare = value;
                OnPropertyChanged(nameof(FoundShare));
            }
        }

        public int AcceptShare {
            get { return _acceptShare; }
            set {
                _acceptShare = value;
                OnPropertyChanged(nameof(AcceptShare));
            }
        }

        public int RejectShare {
            get => _rejectShare;
            set {
                _rejectShare = value;
                OnPropertyChanged(nameof(RejectShare));
            }
        }

        public int IncorrectShare {
            get { return _incorrectShare; }
            set {
                _incorrectShare = value;
                OnPropertyChanged(nameof(IncorrectShare));
            }
        }

        public string LastSpeedOnText {
            get {
                if (!NTMinerRoot.Instance.IsMining || SpeedOn <= Timestamp.UnixBaseTime) {
                    return string.Empty;
                }
                TimeSpan timeSpan = DateTime.Now - SpeedOn;
                if (timeSpan.Days >= 1) {
                    return timeSpan.Days + "天前";
                }
                if (timeSpan.Hours > 0) {
                    return timeSpan.Hours + " 小时前";
                }
                if (timeSpan.Minutes > 2) {
                    return timeSpan.Minutes + "分前";
                }
                return (int)timeSpan.TotalSeconds + "秒前";
            }
        }

        public SolidColorBrush LastSpeedOnForeground {
            get {
                if (SpeedOn.AddSeconds(120) < DateTime.Now) {
                    return Util.RedBrush;
                }
                return Util.BlackBrush;
            }
        }
    }
}
