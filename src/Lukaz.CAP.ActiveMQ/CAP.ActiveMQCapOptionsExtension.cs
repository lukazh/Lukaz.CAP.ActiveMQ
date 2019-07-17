using System;
using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;

namespace Lukaz.CAP.ActiveMQ
{
    /// <summary>
    /// Cap options extension for ActiveMQ
    /// </summary>
    internal sealed class ActiveMQCapOptionsExtension : ICapOptionsExtension
    {
        /// <summary>
        /// Option configuration action
        /// </summary>
        private readonly Action<ActiveMQOptions> _configure;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configure">Option configuration action</param>
        public ActiveMQCapOptionsExtension(Action<ActiveMQOptions> configure)
        {
            _configure = configure;
        }

        /// <summary>
        /// Add services of ActiveMQ for CAP
        /// </summary>
        /// <param name="services"></param>
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<CapMessageQueueMakerService>();

            var options = new ActiveMQOptions();
            _configure?.Invoke(options);
            services.AddSingleton(options);

            services.AddSingleton<IConsumerClientFactory, ActiveMQConsumerClientFactory>();
            services.AddSingleton<IConnectionPool, ConnectionPool>();
            services.AddSingleton<IPublishExecutor, ActiveMQPublishMessageSender>();
            services.AddSingleton<IPublishMessageSender, ActiveMQPublishMessageSender>();
        }
    }
}