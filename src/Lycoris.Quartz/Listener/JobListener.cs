using Lycoris.Quartz.Extensions.Constant;
using Quartz;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.Quartz.Extensions.Listener
{
    /// <summary>
    /// 
    /// </summary>
    public class JobListener : IJobListener
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual string Name => "JobListener";

        /// <summary>
        /// 调度器将要执行任务，但被触发器拒绝时，调用该方法
        /// </summary>
        /// <param name="jobContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobExecutionVetoed(IJobExecutionContext jobContext, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 调度任务将要被执行时，调用该方法
        /// </summary>
        /// <param name="jobContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobToBeExecuted(IJobExecutionContext jobContext, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 任务执行完毕后，调用该方法
        /// </summary>
        /// <param name="jobContext"></param>
        /// <param name="jobException"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task JobWasExecuted(IJobExecutionContext jobContext, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            var jsonMap = jobContext.GetJobDataMap(QuartzConstant.JSON_MAP);

            if (jsonMap == QuartzConstant.ONCE_JOB)
            {
                await jobContext.Scheduler.DeleteJob(jobContext.JobDetail.Key);
            }
        }
    }
}
