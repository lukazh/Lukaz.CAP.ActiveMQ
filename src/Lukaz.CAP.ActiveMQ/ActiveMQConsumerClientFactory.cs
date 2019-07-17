using DotNetCore.CAP;

namespace Lukaz.CAP.ActiveMQ
{
    /// <summary>
    /// Factory to create ActiveMQ consumer client.
    /// </summary>
    internal sealed class ActiveMQConsumerClientFactory : IConsumerClientFactory
    {
        /// <summary>
        /// Connection pool.
        /// </summary>
        private readonly IConnectionPool _connectionPool;
        /// <summary>
        /// Options.
        /// </summary>
        private readonly ActiveMQOptions _ActiveMQOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ActiveMQOptions">Options</param>
        /// <param name="connectionPool">Connection pool</param>
        public ActiveMQConsumerClientFactory(ActiveMQOptions ActiveMQOptions, IConnectionPool connectionPool)
        {
            _ActiveMQOptions = ActiveMQOptions;
            _connectionPool = connectionPool;
        }

        /// <summary>
        /// Create consumer client.
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <returns>Consumer client instance.</returns>
        public IConsumerClient Create(string groupId)
        {
            try
            {
                return new ActiveMQConsumerClient(groupId, _connectionPool, _ActiveMQOptions);
            }
            catch (System.Exception e)
            {
                throw new BrokerConnectionException(e);
            }
        }
    }
}