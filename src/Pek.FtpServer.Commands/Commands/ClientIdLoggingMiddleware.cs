// <copyright file="ClientIdLoggingMiddleware.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//

using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Middleware to log client ID with each FTP command and set up context for all related logs.
    /// </summary>
    public class ClientIdLoggingMiddleware : IFtpCommandMiddleware
    {
        private readonly ILogger<ClientIdLoggingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientIdLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ClientIdLoggingMiddleware(ILogger<ClientIdLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task InvokeAsync(FtpExecutionContext context, FtpCommandExecutionDelegate next)
        {
            var connectionId = GetConnectionId(context);
            var command = context.Command.Name;
            var argument = context.Command.Argument;

            // Try to get client ID from connection features
            var clientIdFeature = context.Connection.Features.Get<IClientIdFeature>();
            var clientId = clientIdFeature?.ClientId ?? "Unknown";

            // 创建客户端ID上下文，使所有后续日志都包含客户端ID信息
            using (ClientIdContext.CreateScope(clientId, connectionId))
            {
                _logger.LogInformation(
                    "Executing command {Command} {Argument} - Connection: {ConnectionId}, Client: {ClientId}",
                    command,
                    argument ?? string.Empty,
                    connectionId,
                    clientId);

                await next(context);
            }
        }

        private static string GetConnectionId(FtpExecutionContext context)
        {
            // Try to get connection ID from connection features or remote endpoint
            var connectionFeature = context.Connection.Features.Get<IConnectionFeature>();
            return connectionFeature?.RemoteEndPoint?.ToString() ?? "unknown";
        }
    }
}