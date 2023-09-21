using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;

namespace Lycoris.Quartz.Extensions.Services.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public JobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler) => _serviceProvider.GetService<QuartzJobRunner>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        public void ReturnJob(IJob job) { }
    }
}
