﻿using System;

namespace NTMiner.MinerServer {
    public interface IServerMessage : IEntity<Guid> {
        Guid Id { get; }
        string Provider { get; }
        string MessageType { get; }
        string Content { get; }
        DateTime Timestamp { get; }
    }
}
