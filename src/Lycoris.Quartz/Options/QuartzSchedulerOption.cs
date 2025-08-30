using Lycoris.Quartz.Constant;
using System;

namespace Lycoris.Quartz.Options
{
    /// <summary>
    /// 
    /// </summary>
    public class QuartzSchedulerOption
    {
        /// <summary>
        /// 
        /// </summary>
        public Type JobType { get; internal set; }

        /// <summary>
        /// 待机任务
        /// </summary>
        public bool Standby { get; set; }

        /// <summary>
        /// 作业名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string JobGroup { get; set; } = QuartzConstant.JOB_DEFAULT_GROUP;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTimeOffset BeginTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// 定时设置(Cron)
        /// </summary>
        public string Cron { get; set; } = "";

        /// <summary>
        /// 执行次数（默认无限循环）
        /// </summary>
        public int RunTimes { get; set; }

        /// <summary>
        /// 执行间隔时间,单位秒（如果有Cron,则IntervalSecond失效）
        /// </summary>
        public int IntervalSecond { get; set; }

        /// <summary>
        /// 触发器类型
        /// </summary>
        public QuartzTriggerEnum Trigger { get; set; }

        /// <summary>
        /// 任务备注
        /// </summary>
        public string Remark { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public string JsonMap { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public string Args { get; set; }

        /// <summary>
        /// Cron
        /// 如果错过了某次调度（比如程序不在运行，或调度器尚未启动），那么启动后立即执行一次任务，然后按原计划继续执行。
        /// </summary>
        public bool CronRunOnProceed { get; set; } = false;
    }
}
