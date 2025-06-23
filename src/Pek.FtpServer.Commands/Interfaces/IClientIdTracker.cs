// <copyright file="IClientIdTracker.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Interface for tracking client IDs associated with FTP connections.
    /// </summary>
    public interface IClientIdTracker
    {
        /// <summary>
        /// Registers a client ID for a connection.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="clientId">The client ID.</param>
        void RegisterClientId(string connectionId, string clientId);

        /// <summary>
        /// Gets the client ID for a connection.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <returns>The client ID if found, otherwise a default value.</returns>
        string GetClientId(string connectionId);

        /// <summary>
        /// Unregisters a connection and its associated client ID.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="clientId">The client ID.</param>
        void UnregisterConnection(string connectionId);
    }
}