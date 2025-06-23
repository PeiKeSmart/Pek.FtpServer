// <copyright file="ClientIdExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for client ID functionality.
    /// </summary>
    public static class ClientIdExtensions
    {
        /// <summary>
        /// Adds client ID tracking support to the FTP server.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddClientIdTracking(this IServiceCollection services)
        {
            // Register the default client ID tracker
            services.AddSingleton<IClientIdTracker, DefaultClientIdTracker>();

            // Register the client ID logging middleware
            services.AddSingleton<IFtpCommandMiddleware, ClientIdLoggingMiddleware>();

            // Register the client ID command handler
            services.AddSingleton<ClientIdCommandHandler>();

            return services;
        }

        /// <summary>
        /// Gets the client ID for the specified FTP connection.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <returns>The client ID if available, otherwise "Unknown".</returns>
        public static string GetClientId(this IFtpConnection connection)
        {
            var clientIdFeature = connection.Features.Get<IClientIdFeature>();
            return clientIdFeature?.ClientId ?? "Unknown";
        }

        /// <summary>
        /// Sets the client ID for the specified FTP connection.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <param name="clientId">The client ID to set.</param>
        public static void SetClientId(this IFtpConnection connection, string clientId)
        {
            var clientIdFeature = connection.Features.Get<IClientIdFeature>();
            if (clientIdFeature == null)
            {
                clientIdFeature = new ClientIdFeature();
                connection.Features.Set<IClientIdFeature>(clientIdFeature);
            }

            clientIdFeature.ClientId = clientId;
        }

        /// <summary>
        /// Checks if a client ID has been set for the specified FTP connection.
        /// </summary>
        /// <param name="connection">The FTP connection.</param>
        /// <returns>True if a client ID has been set, otherwise false.</returns>
        public static bool HasClientId(this IFtpConnection connection)
        {
            var clientIdFeature = connection.Features.Get<IClientIdFeature>();
            return clientIdFeature != null && !string.IsNullOrEmpty(clientIdFeature.ClientId);
        }
    }
}