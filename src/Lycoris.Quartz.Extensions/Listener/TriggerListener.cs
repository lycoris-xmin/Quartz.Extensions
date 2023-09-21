using Quartz;
using System.Threading;
using System.Threading.Tasks;

namespace Lycoris.Quartz.Extensions.Listener
{
    /// <summary>
    /// 
    /// </summary>
    public class TriggerListener : ITriggerListener
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual string Name => "TriggerListener";

        /// <summary>
        /// 完成时执行
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <param name="triggerInstructionCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// 要不要放弃job
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
