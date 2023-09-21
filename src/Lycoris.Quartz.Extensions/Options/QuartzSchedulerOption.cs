using Lycoris.Quartz.Extensions.Job;
using System;

namespace Lycoris.Quartz.Extensions.Options
{
    /// <summary>
    /// 
    /// </summary>
    public class QuartzSchedulerOption
    {
        /// <summary>
        /// 作业编号
        /// </summary>
        public string JobKey { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 作业名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string JobGroup { get; set; } = "localjob";

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
        public QuartzTriggerEnum TriggerType { get; set; }

        /// <summary>
        /// 任务备注
        /// </summary>
        public string Remark { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public string JsonMap { get; set; } = "";
    }
}
