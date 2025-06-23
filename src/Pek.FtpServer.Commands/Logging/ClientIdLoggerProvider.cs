//-----------------------------------------------------------------------
// <copyright file="ClientIdLoggerProvider.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Threading;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

using Microsoft.Extensions.Logging;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// 客户端ID日志上下文提供器，为所有日志添加客户端ID信息。
    /// </summary>
    public class ClientIdLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerProvider _innerProvider;
        private readonly ConcurrentDictionary<string, ClientIdLogger> _loggers = new ConcurrentDictionary<string, ClientIdLogger>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientIdLoggerProvider"/> class.
        /// </summary>
        /// <param name="innerProvider">内部日志提供器。</param>
        public ClientIdLoggerProvider(ILoggerProvider innerProvider)
        {
            _innerProvider = innerProvider;
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name =>
                new ClientIdLogger(_innerProvider.CreateLogger(name), name));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _innerProvider?.Dispose();
            _loggers.Clear();
        }
    }

    /// <summary>
    /// 带客户端ID上下文的日志记录器。
    /// </summary>
    public class ClientIdLogger : ILogger
    {
        private readonly ILogger _innerLogger;
        private readonly string _categoryName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientIdLogger"/> class.
        /// </summary>
        /// <param name="innerLogger">内部日志记录器。</param>
        /// <param name="categoryName">日志类别名称。</param>
        public ClientIdLogger(ILogger innerLogger, string categoryName)
        {
            _innerLogger = innerLogger;
            _categoryName = categoryName;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => _innerLogger.BeginScope(state);

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            // 尝试从当前线程上下文获取客户端ID
            var clientId = GetCurrentClientId();
            var connectionInfo = GetCurrentConnectionInfo();

            if (!string.IsNullOrEmpty(clientId) && ShouldEnhanceLog())
            {
                // 增强日志消息，添加客户端ID信息
                var originalMessage = formatter(state, exception);
                var enhancedMessage = $"[Client: {clientId}] {originalMessage}";
                
                // 创建增强的formatter
                var enhancedFormatter = new Func<TState, Exception, string>((s, ex) => enhancedMessage);
                
                _innerLogger.Log(logLevel, eventId, state, exception, enhancedFormatter);
            }
            else
            {
                // 使用原始日志
                _innerLogger.Log(logLevel, eventId, state, exception, formatter);
            }
        }

        private string GetCurrentClientId()
        {
            // 尝试从AsyncLocal或其他上下文获取客户端ID
            return ClientIdContext.Current?.ClientId ?? "Unknown";
        }

        private string GetCurrentConnectionInfo()
        {
            return ClientIdContext.Current?.ConnectionInfo ?? "Unknown";
        }

        private bool ShouldEnhanceLog()
        {
            // 只对特定的FTP相关日志类别进行增强
            return _categoryName.StartsWith("FubarDev.FtpServer", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// 客户端ID上下文，用于在异步调用链中传递客户端ID信息。
    /// </summary>
    public class ClientIdContext
    {
        private static readonly AsyncLocal<ClientIdContext> _current = new AsyncLocal<ClientIdContext>();

        /// <summary>
        /// Gets or sets the current client ID context.
        /// </summary>
        public static ClientIdContext Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }

        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the connection information.
        /// </summary>
        public string ConnectionInfo { get; set; }

        /// <summary>
        /// Creates a new scope with the specified client ID and connection info.
        /// </summary>
        /// <param name="clientId">客户端ID。</param>
        /// <param name="connectionInfo">连接信息。</param>
        /// <returns>可释放的作用域。</returns>
        public static IDisposable CreateScope(string clientId, string connectionInfo)
        {
            var previousContext = Current;
            Current = new ClientIdContext
            {
                ClientId = clientId,
                ConnectionInfo = connectionInfo
            };

            return new ContextScope(previousContext);
        }

        private class ContextScope : IDisposable
        {
            private readonly ClientIdContext _previousContext;

            public ContextScope(ClientIdContext previousContext)
            {
                _previousContext = previousContext;
            }

            public void Dispose()
            {
                Current = _previousContext;
            }
        }
    }
}