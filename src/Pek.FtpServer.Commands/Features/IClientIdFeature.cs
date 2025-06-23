// <copyright file="IClientIdFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Feature for storing client identification information.
    /// </summary>
    public interface IClientIdFeature
    {
        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        string? ClientId { get; set; }
    }
}