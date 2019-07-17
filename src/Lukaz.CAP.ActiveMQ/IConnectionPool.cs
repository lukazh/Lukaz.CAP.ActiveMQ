using Apache.NMS;

namespace Lukaz.CAP.ActiveMQ
{
    /// <summary>
    /// Connect pool interface
    /// </summary>
    public interface IConnectionPool
    {
        /// <summary>
        /// Get connection.
        /// </summary>
        /// <returns>IConnection instance</returns>
        IConnection GetConnection();

        /// <summary>
        /// Rent a session from the pool.
        /// </summary>
        /// <returns>Session</returns>
        ISession Rent();

        /// <summary>
        /// Return a session to the pool.
        /// </summary>
        /// <param name="session">Session to return</param>
        /// <returns><code>true</code> if returned successfully, else <code>false</code></returns>
        bool Return(ISession session);
    }
}