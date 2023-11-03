using Lycoris.Quartz.Extensions.Options;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.Quartz.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultQuartzJobHostedService : IHostedService
    {
        private readonly IQuartzSchedulerCenter _quartzSchedulerCenter;
        private readonly IEnumerable<QuartzSchedulerOption> _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quartzSchedulerCenter"></param>
        /// <param name="options"></param>
        public DefaultQuartzJobHostedService(IQuartzSchedulerCenter quartzSchedulerCenter, IEnumerable<QuartzSchedulerOption> options)
        {
            _quartzSchedulerCenter = quartzSchedulerCenter;
            _options = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _quartzSchedulerCenter.StartScheduleAsync();

            if (_options != null || _options.Count() > 0)
            {
                foreach (var item in _options.Where(x => x.Standby == false))
                    await _quartzSchedulerCenter.AddJobAsync(item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _quartzSchedulerCenter.StopScheduleAsync();
        }
    }
}
