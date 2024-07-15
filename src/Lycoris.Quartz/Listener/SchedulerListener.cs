using Quartz;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.Quartz.Listener
{
    /// <summary>
    /// 
    /// </summary>
    public class SchedulerListener : ISchedulerListener
    {
        /// <summary>
        /// 添加任务时触发
        /// </summary>
        /// <param name="jobDetail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 删除任务时触发
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 暂停单个任务时触发
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobPaused(JobKey jobKey, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 启动单个任务时触发
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobResumed(JobKey jobKey, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 部署任务时触发
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 分组任务暂停时触发
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobsPaused(string jobGroup, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 分组任务恢复时触发
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobsResumed(string jobGroup, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 任务被卸载时触发
        /// </summary>
        /// <param name="triggerKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 调度中心运行期间触发异常时触发
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="cause"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 调度作用处于 <see langword="Standby"/> 模式下触发
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task SchedulerInStandbyMode(CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 调度中心被停止时触发
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task SchedulerShutdown(CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 调度中心触发停止时触发
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task SchedulerShuttingdown(CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 调度中心启动后触发
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task SchedulerStarted(CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 调度中心启动时触发
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task SchedulerStarting(CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task SchedulingDataCleared(CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 触发器已完成工作任务时触发
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 触发器暂停时触发
        /// </summary>
        /// <param name="triggerKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 触发器恢复时触发
        /// </summary>
        /// <param name="triggerKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 触发器暂停时触发
        /// </summary>
        /// <param name="triggerGroup"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggersPaused(string triggerGroup, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 触发器恢复时触发
        /// </summary>
        /// <param name="triggerGroup"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggersResumed(string triggerGroup, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
