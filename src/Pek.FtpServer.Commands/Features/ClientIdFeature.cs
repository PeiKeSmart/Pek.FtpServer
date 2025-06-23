// <copyright file="ClientIdFeature.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// 客户端ID功能特性的实现
    /// </summary>
    public class ClientIdFeature : IClientIdFeature
    {
        /// <inheritdoc />
        public string? ClientId { get; set; }
    }
}