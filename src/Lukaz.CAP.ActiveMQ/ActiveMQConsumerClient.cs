using System;
using System.Collections.Generic;
using System.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;
using DotNetCore.CAP;

namespace Lukaz.CAP.ActiveMQ
{
    /// <summary>
    /// Consumer Client for ActiveMQ.
    /// Subscribers with same group Id share a client.
    /// </summary>
    internal sealed class ActiveMQConsumerClient : IConsumerClient
    {
        /// <summary>
        /// Group Id.
        /// </summary>
        private readonly string _groupId;
        /// <summary>
        /// Options.
        /// </summary>
        private readonly ActiveMQOptions _ActiveMQOptions;

        /// <summary>
        /// ActiveMQ session.
        /// </summary>
        private readonly ISession _session;
        /// <summary>
        /// ActiveMQ consumer.
        /// </summary>
        private IMessageConsumer _consumer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="connectionPool">Connection pool for session creation</param>
        /// <param name="options">Options</param>
        public ActiveMQConsumerClient(string groupId, IConnectionPool connectionPool, ActiveMQOptions options)
        {
            _groupId = groupId;
            _ActiveMQOptions = options;

            _session = connectionPool.GetConnection().CreateSession(AcknowledgementMode.Transactional);
        }

        /// <summary>
        /// Message received event handler.
        /// </summary>
        public event EventHandler<MessageContext> OnMessageReceived;

        /// <summary>
        /// Logging event handler.
        /// </summary>
        public event EventHandler<LogMessageEventArgs> OnLog;

        /// <summary>
        /// Server address
        /// </summary>
        public string ServersAddress => _ActiveMQOptions.HostName;

        /// <summary>
        /// Subscribe to a set of topics to the message queue
        /// </summary>
        /// <param name="topics">Topics</param>
        public void Subscribe(IEnumerable<string> topics)
        {
            if (topics == null)
            {
                throw new ArgumentNullException(nameof(topics));
            }
            // filter duplicate topics
            var topicSet = new HashSet<string>();
            foreach (var topic in topics)
            {
                topicSet.Add(topic);
            }
            var name = string.Join(",", topicSet).Replace('#', '*');
            var destination = new ActiveMQTopic(name);
            _consumer = _session.CreateConsumer(destination);
        }

        /// <summary>
        /// Start listening messages
        /// </summary>
        /// <param name="timeout">Timeout for one time receiving</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
        {
            // keep listening till cancelled
            while (true)
            {
                ITextMessage message;
                try
                {
                    message = _consumer.Receive(timeout) as ITextMessage;
                }
                catch (Exception e)
                {
                    throw new BrokerConnectionException(e);
                }
                if (message != null)
                {
                    var mc = new MessageContext
                    {
                        Group = _groupId,
                        Name = (message.NMSDestination as ActiveMQTopic).TopicName,
                        Content = message.Text
                    };
                    OnMessageReceived?.Invoke(this, mc);
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Commit transaction.
        /// </summary>
        public void Commit()
        {
            _session.Commit();
        }

        /// <summary>
        /// Roll back transaction.
        /// </summary>
        public void Reject()
        {
            _session.Rollback();
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            _consumer.Dispose();
            _session.Dispose();
        }
    }
}