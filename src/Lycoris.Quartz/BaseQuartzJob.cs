using Lycoris.Quartz.Constant;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Lycoris.Quartz
{
    /// <summary>
    /// [PersistJobDataAfterExecution] 这一次的结果作为值传给下一次
    /// [DisallowConcurrentExecution] 只有上一个任务完成才会执行下一次任务
    /// </summary>
    public abstract class BaseQuartzJob : IJob
    {
        /// <summary>
        /// 任务运行JobTraceId
        /// </summary>
        protected string JobTraceId { get; private set; } = "";

        /// <summary>
        /// 任务名称
        /// </summary>
        protected string JobName { get; private set; } = "";

        /// <summary>
        /// 调度任务上下文
        /// </summary>
        protected IJobExecutionContext Context { get; private set; }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="JobContext"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext JobContext)
        {
            Context = JobContext;

            // 任务名称
            JobName = JobContext.GetJobName();

            // 任务唯一标识Id
            JobTraceId = SetJobTraceId(Context);
            Context.AddJobDataMap(QuartzConstant.TRACE_ID, JobTraceId);

            // 清除上一次的错误提示信息
            if (Context.JobDetail.JobDataMap.ContainsKey(QuartzConstant.EXCEPTION))
                Context.JobDetail.JobDataMap[QuartzConstant.EXCEPTION] = null;

            // 查看是否时间到期
            var endTime = Context.GetEndTime();
            // 如果到期则停止任务
            if (endTime.HasValue && endTime <= DateTime.Now)
            {
                await Context.Scheduler.PauseJob(new JobKey(JobContext.JobDetail.Key.Name, JobContext.JobDetail.Key.Group));
                return;
            }

            try
            {
                await DoWorkAsync();
            }
            catch (Exception ex)
            {
                if (ex is JobExecutionException)
                    throw;

                Context.AddJobException(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="JobContext"></param>
        /// <returns></returns>
        public virtual string SetJobTraceId(IJobExecutionContext JobContext) => Guid.NewGuid().ToString("N");

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <returns></returns>
        protected abstract Task DoWorkAsync();
    }
}
