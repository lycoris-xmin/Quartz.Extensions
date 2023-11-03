using Quartz;
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
        /// 
        /// </summary>
        /// <param name="jobContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobExecutionVetoed(IJobExecutionContext jobContext, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobToBeExecuted(IJobExecutionContext jobContext, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobContext"></param>
        /// <param name="jobException"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobWasExecuted(IJobExecutionContext jobContext, JobExecutionException jobException, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
