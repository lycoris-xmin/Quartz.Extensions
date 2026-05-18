using Lycoris.Quartz.Listener;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;

namespace Lycoris.Quartz
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class QuartzBuilder
    {
        private readonly IServiceCollection services;

        /// <summary>
        /// 线程池个数（默认为：10个）
        /// </summary>
        public int ThreadCount { get; set; } = 10;

        /// <summary>
        /// 调度器实例名称
        /// </summary>
        public string InstanceName { get; set; } = "QuartzScheduler";

        /// <summary>
        /// 表前缀（用于ADO JobStore）
        /// </summary>
        public string TablePrefix { get; set; } = "QRTZ_";

        /// <summary>
        /// JobStore 类型全名（如 "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"）
        /// </summary>
        public string JobStoreType { get; set; }

        /// <summary>
        /// 数据源连接字符串名称
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 自定义 Quartz 属性集合
        /// </summary>
        public System.Collections.Specialized.NameValueCollection Properties { get; } = new System.Collections.Specialized.NameValueCollection();

        internal List<Type> SchedulerListenerTypes { get; } = new List<Type>();
        internal List<Type> JobListenerTypes { get; } = new List<Type>();
        internal List<Type> TriggerListenerTypes { get; } = new List<Type>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="services"></param>
        public QuartzBuilder(IServiceCollection services) => this.services = services;

        /// <summary>
        /// 添加调度器监听
        /// </summary>
        public QuartzBuilder AddSchedulerListener<T>() where T : SchedulerListener
        {
            SchedulerListenerTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// 添加调度任务监听
        /// </summary>
        public QuartzBuilder AddJobListener<T>() where T : JobListener
        {
            JobListenerTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// 添加调度触发器监听
        /// </summary>
        public QuartzBuilder AddTriggerListener<T>() where T : TriggerListener
        {
            TriggerListenerTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// 启用程序启动时自动启动非待机定时任务
        /// 默认：<see langword="false"/> 
        /// 禁用状态下，手动启动调度器 <see cref="IQuartzSchedulerCenter.StartScheduleAsync"/> 并执行 <see cref="IQuartzSchedulerCenter.ManualRunNonStandbyJobsAsync"/> 启动待机任务
        /// </summary>
        public bool EnableRunStandbyJobOnApplicationStart { get; set; } = false;
    }
}
