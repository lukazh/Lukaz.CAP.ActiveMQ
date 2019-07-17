using System;
using System.Threading.Tasks;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Processor.States;
using Microsoft.Extensions.Logging;
using Apache.NMS;
using DotNetCore.CAP;

namespace Lukaz.CAP.ActiveMQ
{
    /// <summary>
    /// Sender for ActiveMQ message publishing
    /// </summary>
    internal sealed class ActiveMQPublishMessageSender : BasePublishMessageSender
    {
        private readonly IConnectionPool _connectionPool;
        private readonly ILogger _logger;

        public ActiveMQPublishMessageSender(ILogger<ActiveMQPublishMessageSender> logger, CapOptions options,
            IStorageConnection connection, IConnectionPool connectionPool, IStateChanger stateChanger)
            : base(logger, options, connection, stateChanger)
        {
            _logger = logger;
            _connectionPool = connectionPool;
        }

        /// <summary>
        /// Publish message to a topic
        /// </summary>
        /// <param name="keyName">Topic name</param>
        /// <param name="content">Message content</param>
        /// <returns>Publish result</returns>
        public override Task<OperateResult> PublishAsync(string keyName, string content)
        {
            var session = _connectionPool.Rent();
            try
            {
                var destination = session.GetTopic(keyName);
                using (IMessageProducer producer = session.CreateProducer(destination))
                {
                    var message = session.CreateTextMessage(content);
                    producer.Send(message);
                }

                var returned = _connectionPool.Return(session);
                if (!returned)
                {
                    session.Dispose();
                }
                _logger.LogDebug($"ActiveMQ topic message [{keyName}] has been published. Body: {content}");

                return Task.FromResult(OperateResult.Success);
            }
            catch (Exception ex)
            {
                session.Dispose();

                var wapperEx = new PublisherSentFailedException(ex.Message, ex);
                var errors = new OperateError
                {
                    Code = ex.HResult.ToString(),
                    Description = ex.Message
                };
                return Task.FromResult(OperateResult.Failed(wapperEx, errors));
            }
        }
    }
}