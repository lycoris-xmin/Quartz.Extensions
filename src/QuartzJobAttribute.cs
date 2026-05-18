using Lycoris.Quartz.Constant;
using System;

namespace Lycoris.Quartz
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class QuartzJobAttribute : Attribute
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; private set; }

        /// <summary>
        /// 任务分组
        /// </summary>
        public string JobGroup { get; set; } = QuartzConstant.JOB_DEFAULT_GROUP;

        /// <summary>
        /// 触发器类型
        /// </summary>
        public QuartzTriggerEnum Trigger { get; set; } = QuartzTriggerEnum.SIMPLE;

        /// <summary>
        /// 定时秒数
        /// </summary>
        public int IntervalSecond { get; set; } = 1;

        /// <summary>
        /// 执行次数
        /// 默认无限循环
        /// </summary>
        public int RunTimes { get; set; } = 0;

        /// <summary>
        /// Cron表达式
        /// </summary>
        public string Cron { get; set; } = "";

        /// <summary>
        /// 待机任务 (待机任务指程序运行时不会启动的任务，需要手动执行启动)
        /// 默认：<see langword="false"/>
        /// </summary>
        public bool Standby { get; set; } = false;

        /// <summary>
        /// CRON定时器
        /// 如果错过了某次调度（比如程序不在运行，或调度器尚未启动），那么启动后立即执行一次任务，然后按原计划继续执行。
        /// </summary>
        public bool CronRunOnProceed { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="JobName"></param>
        public QuartzJobAttribute(string JobName)
        {
            this.JobName = JobName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="JobName"></param>
        /// <param name="JobGroup"></param>
        public QuartzJobAttribute(string JobName, string JobGroup)
        {
            this.JobName = JobName;
            this.JobGroup = JobGroup;
        }
    }
}
