// <copyright file="DefaultClientIdTracker.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// 客户端ID追踪器的默认实现。
    /// </summary>
    public class DefaultClientIdTracker : IClientIdTracker
    {
        private readonly ConcurrentDictionary<string, string> _connectionToClientMap = new ConcurrentDictionary<string, string>();
        private readonly ILogger<DefaultClientIdTracker> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultClientIdTracker"/> class.
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        public DefaultClientIdTracker(ILogger<DefaultClientIdTracker> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public void RegisterClientId(string connectionId, string clientId)
        {
            _connectionToClientMap.AddOrUpdate(connectionId, clientId, (key, oldValue) => clientId);
            _logger.LogDebug("注册客户端ID: {ClientId} 到连接: {ConnectionId}", clientId, connectionId);
        }

        /// <inheritdoc />
        public string GetClientId(string connectionId)
        {
            _connectionToClientMap.TryGetValue(connectionId, out var clientId);
            return clientId ?? "未知客户端";
        }

        /// <inheritdoc />
        public void UnregisterConnection(string connectionId)
        {
            if (_connectionToClientMap.TryRemove(connectionId, out var clientId))
            {
                _logger.LogDebug("移除连接 {ConnectionId} 的客户端ID: {ClientId}", connectionId, clientId);
            }
        }
    }
}