using Quartz;
using System;

namespace Lycoris.Quartz
{
    /// <summary>
    /// 
    /// </summary>
    public class JobDetailModel
    {
        /// <summary>
        /// 任务唯一标识符
        /// </summary>
        public string JobKey { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 任务分组
        /// </summary>
        public string JobGroup { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string JobDescription { get; set; }

        /// <summary>
        /// 触发器类型
        /// </summary>
        public QuartzTriggerEnum TriggerType { get; set; }

        /// <summary>
        /// 触发器状态
        /// </summary>
        public TriggerState Status { get; set; }

        /// <summary>
        /// CRON表达式
        /// </summary>
        public string Cron { get; set; }

        /// <summary>
        /// 间隔秒数
        /// </summary>
        public int IntervalSeconds { get; set; }

        /// <summary>
        /// 设置次数
        /// </summary>
        public int RunTimes { get; set; }

        /// <summary>
        /// 执行次数
        /// </summary>
        public long RunCount { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 上一次执行时间
        /// </summary>
        public DateTime? PreviousFireTime { get; set; }

        /// <summary>
        /// 下一次执行时间
        /// </summary>
        public DateTime? NextFireTime { get; set; }
    }
}
