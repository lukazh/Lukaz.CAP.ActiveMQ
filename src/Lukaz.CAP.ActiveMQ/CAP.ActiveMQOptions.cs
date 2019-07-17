using System;
using Apache.NMS;

namespace Lukaz.CAP.ActiveMQ
{
    /// <summary>
    /// Options for setting up ActiveMQ
    /// </summary>
    public class ActiveMQOptions
    {
        /// <summary>
        /// The host to connect to.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The port to connect on. Default to 61616.
        /// </summary>
        public int Port { get; set; } = 61616;

        /// <summary>
        /// Whether use SSL or not. Default to false.
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// Username to use when authenticating to the server.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password to use when authenticating to the server.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets queue message automatic deletion time (in milliseconds). Default 864000000 ms (10 days).
        /// </summary>
        public int QueueMessageExpires { get; set; } = 864000000;

        /// <summary>
        /// ActiveMQ native connection factory options
        /// </summary>
        public Action<NMSConnectionFactory> ConnectionFactoryOptions { get; set; }
    }
}