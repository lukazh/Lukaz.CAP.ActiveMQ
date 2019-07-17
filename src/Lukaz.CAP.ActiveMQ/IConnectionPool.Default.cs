using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace Lukaz.CAP.ActiveMQ
{
    /// <summary>
    /// Default connection pool implementation
    /// </summary>
    public class ConnectionPool : IConnectionPool, IDisposable
    {
        private const int defaultPoolSize = 15;
        private static readonly object _lockObj = new object();

        /// <summary>
        /// activator for connection
        /// </summary>
        private readonly Func<Connection> _connectionActivator;
        private readonly ILogger<ConnectionPool> _logger;
        private readonly ConcurrentQueue<ISession> _pool;
        private Connection _connection;

        private int _count;
        private int _maxSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="options">Options</param>
        public ConnectionPool(ILogger<ConnectionPool> logger, ActiveMQOptions options)
        {
            _logger = logger;
            _maxSize = defaultPoolSize;
            _pool = new ConcurrentQueue<ISession>();
            _connectionActivator = GetConnectionActivator(options);
        }

        /// <summary>
        /// Rent a session from the pool.
        /// </summary>
        /// <returns>Session</returns>
        public ISession Rent()
        {
            lock (_lockObj)
            {
                while (_count > _maxSize)
                {
                    Thread.SpinWait(1);
                }

                if (_pool.TryDequeue(out var session))
                {
                    Interlocked.Decrement(ref _count);
                    Debug.Assert(_count >= 0);
                    return session;
                }

                try
                {
                    session = GetConnection().CreateSession();
                    return session;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "ActiveMQ session create failed!");
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Return a session to the pool.
        /// </summary>
        /// <param name="session">Session to return</param>
        /// <returns><code>true</code> if returned successfully, else <code>false</code></returns>
        public bool Return(ISession session)
        {
            if (Interlocked.Increment(ref _count) <= _maxSize)
            {
                _pool.Enqueue(session);
                return true;
            }

            Interlocked.Decrement(ref _count);
            Debug.Assert(_maxSize == 0 || _pool.Count <= _maxSize);
            return false;
        }

        /// <summary>
        /// Dispose connection and sessions
        /// </summary>
        public void Dispose()
        {
            _maxSize = 0;
            while (_pool.TryDequeue(out var session))
            {
                session.Dispose();
            }
            _connection.Dispose();
        }

        /// <summary>
        /// Get connection.
        /// </summary>
        /// <returns>IConnection instance</returns>
        public IConnection GetConnection()
        {
            if (_connection == null || _connection.TransportFailed)
            {
                lock (_lockObj)
                {
                    if (_connection == null || _connection.TransportFailed)
                    {
                        if (!_pool.IsEmpty)
                        {
                            _maxSize = 0;
                            while (_pool.TryDequeue(out var session))
                            {
                                session.Dispose();
                            }
                            _maxSize = defaultPoolSize;
                        }

                        _connection = _connectionActivator();
                        _connection.ConnectionInterruptedListener +=
                            () => _logger.LogWarning("ActiveMQ client connection interrupted!");
                    }
                }
            }

            return _connection;
        }

        /// <summary>
        /// Get IConnection instance activator with options
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>IConnection instance activator</returns>
        private static Func<Connection> GetConnectionActivator(ActiveMQOptions options)
        {
            var protocol = options.UseSsl ? "ssl" : "tcp";
            var connectUri = new Uri($"activemq:{protocol}://{options.HostName}:{options.Port}");
            var factory = new NMSConnectionFactory(connectUri);
            options.ConnectionFactoryOptions?.Invoke(factory);
            return () =>
            {
                var conn = (Connection)factory.CreateConnection(options.UserName, options.Password);
                conn.Start();
                return conn;
            };
        }
    }
}