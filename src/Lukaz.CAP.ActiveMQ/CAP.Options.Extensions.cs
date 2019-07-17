using System;
using DotNetCore.CAP;
using Lukaz.CAP.ActiveMQ;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Cap options extensions to use ActiveMQ
    /// </summary>
    public static class CapOptionsExtensions
    {
        /// <summary>
        /// Extension to use ActiveMQ with specific host name
        /// </summary>
        /// <param name="options">Cap options</param>
        /// <param name="hostName">Host name to connect on</param>
        /// <returns>Cap options</returns>
        public static CapOptions UseActiveMQ(this CapOptions options, string hostName = "localhost")
        {
            return options.UseActiveMQ(opt => { opt.HostName = hostName; });
        }

        /// <summary>
        /// Extension to use ActiveMQ with configuration action
        /// </summary>
        /// <param name="options">Cap options</param>
        /// <param name="configure">configuration action</param>
        /// <returns>Cap options</returns>
        public static CapOptions UseActiveMQ(this CapOptions options, Action<ActiveMQOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new ActiveMQCapOptionsExtension(configure));

            return options;
        }
    }
}