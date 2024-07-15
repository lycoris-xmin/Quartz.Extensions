using Quartz;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.Quartz.Listener
{
    /// <summary>
    /// 
    /// </summary>
    public class TriggerListener : ITriggerListener
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual string Name { get; } = "TriggerListener";

        /// <summary>
        /// 调度任务触发执行完方法时候，调用该方法
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <param name="triggerInstructionCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 调度任务触发执行方法时候，调用该方法
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 触发器错过触发时候，调用该方法
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 触发器触发后，但是调度中心向监听器确认是否要暂停执行调度任务
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>false-继续执行，true-不执行</returns>
        public virtual Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
