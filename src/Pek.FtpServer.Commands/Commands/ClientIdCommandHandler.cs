// <copyright file="ClientIdCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Command handler for the CLID command (Client ID).
    /// Allows clients to identify themselves with a custom ID.
    /// </summary>
    [FtpCommandHandlerAttribute("CLID", isAbortable: false, isLoginRequired: false)]
    public class ClientIdCommandHandler : FtpCommandHandler
    {
        private readonly ILogger<ClientIdCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientIdCommandHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ClientIdCommandHandler(ILogger<ClientIdCommandHandler> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var connection = CommandContext?.FtpContext?.Connection;
            if (connection == null)
            {
                return Task.FromResult<IFtpResponse?>(new FtpResponse(500, "Internal server error"));
            }

            var clientId = command.Argument?.Trim();
            if (string.IsNullOrEmpty(clientId))
            {
                return Task.FromResult<IFtpResponse?>(new FtpResponse(501, "CLID requires client ID parameter"));
            }

            // Store the client ID in connection features
            var clientIdFeature = connection.Features.Get<IClientIdFeature>();
            if (clientIdFeature == null)
            {
                clientIdFeature = new ClientIdFeature();
                connection.Features.Set<IClientIdFeature>(clientIdFeature);
            }

            var oldClientId = clientIdFeature.ClientId;
            clientIdFeature.ClientId = clientId;

            // Get connection ID safely
            var connectionId = GetConnectionId(connection);

            _logger.LogInformation(
                "Client ID updated - Connection: {ConnectionId}, Old: {OldClientId}, New: {NewClientId}",
                connectionId,
                oldClientId ?? "None",
                clientId);

            return Task.FromResult<IFtpResponse?>(new FtpResponse(200, $"Client ID set to '{clientId}'"));
        }

        private static string GetConnectionId(IFtpConnection connection)
        {
            // Try to get connection ID from connection features or remote endpoint
            var connectionFeature = connection.Features.Get<IConnectionFeature>();
            return connectionFeature?.RemoteEndPoint?.ToString() ?? "unknown";
        }
    }
}