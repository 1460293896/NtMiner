﻿using NTMiner.Notifications;

namespace NTMiner.Vms {
    public static class NotificationMessageManagerExtension {
        public static NotificationMessageBuilder Warning(this NotificationMessageBuilder builder, string message) {
            builder
                .Accent("#1751C3")
                .Background("#FFCC00")
                .Foreground("Black")
                .HasHeader("提醒");
            builder.SetMessage(message);

            return builder;
        }

        public static NotificationMessageBuilder Error(this NotificationMessageBuilder builder, string message) {
            builder
                .Accent("#1751C3")
                .Background("#F15B19")
                .HasHeader("错误");
            builder.SetMessage(message);

            return builder;
        }

        public static NotificationMessageBuilder Success(this NotificationMessageBuilder builder, string header, string message) {
            builder
                .Accent("#1751C3")
                .Foreground("White")
                .Background("Green")
                .HasHeader(header);
            builder.SetMessage(message);

            return builder;
        }
    }
}
