//-----------------------------------------------------------------------
// <copyright file="ClientIdLoggingMiddleware.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Middleware
{
    /// <summary>
    /// 客户端ID日志中间件，在日志中包含客户端ID信息。
    /// </summary>
    public class ClientIdLoggingMiddleware : IFtpMiddleware
    {
        private readonly ILogger<ClientIdLoggingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientIdLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        public ClientIdLoggingMiddleware(ILogger<ClientIdLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task InvokeAsync(FtpContext context, FtpRequestDelegate next)
        {
            // 获取客户端ID特性
            var clientIdFeature = context.Connection.Features.Get<IClientIdFeature>();
            var clientId = clientIdFeature?.ClientId ?? "未知";

            // 获取连接信息
            var connectionFeature = context.Connection.Features.Get<IConnectionFeature>();
            var connectionInfo = connectionFeature?.RemoteEndPoint?.ToString() ?? "未知连接";

            // 使用作用域添加客户端ID到日志上下文
            using (_logger.BeginScope(new { ClientId = clientId, ConnectionInfo = connectionInfo }))
            {
                _logger.LogDebug("处理来自客户端 {ClientId} 的请求", clientId);
                
                // 调用下一个中间件
                await next(context);
            }
        }
    }
}