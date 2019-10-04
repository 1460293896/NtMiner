﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NTMiner {
    public static class MainAssemblyInfo {
        public const string Version = "2.6.6";
        public const string Build = "5";
        public const string Tag = "蛮吉";
        public const string MinerJsonBucket = "https://minerjson.oss-cn-beijing.aliyuncs.com/";
        public const string Copyright = "Copyright ©  NTMiner";

        public static readonly Version CurrentVersion;
        public static readonly string CurrentVersionTag = string.Empty;
        public static readonly string TempDirFullName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NTMiner");
        public static readonly string ServerJsonFileName;
        public static readonly string ServerVersionJsonFileFullName;

        public static string OfficialServerHost { get; private set; } = "server.ntminer.com";
        public static string HomeDirFullName { get; private set; } = TempDirFullName;
        public static readonly string RootLockFileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "home.lock");
        public static readonly string RootConfigFileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "home.config");
        public static readonly bool IsLocalHome;

        public static void SetOfficialServerHost(string host) {
            OfficialServerHost = host;
        }

        public static void SetHomeDirFullName(string dirFullName) {
            HomeDirFullName = dirFullName;
        }

        static MainAssemblyInfo() {
            Assembly mainAssembly = Assembly.GetEntryAssembly();
            // 单元测试时为null
            if (mainAssembly != null) {
                CurrentVersion = mainAssembly.GetName().Version;
                var description = (AssemblyDescriptionAttribute)mainAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), inherit: false).FirstOrDefault();
                CurrentVersionTag = description?.Description;
                ServerJsonFileName = $"server{CurrentVersion.Major}.0.0.json";
            }
            else {
                ServerJsonFileName = $"server2.0.0.json";
            }
            if (!File.Exists(RootLockFileFullName)) {
                if (File.Exists(RootConfigFileFullName)) {
                    HomeDirFullName = AppDomain.CurrentDomain.BaseDirectory;
                    IsLocalHome = true;
                }
                else if (!Directory.Exists(HomeDirFullName)) {
                    Directory.CreateDirectory(HomeDirFullName);
                }
            }
            else {
                HomeDirFullName = AppDomain.CurrentDomain.BaseDirectory;
                IsLocalHome = true;
            }
            if (IsLocalHome) {
                if (HomeDirFullName.EndsWith("\\")) {
                    HomeDirFullName = HomeDirFullName.Substring(0, HomeDirFullName.Length - 1);
                }
            }
            ServerVersionJsonFileFullName = Path.Combine(HomeDirFullName, ServerJsonFileName);
        }

        public static void ExtractManifestResource(this Assembly assembly, Type type, string name, string saveFileFuleName) {
            using (var stream = assembly.GetManifestResourceStream(type, name)) {
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                File.WriteAllBytes(saveFileFuleName, data);
            }
        }
    }
}
