using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Lycoris.Quartz.Extensions.Services.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class QuartzJobRunner : IJob
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public QuartzJobRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var jobType = context.JobDetail.JobType;
                    var jobService = scope.ServiceProvider.GetRequiredService(jobType);
                    if (jobService == null)
                        return;

                    if (jobService is IJob job)
                        await job.Execute(context);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"execute task:{context.JobDetail.JobType.FullName} failed", ex);
            }
        }
    }
}
