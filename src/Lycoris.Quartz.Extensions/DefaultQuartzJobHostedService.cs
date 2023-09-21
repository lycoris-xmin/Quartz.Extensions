using Lycoris.Quartz.Extensions.Options;
using Lycoris.Quartz.Extensions.Services;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quartzSchedulerCenter"></param>
        public DefaultQuartzJobHostedService(IQuartzSchedulerCenter quartzSchedulerCenter)
        {
            _quartzSchedulerCenter = quartzSchedulerCenter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var jobTypes = QuartzSchedulderStore.GetAllJobTypes();

            if (jobTypes != null && jobTypes.Any())
            {
                await _quartzSchedulerCenter.StartScheduleAsync();

                foreach (var item in jobTypes)
                {
                    if (item.JobType == null || item.JobSettings == null)
                        continue;

                    QuartzSchedulderStore.HostedJobOptions.Add(item.JobType, new QuartzSchedulerOption()
                    {
                        BeginTime = new DateTime(2000, 1, 1),
                        TriggerType = item.JobSettings.Trigger,
                        Cron = item.JobSettings.Cron,
                        IntervalSecond = item.JobSettings.IntervalSecond,
                        RunTimes = item.JobSettings.RunTimes,
                        JobGroup = string.IsNullOrEmpty(item.JobSettings.JobGroup) ? "unclassified" : item.JobSettings.JobGroup,
                        JobName = item.JobSettings.JobName
                    });
                }

                if (QuartzSchedulderStore.DisabledRunHostedJob)
                    return;

                var addJob = _quartzSchedulerCenter.GetType().GetMethods().Where(x => x.Name == "AddJobAsync" && x.GetParameters().Length == 1).SingleOrDefault() ?? throw new Exception("could not find available AddJobAsync generic method");

                foreach (var item in QuartzSchedulderStore.HostedJobOptions)
                    addJob.MakeGenericMethod(item.Key).Invoke(_quartzSchedulerCenter, new object[] { item.Value });

                QuartzSchedulderStore.HostedJobOptions = new Dictionary<Type, QuartzSchedulerOption>();
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
