using System;

namespace Lycoris.Quartz.Extensions.Job
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class QuartzJobAttribute : Attribute
    {
        private const string DefaultJobGroup = "unclassified";

        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; private set; }

        /// <summary>
        /// 任务分组
        /// </summary>
        public string JobGroup { get; set; } = DefaultJobGroup;

        /// <summary>
        /// 触发器类型
        /// </summary>
        public QuartzTriggerEnum Trigger { get; set; }

        /// <summary>
        /// 定时秒数
        /// </summary>
        public int IntervalSecond { get; set; }

        /// <summary>
        /// 执行次数，默认无限循环
        /// </summary>
        public int RunTimes { get; set; } = 0;

        /// <summary>
        /// Cron表达式
        /// </summary>
        public string Cron { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="JobName"></param>
        public QuartzJobAttribute(string JobName)
        {
            this.JobName = JobName;
        }
    }
}
